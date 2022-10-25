using HSNCustomizations.DAC;
using HSNHighcareCistomizations.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS
{
    public class SMEquipmentMaint_Extension : PXGraphExtension<SMEquipmentMaint>
    {
        public SelectFrom<LUMEquipmentPINCode>
              .Where<LUMEquipmentPINCode.sMEquipmentID.IsEqual<FSEquipment.SMequipmentID.FromCurrent>>
              .View EquipmentPINCodeList;

        public SelectFrom<LUMHSNSetup>.View HSNSetup;

        public override void Initialize()
        {
            base.Initialize();
            var setup = this.HSNSetup.Select().TopFirst;
            this.EquipmentPINCodeList.AllowSelect = setup.GetExtension<LUMHSNSetupExtension>()?.EnableHighcareFunction ?? false;
        }

        #region Event

        public virtual void _(Events.RowPersisting<LUMEquipmentPINCode> e)
        {
            var row = e.Row;
            var doc = Base.EquipmentRecords.Current;
            if (row != null && doc != null && doc.SMEquipmentID != row.SMEquipmentID)
                row.SMEquipmentID = doc?.SMEquipmentID;
        }

        public virtual void _(Events.RowSelected<LUMEquipmentPINCode> e)
        {
            var row = e.Row;
            if (row != null)
            {
                var mapPINCode = GetMappingPINCodeInfo(row.Pincode);
                row.IsActive = mapPINCode?.IsActive;
                row.StartDate = mapPINCode?.StartDate;
                row.EndDate = mapPINCode?.EndDate;
                row.CPriceClassID = mapPINCode?.CPriceClassID;
            }
        }

        public virtual void _(Events.FieldUpdated<LUMEquipmentPINCode.pincode> e)
        {
            var row = e.Row as LUMEquipmentPINCode;
            if (row != null)
            {
                var mapPINCode = GetMappingPINCodeInfo((string)e.NewValue);
                if (mapPINCode == null)
                    e.Cache.RaiseExceptionHandling<LUMEquipmentPINCode.pincode>(e.Row, e.NewValue,
                        new PXSetPropertyException<LUMEquipmentPINCode.pincode>("找不到此PIN Code，請先綁定Customer PIN Code!!", PXErrorLevel.Error));

                if (!row.IsActive.HasValue)
                {
                    object newActive;
                    e.Cache.RaiseFieldDefaulting<LUMEquipmentPINCode.isActive>(row, out newActive);
                    row.IsActive = (bool?)newActive;
                }

                if (!row.StartDate.HasValue)
                {
                    object newStartDate;
                    e.Cache.RaiseFieldDefaulting<LUMEquipmentPINCode.startDate>(row, out newStartDate);
                    row.StartDate = (DateTime?)newStartDate;
                }

                if (!row.EndDate.HasValue)
                {
                    object newEndDate;
                    e.Cache.RaiseFieldDefaulting<LUMEquipmentPINCode.endDate>(row, out newEndDate);
                    row.EndDate = (DateTime?)newEndDate;
                }

                if(string.IsNullOrEmpty(row.CPriceClassID))
                {
                    object newCPriceClassID;
                    e.Cache.RaiseFieldDefaulting<LUMEquipmentPINCode.cPriceClassID>(row,out newCPriceClassID);
                    row.CPriceClassID = (string)newCPriceClassID;
                }
            }
        }

        public virtual void _(Events.FieldDefaulting<LUMEquipmentPINCode.isActive> e)
            => e.NewValue = SelectFrom<LUMCustomerPINCode>
                           .Where<LUMCustomerPINCode.pin.IsEqual<P.AsString>>
                           .View.Select(Base, (e.Row as LUMEquipmentPINCode)?.Pincode).TopFirst?.IsActive;

        public virtual void _(Events.FieldDefaulting<LUMEquipmentPINCode.startDate> e)
            => e.NewValue = SelectFrom<LUMCustomerPINCode>
                           .Where<LUMCustomerPINCode.pin.IsEqual<P.AsString>>
                           .View.Select(Base, (e.Row as LUMEquipmentPINCode)?.Pincode).TopFirst?.StartDate;

        public virtual void _(Events.FieldDefaulting<LUMEquipmentPINCode.endDate> e)
            => e.NewValue = SelectFrom<LUMCustomerPINCode>
                           .Where<LUMCustomerPINCode.pin.IsEqual<P.AsString>>
                           .View.Select(Base, (e.Row as LUMEquipmentPINCode)?.Pincode).TopFirst?.EndDate;

        #endregion

        #region Method

        public LUMCustomerPINCode GetMappingPINCodeInfo(string pinCode)
            => SelectFrom<LUMCustomerPINCode>
              .Where<LUMCustomerPINCode.pin.IsEqual<P.AsString>
                .And<LUMCustomerPINCode.bAccountID.IsEqual<P.AsInt>>>
              .View.Select(Base, pinCode, Base.EquipmentRecords.Current?.OwnerID).TopFirst;

        #endregion

    }
}
