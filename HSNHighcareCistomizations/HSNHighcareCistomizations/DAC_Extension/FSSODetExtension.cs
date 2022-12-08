using HSNCustomizations.DAC;
using HSNHighcareCistomizations.DAC;
using HSNHighcareCistomizations.Descriptor;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.EP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PX.Objects.FS.FSSODet;

namespace PX.Objects.FS
{
    public class FSSODetExtension : PXCacheExtension<FSSODet>
    {
        public static bool IsActive()
        {
            return (SelectFrom<LUMHSNSetup>.View.Select(new PXGraph()).RowCast<LUMHSNSetup>().FirstOrDefault()?.GetExtension<LUMHSNSetupExtension>().EnableHighcareFunction ?? false);
        }

        #region SMEquipmentID
        [PXDBInt]
        [PXUIField(DisplayName = "Target Equipment ID", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [PXUIEnabled(typeof(Where<Current<isTravelItem>, NotEqual<True>>))]
        [PXSelector(typeof(SelectFrom<FSEquipment>
                           .InnerJoin<FSSrvOrdType>.On<FSSrvOrdType.srvOrdType.IsEqual<FSServiceOrder.srvOrdType.FromCurrent>>
                           .LeftJoin<Customer>.On<Customer.bAccountID.IsEqual<FSServiceOrder.customerID.FromCurrent>>
                           .CrossJoin<FSSetup>.SingleTableOnly
                           .Where<FSEquipment.requireMaintenance.IsEqual<True>
                               .And<FSSetup.enableAllTargetEquipment.IsEqual<True>>
                               .And<FSEquipment.ownerID.IsEqual<FSServiceOrder.customerID.FromCurrent>.And<Customer.customerClassID.IsEqual<HighcareClassAttr>>>
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
                           .Where<LUMCustomerPINCode.bAccountID.IsEqual<FSServiceOrder.customerID.FromCurrent>
                             .And<FSEquipment.SMequipmentID.IsEqual<FSSODet.SMequipmentID.FromCurrent>>>
                           .SearchFor<LUMCustomerPINCode.pin>),
                           typeof(LUMCustomerPINCode.cPriceClassID))]
        public virtual string UsrHighcarePINCode { get; set; }
        public abstract class usrHighcarePINCode : PX.Data.BQL.BqlString.Field<usrHighcarePINCode> { }
        #endregion

        #region UsrCPriceClassID
        [PXString]
        [PXDefault(typeof(Search<LUMCustomerPINCode.cPriceClassID,
                           Where<LUMCustomerPINCode.pin, Equal<Current<FSSODetExtension.usrHighcarePINCode>>,
                             And<LUMCustomerPINCode.bAccountID, Equal<Current<FSServiceOrder.customerID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Highcare Price Class", Enabled = false)]
        public virtual string UsrCPriceClassID { get; set; }
        public abstract class usrCPriceClassID : PX.Data.BQL.BqlString.Field<usrCPriceClassID> { }
        #endregion

    }
}
