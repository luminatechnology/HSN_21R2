using HSNCustomizations.DAC;
using HSNHighcareCistomizations.DAC;
using HSNHighcareCistomizations.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.EP;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS
{
    public class AppointmentEntryExt : PXGraphExtension<AppointmentEntry>
    {
        public SelectFrom<v_HighcareServiceHistory>
               .Where<v_HighcareServiceHistory.aptRefNbr.IsNotEqual<FSAppointment.refNbr.FromCurrent>
                 .And<v_HighcareServiceHistory.customerID.IsEqual<FSAppointment.customerID.FromCurrent>>>
               .View HighcareSrvHistory;

        public SelectFrom<LUMServiceScope>
               .Where<LUMServiceScope.cPriceClassID.IsEqual<ServiceScopeFilter.cPriceClassID.FromCurrent>>
               .View SrvScope;

        public PXFilter<ServiceScopeFilter> SrvScopeFilter;

        [PXHidden]
        public SelectFrom<LUMHSNSetup>.View hsnSetup;


        #region Override Method
        public override void Initialize()
        {
            base.Initialize();

            var hsnSetup = SelectFrom<LUMHSNSetup>.View.Select(Base).RowCast<LUMHSNSetup>().FirstOrDefault();

            this.HighcareSrvHistory.AllowDelete = this.HighcareSrvHistory.AllowInsert = this.HighcareSrvHistory.AllowUpdate = false;
            this.HighcareSrvHistory.AllowSelect = hsnSetup.GetExtension<LUMHSNSetupExtension>()?.EnableHighcareFunction ?? false;

            this.SrvScope.AllowDelete = SrvScope.AllowInsert = this.SrvScope.AllowUpdate = false;
            this.SrvScope.AllowSelect = hsnSetup.GetExtension<LUMHSNSetupExtension>()?.EnableHighcareFunction ?? false;
        }
        #endregion

        #region Event

        public virtual void _(Events.FieldUpdated<FSAppointmentDet.SMequipmentID> e, PXFieldUpdated baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);
            if (this.hsnSetup.Select().TopFirst.GetExtension<LUMHSNSetupExtension>()?.EnableHighcareFunction ?? false)
            {
                var doc = Base.AppointmentRecords.Current;
                HighcareHelper helper = new HighcareHelper();
                var pincodeList = helper.GetEquipmentPINCodeList(doc?.CustomerID, (int?)e.NewValue);
                Base.AppointmentDetails.SetValueExt<FSAppointmentDetExtension.usrHighcarePINCode>((FSAppointmentDet)e.Row, pincodeList.FirstOrDefault()?.Pincode);
            }
        }

        public virtual void _(Events.FieldUpdated<FSAppointmentDetExtension.usrHighcarePINCode> e)
        {
            if (this.hsnSetup.Select().TopFirst.GetExtension<LUMHSNSetupExtension>()?.EnableHighcareFunction ?? false)
            {
                GetHighcareDiscount(e);
                if (e.NewValue != null)
                {
                    object newhighcarePriceClass;
                    e.Cache.RaiseFieldDefaulting<FSAppointmentDetExtension.usrCPriceClassID>(e.Row, out newhighcarePriceClass);
                    (e.Row as FSAppointmentDet).GetExtension<FSAppointmentDetExtension>().UsrCPriceClassID = (string)newhighcarePriceClass;
                }
            }
        }

        public virtual void _(Events.RowSelected<FSAppointmentDet> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);
            if (e.Row != null && (this.hsnSetup.Select().TopFirst.GetExtension<LUMHSNSetupExtension>()?.EnableHighcareFunction ?? false))
            {
                object newhighcarePriceClass;
                e.Cache.RaiseFieldDefaulting<FSAppointmentDetExtension.usrCPriceClassID>(e.Row, out newhighcarePriceClass);
                e.Row.GetExtension<FSAppointmentDetExtension>().UsrCPriceClassID = (string)newhighcarePriceClass;
            }
        }

        #endregion

        #region Method

        public void GetHighcareDiscount(Events.FieldUpdated<FSAppointmentDetExtension.usrHighcarePINCode> e)
        {
            var doc = Base.AppointmentRecords.Current;
            if (e.Row is FSAppointmentDet row && row != null && e.NewValue != null && doc != null)
            {
                HighcareHelper helper = new HighcareHelper();
                var itemInfo = InventoryItem.PK.Find(Base, row.InventoryID);
                var customerInfo = Customer.PK.Find(Base, doc.CustomerID);
                if (customerInfo.ClassID != "HIGHCARE")
                    return;
                var _equipment = helper.GetEquipmentInfo(row.SMEquipmentID);
                // 裝置有綁定 未啟用 不計算
                if (_equipment.Status != EPEquipmentStatus.Active)
                    return;
                var currentPINCode = (string)e.NewValue;
                if (string.IsNullOrEmpty(currentPINCode))
                    return;
                var pinCodeInfo = SelectFrom<LUMCustomerPINCode>
                                  .Where<LUMCustomerPINCode.pin.IsEqual<P.AsString>
                                    .And<LUMCustomerPINCode.bAccountID.IsEqual<P.AsInt>>>
                                  .View.Select(Base, currentPINCode, customerInfo.BAccountID)
                                  .RowCast<LUMCustomerPINCode>().ToList()
                                  .Where(x => Base.Accessinfo.BusinessDate?.Date >= x.StartDate?.Date && Base.Accessinfo.BusinessDate?.Date <= x.EndDate?.Date && (x.IsActive ?? false)).FirstOrDefault();

                if (pinCodeInfo == null)
                    return;
                var servicescopeInfo = SelectFrom<LUMServiceScope>
                                .Where<LUMServiceScope.cPriceClassID.IsEqual<P.AsString>
                                  .And<LUMServiceScope.priceClassID.IsEqual<P.AsString>.Or<LUMServiceScope.inventoryID.IsEqual<P.AsInt>>>>
                                .View.Select(Base, pinCodeInfo.CPriceClassID, itemInfo?.PriceClassID, row.InventoryID)
                                .RowCast<LUMServiceScope>().FirstOrDefault();
                if (servicescopeInfo == null)
                    return;
                // Service History
                var usedServiceCountHist = !string.IsNullOrEmpty(servicescopeInfo?.PriceClassID) ?
                                         this.HighcareSrvHistory.Select()
                                             .RowCast<v_HighcareServiceHistory>()
                                             .Where(x => x?.PriceClassID == servicescopeInfo?.PriceClassID && x.Pincode == currentPINCode).Count() :
                                         this.HighcareSrvHistory.Select()
                                             .RowCast<v_HighcareServiceHistory>()
                                             .Where(x => x?.InventoryID == servicescopeInfo?.InventoryID && x.Pincode == currentPINCode).Count();
                // Detail Cache
                var usedServiceCountCache = Base.AppointmentDetails
                                            .Select().RowCast<FSAppointmentDet>()
                                            .Where(x => x != row && x.InventoryID == row.InventoryID &&
                                                        helper.GetEquipmentInfo(x?.SMEquipmentID).GetExtension<FSEquipmentExtension>()?.UsrPINCode == currentPINCode &&
                                                         helper.GetEquipmentInfo(x?.SMEquipmentID).Status == EPEquipmentStatus.Active).Count();
                // 不限次數，直接給折扣
                if (servicescopeInfo.LimitedCount == 0)
                    Base.AppointmentDetails.Cache.SetValueExt<FSAppointmentDet.discPct>(row, (servicescopeInfo?.DiscountPrecent ?? 0));
                // 限制次數，給予折扣
                else if (servicescopeInfo.LimitedCount - usedServiceCountHist - usedServiceCountCache > 0)
                    Base.AppointmentDetails.Cache.SetValueExt<FSAppointmentDet.discPct>(row, (servicescopeInfo?.DiscountPrecent ?? 0));
                // 次數不夠，跳出警示
                else
                    e.Cache.RaiseExceptionHandling<FSAppointmentDet.SMequipmentID>(
                        row,
                        e.NewValue,
                        new PXSetPropertyException<FSAppointmentDet.SMequipmentID>("Limited count for this service has been reached", PXErrorLevel.Warning));
            }
            // 移除Equipment時 還原折扣
            else if (e.NewValue == null)
                Base.AppointmentDetails.Cache.SetValueExt<FSAppointmentDet.discPct>(e.Row, (decimal)0);
        }

        #endregion
    }
}
