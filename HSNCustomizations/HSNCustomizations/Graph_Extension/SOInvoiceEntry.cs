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
            var doc = Base.Document.Current;
            #region [All-Phase2] 1.26.2	Display an Pop-up Error Message when the Amount does not Tally in Credit and that of Original Invoice
            var setup = SelectFrom<LUMHSNSetup>.View.Select(Base).TopFirst;
            if (doc != null && doc.DocType == "CRM" && (setup?.EnableValidationAmountInCreditMemo ?? false))
            {
                decimal totalOrigInvoiceAmount = 0;
                foreach (var line in Base.Transactions.Select().RowCast<ARTran>().Select(x => new { x.OrigInvoiceType, x.OrigInvoiceNbr }).Distinct())
                    totalOrigInvoiceAmount += ARInvoice.PK.Find(Base, line.OrigInvoiceType, line.OrigInvoiceNbr)?.CuryOrigDocAmt ?? 0;
                if (doc.CuryDocBal != totalOrigInvoiceAmount)
                    throw new Exception("The Balance of the Credit Memo is not Equal to the One in the Original Invoice, Please Revise to Further Process/Release");
            }

            #endregion

            #region [All-Phase2] Add a Validation for Qty Available by Warehouse and Location
            // 如果事先Update IN 就不用檢查
            var soOrderShipmentInfo = SelectFrom<SOOrderShipment>
                                      .Where<SOOrderShipment.invoiceNbr.IsEqual<P.AsString>
                                        .And<SOOrderShipment.invoiceType.IsEqual<P.AsString>>>
                                      .View.Select(Base, Base.Document.Current?.RefNbr, Base.Document.Current?.DocType).TopFirst;
            // (Invoice or Debit memo) AND 沒有Update IN 才檢查 
            if ((Base.Document.Current?.DocType == "INV" || Base.Document.Current?.DocType == "DRM") && string.IsNullOrEmpty(soOrderShipmentInfo?.InvtRefNbr))
            {
                foreach (ARTran item in Base.Transactions.Cache.Cached.RowCast<ARTran>())
                {
                    if (item.InventoryID.HasValue)
                    {
                        var inventoryItem = PX.Objects.IN.InventoryItem.PK.Find(Base, item?.InventoryID);
                        var itemclass = PX.Objects.IN.INItemClass.PK.Find(Base, inventoryItem?.ItemClassID);
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
            }
            #endregion

            var releaseResult = baseMethod.Invoke(adapter);
            var tranRows = Base.Transactions.Select().RowCast<ARTran>().ToList().FirstOrDefault();
            if (tranRows != null)
            {
                var fsARTran = FSARTran.PK.Find(Base, doc.DocType, doc.RefNbr, tranRows.LineNbr);
                if (!string.IsNullOrEmpty(fsARTran?.AppointmentRefNbr) && !string.IsNullOrEmpty(fsARTran?.ServiceOrderRefNbr) && !string.IsNullOrEmpty(fsARTran?.SrvOrdType))
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
                // Mark invoice is printed
                Base.Document.Cache.SetValueExt(Base.Document.Current, PX.Objects.CS.Messages.Attribute + "REPRINT", true);
                Base.Save.Press();
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

        #region Action
        public PXAction<ARInvoice> PrintBillingStatement;
        [PXButton(Category = "Printing and Emailing", DisplayOnMainToolbar = false)]
        [PXUIField(DisplayName = "Print Billing Statement", MapEnableRights = PXCacheRights.Select)]
        public virtual IEnumerable printBillingStatement(PXAdapter adapter)
        {
            if (Base.Document.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["DocType"] = Base.Document.Current.DocType;
                parameters["RefNbr"] = Base.Document.Current.RefNbr;
                throw new PXReportRequiredException(parameters, "LM643000", "Print Billing Statement", PXBaseRedirectException.WindowMode.NewWindow);
            }
            return adapter.Get();
        }

        /// <summary>
        /// Print Original Delivery Order (LM643001)
        /// </summary>
        public PXAction<ARInvoice> PrintOriginalDeliveryOrder;
        [PXButton(Category = "Printing and Emailing", DisplayOnMainToolbar = false)]
        [PXUIField(DisplayName = "Print Original Delivery Order", MapEnableRights = PXCacheRights.Select)]
        public virtual IEnumerable printOriginalDeliveryOrder(PXAdapter adapter)
        {
            if (Base.Document.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["DocType"] = Base.Document.Current.DocType;
                parameters["RefNbr"] = Base.Document.Current.RefNbr;
                throw new PXReportRequiredException(parameters, "LM643001", "Print Original Delivery Order", PXBaseRedirectException.WindowMode.NewWindow);
            }
            return adapter.Get();
        }

        /// <summary>
        /// Print Original Tax Invoice for Customer (LM643002)
        /// </summary>
        public PXAction<ARInvoice> PrintOriginalTaxInvoiceforCustomer;
        [PXButton(Category = "Printing and Emailing", DisplayOnMainToolbar = false)]
        [PXUIField(DisplayName = "Print Original Tax Invoice for Customer", MapEnableRights = PXCacheRights.Select)]
        public virtual IEnumerable printOriginalTaxInvoiceforCustomer(PXAdapter adapter)
        {
            if (Base.Document.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["DocType"] = Base.Document.Current.DocType;
                parameters["RefNbr"] = Base.Document.Current.RefNbr;
                throw new PXReportRequiredException(parameters, "LM643002", "Print Original Tax Invoice for Customer", PXBaseRedirectException.WindowMode.NewWindow);
            }
            return adapter.Get();
        }

        /// <summary>
        /// Print Copy Tax Invoice/Delivery Order for Customer (LM643003)
        /// </summary>
        public PXAction<ARInvoice> PrintCopyTaxInvoiceDeliveryOrderForCustomer;
        [PXButton(Category = "Printing and Emailing", DisplayOnMainToolbar = false)]
        [PXUIField(DisplayName = "Print Copy Tax Invoice/Delivery Order for Customer", MapEnableRights = PXCacheRights.Select)]
        public virtual IEnumerable printCopyTaxInvoiceDeliveryOrderForCustomer(PXAdapter adapter)
        {
            if (Base.Document.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["DocType"] = Base.Document.Current.DocType;
                parameters["RefNbr"] = Base.Document.Current.RefNbr;
                throw new PXReportRequiredException(parameters, "LM643003", "Print Copy Tax Invoice/Delivery Order for Customer", PXBaseRedirectException.WindowMode.NewWindow);
            }
            return adapter.Get();
        }

        /// <summary>
        /// Print Original Invoice/Copy Tax Invoice (LM643004)
        /// </summary>
        public PXAction<ARInvoice> PrintOriginalInvoiceCopyTaxInvoice;
        [PXButton(Category = "Printing and Emailing", DisplayOnMainToolbar = false)]
        [PXUIField(DisplayName = "Print Original Invoice/Copy Tax Invoice", MapEnableRights = PXCacheRights.Select)]
        public virtual IEnumerable printOriginalInvoiceCopyTaxInvoice(PXAdapter adapter)
        {
            if (Base.Document.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["DocType"] = Base.Document.Current.DocType;
                parameters["RefNbr"] = Base.Document.Current.RefNbr;
                throw new PXReportRequiredException(parameters, "LM643004", "Print Original Invoice/Copy Tax Invoice", PXBaseRedirectException.WindowMode.NewWindow);
            }
            return adapter.Get();
        }

        /// <summary>
        /// Print Tax Invoice for Finance (LM643005)
        /// </summary>
        public PXAction<ARInvoice> PrintTaxInvoiceforFinance;
        [PXButton(Category = "Printing and Emailing", DisplayOnMainToolbar = false)]
        [PXUIField(DisplayName = "Print Tax Invoice for Finance", MapEnableRights = PXCacheRights.Select)]
        public virtual IEnumerable printTaxInvoiceforFinance(PXAdapter adapter)
        {
            if (Base.Document.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["DocType"] = Base.Document.Current.DocType;
                parameters["RefNbr"] = Base.Document.Current.RefNbr;
                throw new PXReportRequiredException(parameters, "LM643005", "Print Tax Invoice for Finance", PXBaseRedirectException.WindowMode.NewWindow);
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
