using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CN.Compliance.AR.CacheExtensions;
using PX.Objects.FS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO
{
    public class SOInvoiceEntryExt : PXGraphExtension<SOInvoiceEntry>
    {
        #region Delegate Method
        public delegate IEnumerable ReleaseDelegate(PXAdapter adapter);
        [PXOverride]
        public virtual IEnumerable Release(PXAdapter adapter, ReleaseDelegate baseMethod)
        {

            #region [All-Phase2] Add a Validation for Qty Available by Warehouse and Location
            // Invoice and Debit memo 才檢查
            if (Base.Document.Current?.DocType == "INV" || Base.Document.Current?.DocType == "DRM")
            {
                foreach (ARTran item in Base.Transactions.Cache.Cached.RowCast<ARTran>())
                {
                    var inventoryItem = PX.Objects.IN.InventoryItem.PK.Find(Base, item.InventoryID);
                    var itemclass = PX.Objects.IN.INItemClass.PK.Find(Base, inventoryItem.ItemClassID);
                    if (itemclass.StkItem ?? false)
                    {
                        if (item.LocationID == null)
                            throw new PXException($"LocationID can not be empty (InventoryID: {inventoryItem.InventoryCD})");
                        var qtyOnHand = PX.Objects.IN.INLocationStatus.PK.Find(Base, item.InventoryID, item.SubItemID, item.SiteID, item.LocationID)?.QtyOnHand ?? 0;
                        if (item.Qty > qtyOnHand)
                            throw new PXException($"Inventory quantity for {inventoryItem.InventoryCD} in warehouse will go negative.");
                    }
                }
            }
            #endregion

            var releaseResult = baseMethod.Invoke(adapter);
            var doc = Base.Document.Current;
            var tranRows = Base.Transactions.Select().RowCast<ARTran>().ToList().FirstOrDefault();
            if (tranRows != null)
            {
                var fsARTran = FSARTran.PK.Find(Base, doc.DocType, doc.RefNbr, tranRows.LineNbr);
                if (!string.IsNullOrEmpty(fsARTran.AppointmentRefNbr) && !string.IsNullOrEmpty(fsARTran.ServiceOrderRefNbr) && !string.IsNullOrEmpty(fsARTran.SrvOrdType))
                    UpdateAppointmentStageManual(fsARTran.AppointmentRefNbr, fsARTran.ServiceOrderRefNbr, fsARTran.SrvOrdType);
            }
            return releaseResult;
        }

        // [Upgrade Fix]
        public delegate IEnumerable PrintInvoiceDelegate(PXAdapter adapter, string reportID = null);
        [PXOverride]
        public virtual IEnumerable PrintInvoice(PXAdapter adapter, string reportID, PrintInvoiceDelegate baseMethod)
        {
            try
            {
                baseMethod(adapter, reportID);
            }
            catch (Exception ex)
            {
                var prepaymentPrice = Base.Transactions.Select().RowCast<ARTran>().ToList().Where(x => x.CuryUnitPrice < 0).Sum(x => x.CuryUnitPrice) ?? 0;
                var preference = SelectFrom<LUMHSNSetup>.View.Select(Base).RowCast<LUMHSNSetup>().ToList().FirstOrDefault();
                if (preference != null && (preference?.EnableChgInvTypeOnBill ?? false))
                    ((PXReportRequiredException)ex).Parameters.Add("PrepaymentPrice", prepaymentPrice.ToString());
                throw ex;
            }
            return adapter.Get();
        }

        #endregion

        #region Method
        public bool UpdateAppointmentStageManual(string appointmentRefNbr, string serviceOrderRefNbr, string srvType)
        {
            var apptData = FSWorkflowStageHandler.GetCurrentAppointment(srvType, appointmentRefNbr);
            var srvData = FSWorkflowStageHandler.GetCurrentServiceOrder(srvType, serviceOrderRefNbr);
            if (apptData == null || srvData == null)
                return false;

            FSWorkflowStageHandler.InitStageList();
            LUMAutoWorkflowStage autoWFStage = new LUMAutoWorkflowStage();
            // INVOICE01
            autoWFStage = LUMAutoWorkflowStage.UK.Find(new PXGraph(), srvType, nameof(WFRule.INVOICE01), apptData.WFStageID);
            if (autoWFStage != null && autoWFStage.Active == true)
            {
                // update Appointment and Insert log
                FSWorkflowStageHandler.UpdateTargetFormStage(nameof(AppointmentEntry), autoWFStage.NextStage, srvType, appointmentRefNbr, serviceOrderRefNbr);
                FSWorkflowStageHandler.InsertTargetFormHistory(nameof(AppointmentEntry), autoWFStage, srvType, appointmentRefNbr, serviceOrderRefNbr);

                // update ServiceOrder and Insert log
                FSWorkflowStageHandler.UpdateTargetFormStage(nameof(ServiceOrderEntry), autoWFStage.NextStage, srvType, appointmentRefNbr, serviceOrderRefNbr);
                FSWorkflowStageHandler.InsertTargetFormHistory(nameof(ServiceOrderEntry), autoWFStage, srvType, appointmentRefNbr, serviceOrderRefNbr);
                return true;
            }
            else
            {
                PXTrace.WriteWarning("Update Workflow Stage fail!!");
                return false;
            }

        }
        #endregion

    }
}
