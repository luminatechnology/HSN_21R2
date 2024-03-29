﻿using HSNCustomizations.DAC;
using HSNHighcareCistomizations.DAC;
using HSNHighcareCistomizations.Descriptor;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using System.Linq;

namespace PX.Objects.FS
{
    public class FSAppointmentDetExtension : PXCacheExtension<FSAppointmentDet>
    {
        public static bool IsActive()
        {
            return (SelectFrom<LUMHSNSetup>.View.Select(new PXGraph()).RowCast<LUMHSNSetup>().FirstOrDefault()?.GetExtension<LUMHSNSetupExtension>().EnableHighcareFunction ?? false);
        }

        #region SMEquipmentID
        [PXDBInt]
        [PXUIField(DisplayName = "Target Equipment ID", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [PXUIEnabled(typeof(Where<Current<FSAppointmentDet.isTravelItem>, NotEqual<True>>))]
        [PXSelector(typeof(SelectFrom<FSEquipment>
                           .InnerJoin<FSSrvOrdType>.On<FSSrvOrdType.srvOrdType.IsEqual<FSAppointment.srvOrdType.FromCurrent>>
                           .LeftJoin<Customer>.On<Customer.bAccountID.IsEqual<FSAppointment.customerID.FromCurrent>>
                           .CrossJoin<FSSetup>.SingleTableOnly
                           .Where<FSEquipment.requireMaintenance.IsEqual<True>
                               .And<FSSetup.enableAllTargetEquipment.IsEqual<True>>
                               .And<FSEquipment.ownerID.IsEqual<FSAppointment.customerID.FromCurrent>.And<Customer.customerClassID.IsEqual<HighcareClassAttr>>>
                               .Or<Customer.customerClassID.IsNotEqual<HighcareClassAttr>>>
                           .SearchFor<FSEquipment.SMequipmentID>),
                     typeof(FSEquipment.refNbr),
                     typeof(FSEquipment.descr),
                     typeof(FSEquipment.serialNumber),
                     typeof(FSEquipment.ownerType),
                     typeof(FSEquipment.ownerID),
                     typeof(FSEquipment.locationType),
                     typeof(FSEquipment.status),
                     typeof(FSEquipment.branchID),
                     typeof(FSEquipment.branchLocationID),
                     typeof(FSEquipment.inventoryID),
                     typeof(FSEquipment.color),
                     typeof(FSEquipmentExtension.usrEquipAttrAssetNbr),
                     typeof(FSEquipment.registrationNbr),
                     SubstituteKey = typeof(FSEquipment.refNbr))]
        //[PXRestrictor(typeof(Where<FSEquipment.status, Equal<EPEquipmentStatus.EquipmentStatusActive>>),
        //               TX.Messages.EQUIPMENT_IS_INSTATUS, typeof(FSEquipment.status))]
        public virtual int? SMEquipmentID { get; set; }
        #endregion

        #region UsrHighcarePINCode
        [PXDBString(100)]
        [PXUIField(DisplayName = "Highcare PIN Code")]
        [PXSelector(typeof(SelectFrom<LUMCustomerPINCode>
                           .InnerJoin<FSEquipment>.On<LUMCustomerPINCode.bAccountID.IsEqual<FSEquipment.ownerID>>
                           .InnerJoin<LUMEquipmentPINCode>.On<FSEquipment.SMequipmentID.IsEqual<LUMEquipmentPINCode.sMEquipmentID>
                                                          .And<LUMCustomerPINCode.pin.IsEqual<LUMEquipmentPINCode.pincode>>>
                           .Where<LUMCustomerPINCode.bAccountID.IsEqual<FSAppointment.customerID.FromCurrent>
                             .And<FSEquipment.SMequipmentID.IsEqual<FSAppointmentDet.SMequipmentID.FromCurrent>>
                             .And<AccessInfo.businessDate.FromCurrent.IsBetween<LUMCustomerPINCode.startDate, LUMCustomerPINCode.endDate>>>
                           .SearchFor<LUMCustomerPINCode.pin>),
                    typeof(LUMCustomerPINCode.cPriceClassID),
                    typeof(LUMCustomerPINCode.startDate),
                    typeof(LUMCustomerPINCode.endDate))]
        public virtual string UsrHighcarePINCode { get; set; }
        public abstract class usrHighcarePINCode : PX.Data.BQL.BqlString.Field<usrHighcarePINCode> { }
        #endregion

        #region UsrCPriceClassID
        [PXString]
        [PXDefault(typeof(Search<LUMCustomerPINCode.cPriceClassID,
                           Where<LUMCustomerPINCode.pin, Equal<Current<FSAppointmentDetExtension.usrHighcarePINCode>>,
                             And<LUMCustomerPINCode.bAccountID, Equal<Current<FSAppointment.customerID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Highcare Price Class", Enabled = false)]
        public virtual string UsrCPriceClassID { get; set; }
        public abstract class usrCPriceClassID : PX.Data.BQL.BqlString.Field<usrCPriceClassID> { }
        #endregion
    }
}
