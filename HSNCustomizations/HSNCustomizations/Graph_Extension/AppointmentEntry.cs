using PX.SM;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.CR;
using PX.Objects.CR.Standalone;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;

namespace PX.Objects.FS
{
    public class AppointmentEntry_Extension : PXGraphExtension<AppointmentEntry>
    {
        #region Constant String & Classes
        public const string TransferScr = "IN304000";
        public const string ReceiptScr = "IN301000";
        public const string RMAReqAttr = "RMAREQ";
        public const string BrandAttr = "BRAND";

        public class rMAReqAttrID : PX.Data.BQL.BqlString.Constant<rMAReqAttrID>
        {
            public rMAReqAttrID() : base(RMAReqAttr) { }
        }
        #endregion

        #region Selects
        public SelectFrom<LUMAppEventHistory>.Where<LUMAppEventHistory.srvOrdType.IsEqual<FSAppointment.srvOrdType.FromCurrent>
                                                    .And<LUMAppEventHistory.apptRefNbr.IsEqual<FSAppointment.refNbr.FromCurrent>>>.View EventHistory;

        public SelectFrom<INRegister>.Where<INRegister.docType.IsIn<INDocType.transfer, INDocType.receipt>
                                            .And<INRegisterExt.usrSrvOrdType.IsEqual<FSAppointment.srvOrdType.FromCurrent>
                                                 .And<INRegisterExt.usrAppointmentNbr.IsEqual<FSAppointment.refNbr.FromCurrent>>>>.View INRegisterView;

        public SelectFrom<LUMHSNSetup>.View HSNSetupView;
        #endregion

        #region Override Methods
        public override void Initialize()
        {
            base.Initialize();

            Base.menuDetailActions.AddMenuAction(openPartRequest);
            Base.menuDetailActions.AddMenuAction(openPartReceive);
            Base.menuDetailActions.AddMenuAction(openInitiateRMA);
            Base.menuDetailActions.AddMenuAction(openReturnRMA);
            Base.menuDetailActions.AddMenuAction(toggleRMA);

            FSWorkflowStageHandler.InitStageList();
            AddAllStageButton();
        }
        #endregion

        #region Delegate DataView

        protected virtual IEnumerable staffRecords()
        {
            var staffReuslt = StaffSelectionHelper.StaffRecordsDelegate(Base.AppointmentServiceEmployees,
                                                             Base.SkillGridFilter,
                                                             Base.LicenseTypeGridFilter,
                                                             Base.StaffSelectorFilter);
            // 是否篩選Staff
            var isFilter = Base.AppointmentRecords.Current == null ? false : (FSSrvOrdType.PK.Find(Base, Base.AppointmentRecords.Current.SrvOrdType)?.GetExtension<FSSrvOrdTypeExt>()?.UsrStaffFilterByBranch ?? false);
            // Appointment Record
            var apptCurrent = Base.AppointmentRecords.Current;
            // Appointment Branch LocationID
            var branchLocationID = FSServiceOrder.PK.Find(Base, apptCurrent?.SrvOrdType, apptCurrent?.SORefNbr)?.BranchLocationID;
            // BranchLocationID 實際的BranchID
            var currentBranchID = FSBranchLocation.PK.Find(Base, branchLocationID)?.BranchID;
            foreach (BAccountStaffMember staffItem in staffReuslt)
            {
                // 需要篩選Staff 或 Appointment Current Record != null
                if (isFilter && apptCurrent != null)
                {
                    // Employee Info
                    var employeeInfo = EPEmployee.PK.Find(Base, staffItem.BAccountID);
                    if (employeeInfo != null)
                    {
                        // Find Employee Branch
                        var staffBranchID = SelectFrom<PX.Objects.GL.Branch>
                                       .Where<PX.Objects.GL.Branch.bAccountID.IsEqual<P.AsInt>>
                                       .View.SelectSingleBound(Base, null, employeeInfo.ParentBAccountID)
                                       .TopFirst?.BranchID;
                        if (staffBranchID == currentBranchID)
                            yield return staffItem;
                    }
                }
                else
                    yield return staffItem;
            }
        }
        #endregion

        #region Delegate Methods
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            if (Base.AppointmentRecords.Current != null &&
               (SelectFrom<FSSrvOrdType>.Where<FSSrvOrdType.srvOrdType.IsEqual<P.AsString>>.View.Select(Base, Base.AppointmentRecords.Current.SrvOrdType).TopFirst?.GetExtension<FSSrvOrdTypeExt>().UsrEnableEquipmentMandatory ?? false))
            {
                VerifyEquipmentIDMandatory();
            }

            if (Base.AppointmentRecords.Current != null &&
                Base.AppointmentRecords.Current.Status != FSAppointment.status.Values.Closed &&
                HSNSetupView.Select().TopFirst?.EnableHeaderNoteSync == true)
            {
                if (Base.AppointmentRecords.Current.AppointmentID < 0)
                {
                    SyncNoteApptOrSrvOrd(Base, typeof(FSServiceOrder), typeof(FSAppointment));
                }
                else
                {
                    SyncNoteApptOrSrvOrd(Base, typeof(FSAppointment), typeof(FSServiceOrder));
                }
            }

            var isNewData = Base.AppointmentRecords.Cache.Inserted.RowCast<FSAppointment>().Count() > 0;
            // Check Status is Dirty
            var statusDirtyResult = CheckStatusIsDirty(Base.AppointmentRecords.Current);
            // Check Stage is Dirty
            var wfStageDirtyResult = CheckWFStageIsDirty(Base.AppointmentRecords.Current);
            // Detect is New Staff Record & lineType = InventoryItem or Service type
            var newStaffRecords = Base.AppointmentServiceEmployees.Cache.Inserted.RowCast<FSAppointmentEmployee>().ToList();
            // IsNew Detail Record
            FSWorkflowStageHandler.IsNewDetailRecord = Base.AppointmentDetails.Cache.Inserted.RowCast<FSAppointmentDet>().Any(x => x.LineType == "SLPRO" || x.LineType == "SERVI");
            // Set UsrLastSatusModDate if Stage is dirty
            if (wfStageDirtyResult.IsDirty)
                Base.AppointmentRecords.Current.GetExtension<FSAppointmentExt>().UsrLastSatusModDate = PXTimeZoneInfo.Now;

            // 記錄刪除的Details資料[Phase II]
            var detailDeleteRecord = new List<FSAppointmentDet>();
            detailDeleteRecord.AddRange(Base.AppointmentDetails.Cache.Deleted.RowCast<FSAppointmentDet>());

            baseMethod();
            try
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    // Init object
                    bool isDriveStaff = false;
                    FSWorkflowStageHandler.apptEntry = Base;
                    FSWorkflowStageHandler.InitStageList();

                    // insert log if status is change
                    if (statusDirtyResult.IsDirty && !string.IsNullOrEmpty(statusDirtyResult.oldValue))
                        FSWorkflowStageHandler.InsertEventHistoryForStatus(nameof(AppointmentEntry), statusDirtyResult.oldValue, statusDirtyResult.newValue);

                    LUMAutoWorkflowStage autoWFStage = new LUMAutoWorkflowStage();

                    // check staff is driver
                    foreach (var staff in newStaffRecords)
                    {
                        var employee = EPEmployee.PK.Find(Base, staff.EmployeeID);
                        var attr = CSAnswers.PK.Find(Base, employee?.NoteID, "DRIVER");
                        if (attr != null && attr.Value == "1")
                        {
                            isDriveStaff = true;
                            break;
                        }
                    }

                    #region WorkFlower

                    // New Data
                    if (isNewData)
                        autoWFStage = LUMAutoWorkflowStage.PK.Find(Base, Base.AppointmentRecords.Current.SrvOrdType, nameof(WFRule.OPEN01));
                    // Manual Chagne Stage
                    else if (wfStageDirtyResult.IsDirty && wfStageDirtyResult.oldValue.HasValue && wfStageDirtyResult.newValue.HasValue)
                        autoWFStage = new LUMAutoWorkflowStage()
                        {
                            SrvOrdType = Base.AppointmentRecords.Current.SrvOrdType,
                            WFRule = "MANUAL",
                            Active = true,
                            CurrentStage = wfStageDirtyResult.oldValue,
                            NextStage = wfStageDirtyResult.newValue,
                            Descr = "Manual change Stage"
                        };
                    // Staff Drive stage
                    else if (isDriveStaff)
                        autoWFStage = LUMAutoWorkflowStage.PK.Find(Base, Base.AppointmentRecords.Current.SrvOrdType, nameof(WFRule.ASSIGN03));
                    // Workflow
                    else
                        autoWFStage = FSWorkflowStageHandler.AutoWFStageRule(nameof(AppointmentEntry));

                    if (autoWFStage != null && autoWFStage.Active == true)
                        FSWorkflowStageHandler.UpdateWFStageID(nameof(AppointmentEntry), autoWFStage);

                    #endregion

                    // 執行Base Persisted
                    baseMethod();

                    #region [All-Phase2]Sync Delete Details Record with Service Order Details
                    // 判斷Primary Current是否存在(刪除整張單) 並同步Service Order Details
                    if (Base.AppointmentRecords.Current != null && detailDeleteRecord.Count > 0)
                    {
                        PXTrace.WriteInformation($"Delete Service Order Details (count: {detailDeleteRecord.Count})");
                        var srvGraph = PXGraph.CreateInstance<ServiceOrderEntry>();
                        srvGraph.ServiceOrderRecords.Current = srvGraph.ServiceOrderRecords.Search<FSServiceOrder.refNbr>(Base.AppointmentRecords.Current.SORefNbr, Base.AppointmentRecords.Current.SrvOrdType);
                        if (srvGraph.ServiceOrderRecords.Current != null)
                        {
                            foreach (var deletedItem in detailDeleteRecord)
                            {
                                var currentLine = srvGraph.ServiceOrderDetails.Select().RowCast<FSSODet>().FirstOrDefault(x => x.LineNbr == deletedItem.OrigLineNbr && x.InventoryID == deletedItem.InventoryID);
                                if (currentLine != null)
                                    srvGraph.ServiceOrderDetails.Cache.Delete(currentLine);
                            }
                            srvGraph.Save.Press();
                        }
                    }
                    #endregion

                    ts.Complete();
                }
            }
            catch (PXException)
            {
                throw;
            }
        }

        public delegate IEnumerable CloseAppointmentDelegate(PXAdapter adapter);
        [PXOverride]
        public IEnumerable CloseAppointment(PXAdapter adapter, CloseAppointmentDelegate baseMethod)
        {
            if (Base.AppointmentDetails.Select().RowCast<FSAppointmentDet>().Where(x => x.GetExtension<FSAppointmentDetExt>().UsrRMARequired == true &&
                                                                                        x.Status != FSAppointmentDet.status.CANCELED).Count() > 0)
            {
                if (this.INRegisterView.Select().RowCast<INRegister>().Where(x => x.DocType == INDocType.Receipt &&
                                                                                  x.GetExtension<INRegisterExt>().UsrTransferPurp == LUMTransferPurposeType.RMAInit).Count() <= 0)
                {
                    throw new PXException(HSNMessages.NoInitRMARcpt);
                }

                if (this.INRegisterView.Select().RowCast<INRegister>().Where(x => x.DocType == INDocType.Transfer &&
                                                                                  x.GetExtension<INRegisterExt>().UsrTransferPurp == LUMTransferPurposeType.RMARetu).Count() <= 0)
                {
                    throw new PXException(HSNMessages.MustReturnRMA);
                }
            }

            if (this.INRegisterView.Select().RowCast<INRegister>().Where(x => x.Status != INDocStatus.Released).Count() > 0)
            {
                throw new PXException(HSNMessages.InvtTranNoAllRlsd);
            }

            return baseMethod(adapter);
        }

        public delegate IEnumerable StartAppointmentDelegate(PXAdapter adapter);
        [PXOverride]
        public IEnumerable StartAppointment(PXAdapter adapter, StartAppointmentDelegate baseMethod)
        {
            if (Base.AppointmentServiceEmployees.Select().Count <= 0 && Base.ServiceOrderTypeSelected.Current?.GetExtension<FSSrvOrdTypeExt>().UsrOnStaffIsMandStartAppt == true)
            {
                throw new PXException(HSNMessages.StartApptNoStaff);
            }

            return baseMethod(adapter);
        }

        [PXButton]
        [PXUIField(DisplayName = "Run Appointment Billing", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual IEnumerable invoiceAppointment(PXAdapter adapter)
        {
            var doc = Base.AppointmentRecords.Current;
            var prepeymentInfo = Base.ServiceOrderRelated.Current;
            var customerInfo = Customer.PK.Find(Base, doc.CustomerID);
            if (prepeymentInfo?.SOPrepaymentRemaining > 0 && (customerInfo == null || !customerInfo.PrepaymentAcctID.HasValue || !customerInfo.PrepaymentSubID.HasValue))
                throw new PXException("Please maintain the prepayment account for this customer");

            // [All-Phase2] Add a Validation for Qty Available by Warehouse and Location
            var details = Base.AppointmentDetails.Cache.Cached.RowCast<FSAppointmentDet>().Where(x => x.LineType == "SLPRO");
            foreach (var item in details)
            {
                var inventoryInfo = InventoryItem.PK.Find(Base, item?.InventoryID);
                var itemclass = INItemClass.PK.Find(Base, inventoryInfo?.ItemClassID);
                // Line Status != "Cancel" && Stock Item 才做檢核
                if (item?.Status != "CC" && (itemclass?.StkItem ?? false))
                {
                    if (item.LocationID == null)
                        throw new PXException($"LocationID can not be empty (InventoryID: {item.InventoryCD})");
                    var qtyOnHand = INLocationStatus.PK.Find(Base, item.InventoryID, item.SubItemID, item.SiteID, item.LocationID)?.QtyOnHand ?? 0;
                    if (item.ActualQty > qtyOnHand)
                        throw new PXException($"Inventory quantity for {item.InventoryCD} in warehouse will go negative.");
                }
            }
            return Base.InvoiceAppointment(adapter);
        }

        // [All-Phase2] Enable the modification of TAX Amt in Appointment
        public delegate void OpenPostingDocumentDelegate();
        [PXOverride]
        public virtual void openPostingDocument(OpenPostingDocumentDelegate baseMethod)
        {
            try
            {
                baseMethod();
            }
            catch (PXRedirectRequiredException ex)
            {
                // [All-Phase2] Enable the modification of TAX Amt in Appointment
                var hsnSetup = SelectFrom<LUMHSNSetup>.View.Select(Base).TopFirst;
                if (ex.Graph is SOInvoiceEntry && (hsnSetup?.EnableModificationofTaxAmount ?? false))
                {
                    var invoiceGraph = ex.Graph as SOInvoiceEntry;
                    var currentDoc = invoiceGraph.Document.Current;
                    var apptTaxTotal = Base.AppointmentRecords.Current?.CuryTaxTotal;
                    if (currentDoc != null && currentDoc?.Status == ARDocStatus.Hold && apptTaxTotal != null )
                    {
                        // setting Tax
                        invoiceGraph.Taxes.Current = invoiceGraph.Taxes.Select();
                        // System Tax
                        var systemTax = invoiceGraph.Taxes.Current?.CuryTaxAmt ?? 0;
                        invoiceGraph.Taxes.SetValueExt<ARTaxTran.curyTaxAmt>(invoiceGraph.Taxes.Current, apptTaxTotal);
                        invoiceGraph.Taxes.Cache.MarkUpdated(invoiceGraph.Taxes.Current);
                        // setting Document
                        invoiceGraph.Document.SetValueExt<ARInvoice.curyTaxTotal>(invoiceGraph.Document.Current, apptTaxTotal);
                        invoiceGraph.Document.SetValueExt<ARInvoice.curyDocBal>(invoiceGraph.Document.Current, invoiceGraph.Document.Current.CuryDocBal + (apptTaxTotal ?? 0) - systemTax);
                        invoiceGraph.Document.SetValueExt<ARInvoice.curyOrigDocAmt>(invoiceGraph.Document.Current, invoiceGraph.Document.Current.CuryOrigDocAmt + (apptTaxTotal ?? 0) - systemTax);
                        invoiceGraph.Document.Update(invoiceGraph.Document.Current);
                        invoiceGraph.Save.Press();
                    }
                }
                throw ex;
            }
        }

        #region Mandatory overwriting and copying of standard methods, future upgrades and modifications must be followed up
        public delegate bool UpdateServiceOrderWithoutErrorHandlerdelegate(FSAppointment fsAppointmentRow, AppointmentEntry graphAppointmentEntry,
                                                                           object rowInProcessing, PXDBOperation operation, PXTranStatus? tranStatus);
        [PXOverride]
        public bool UpdateServiceOrderWithoutErrorHandler(FSAppointment fsAppointmentRow, AppointmentEntry graphAppointmentEntry,
                                                          object rowInProcessing, PXDBOperation operation, PXTranStatus? tranStatus,
                                                          UpdateServiceOrderWithoutErrorHandlerdelegate baseMethod)
        {
            if (serviceOrderIsAlreadyUpdated == true || Base.SkipServiceOrderUpdate == true || fsAppointmentRow == null || fsAppointmentRow.MustUpdateServiceOrder != true)
            {
                return true;
            }

            // tranStatus is null when the caller is a RowPersisting event.
            if (tranStatus != null && tranStatus == PXTranStatus.Aborted)
            {
                return false;
            }

            bool deletingAppointment = false;
            bool forceAppointmentCheckings = false;
            PXEntryStatus appointmentRowEntryStatus = graphAppointmentEntry.AppointmentRecords.Cache.GetStatus(fsAppointmentRow);

            if (appointmentRowEntryStatus == PXEntryStatus.Deleted)
            {
                // When the Appointment is being deleted, the ServiceOrder is not updated in any RowPersisting event
                // but in the RowPersisted event of FSAppointment.
                if (tranStatus == null
                    || tranStatus != PXTranStatus.Completed
                    || operation != PXDBOperation.Delete
                    || (rowInProcessing is FSAppointment) == false)
                {
                    return true;
                }
                else
                {
                    deletingAppointment = true;
                }
            }

            ServiceOrderEntry _ServiceOrderEntryGraph = null;

            if (_ServiceOrderEntryGraph == null)
            {
                _ServiceOrderEntryGraph = PXGraph.CreateInstance<ServiceOrderEntry>();
            }
            else if (_ServiceOrderEntryGraph.RunningPersist == false)
            {
                _ServiceOrderEntryGraph.Clear();
            }

            if (_ServiceOrderEntryGraph.RunningPersist == false)
            {
                _ServiceOrderEntryGraph.RecalculateExternalTaxesSync = true;
                _ServiceOrderEntryGraph.GraphAppointmentEntryCaller = Base;
            }

            ServiceOrderEntry graphServiceOrderEntry = _ServiceOrderEntryGraph;

            // Variables with short names
            ServiceOrderEntry soGraph = graphServiceOrderEntry;
            AppointmentEntry apptGraph = graphAppointmentEntry;

            FSServiceOrder fsServiceOrderRow = null;

            if (graphServiceOrderEntry.RunningPersist == false)
            {
                graphServiceOrderEntry.ServiceOrderRecords.Current = graphServiceOrderEntry.ServiceOrderRecords
                        .Search<FSServiceOrder.refNbr>(fsAppointmentRow.SORefNbr, fsAppointmentRow.SrvOrdType);
            }

            fsServiceOrderRow = graphServiceOrderEntry.ServiceOrderRecords.Current;

            if (fsServiceOrderRow == null
                || fsServiceOrderRow.SrvOrdType != fsAppointmentRow.SrvOrdType
                || fsServiceOrderRow.RefNbr != fsAppointmentRow.SORefNbr)
            {
                throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(FSServiceOrder)));
            }

            if (deletingAppointment == false)
            {
                if (appointmentRowEntryStatus != PXEntryStatus.Inserted)
                {
                    bool? oldNotStarted = (bool?)graphAppointmentEntry.AppointmentRecords.Cache.GetValueOriginal<FSAppointment.notStarted>(fsAppointmentRow);
                    bool? oldInProcess = (bool?)graphAppointmentEntry.AppointmentRecords.Cache.GetValueOriginal<FSAppointment.inProcess>(fsAppointmentRow);

                    forceAppointmentCheckings = (oldNotStarted == true || oldInProcess == true) != (fsAppointmentRow.NotStarted == true || fsAppointmentRow.InProcess == true);

                    if (fsAppointmentRow.Finished != (bool?)graphAppointmentEntry.AppointmentRecords.Cache.GetValueOriginal<FSAppointment.finished>(fsAppointmentRow))
                    {
                        forceAppointmentCheckings = true;
                    }
                }
                else
                {
                    if ((fsAppointmentRow.NotStarted == true || fsAppointmentRow.InProcess == true) && fsAppointmentRow.Finished == false)
                    {
                        forceAppointmentCheckings = true;
                    }
                }

                if (forceAppointmentCheckings == false)
                {
                    forceAppointmentCheckings = IsThereAnySODetReferenceBeingDeleted<FSAppointmentDet.sODetID>(graphAppointmentEntry.AppointmentDetails.Cache);
                }
            }

            if (graphServiceOrderEntry.ServiceOrderRecords.Current.CuryInfoID < 0)
            {
                graphServiceOrderEntry.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.curyInfoID>(graphServiceOrderEntry.ServiceOrderRecords.Current, fsAppointmentRow.CuryInfoID);
            }

            PXEntryStatus serviceOrderStatus = Base.ServiceOrderRelated.Cache.GetStatus(Base.ServiceOrderRelated.Current);
            if (deletingAppointment == true
                || forceAppointmentCheckings == true
                || serviceOrderStatus != PXEntryStatus.Notchanged)
            {
                // There is not need to copy ServiceOrderRelated's current values
                // to graphServiceOrderEntry.ServiceOrderRecords' current
                // because this graph (AppointmentEntry) just finished persisting the ServiceOrderRelated record.
                // However we mark the graphServiceOrderEntry.ServiceOrderRecords' current record as Updated
                // in order to graphServiceOrderEntry runs all its validations.
                graphServiceOrderEntry.ServiceOrderRecords.Cache.SetStatus(fsServiceOrderRow, PXEntryStatus.Updated);
                graphServiceOrderEntry.ServiceOrderRecords.Cache.IsDirty = true;
            }

            Base.UpdateRelatedApptSummaryFieldsByDeletedLines(soGraph, graphAppointmentEntry);

            if (deletingAppointment == true)
            {
                // Save the ServiceOrder to persist the new related appointment summary values.
                try
                {
                    graphServiceOrderEntry.GraphAppointmentEntryCaller = null;
                    graphServiceOrderEntry.ForceAppointmentCheckings = true;

                    graphServiceOrderEntry.Save.Press();

                    serviceOrderIsAlreadyUpdated = true;
                }
                catch (Exception ex)
                {
                    ReplicateServiceOrderExceptions(graphAppointmentEntry, graphServiceOrderEntry, ex);

                    Base.VerifyIfTransactionWasAborted(graphServiceOrderEntry, ex);

                    return false;
                }
                finally
                {
                    graphServiceOrderEntry.ForceAppointmentCheckings = false;
                }
            }
            else
            {
                PXResultset<FSAppointmentDet> apptLines = graphAppointmentEntry.AppointmentDetails.Select();
                List<FSApptLineSplit> processedApptSplits = new List<FSApptLineSplit>();
                graphServiceOrderEntry.SkipTaxCalcTotals = true;

                if (serviceOrderStatus == PXEntryStatus.Inserted
                    && graphAppointmentEntry.AppointmentRecords.Current.BillServiceContractID != null)
                {
                    graphServiceOrderEntry.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.billServiceContractID>
                            (graphServiceOrderEntry.ServiceOrderRecords.Current, graphAppointmentEntry.AppointmentRecords.Current.BillServiceContractID);
                }

                foreach (FSAppointmentDet fsAppointmentDetRow in apptLines.Where(x => ((FSAppointmentDet)x).LineType != ID.LineType_ALL.PICKUP_DELIVERY))
                {
                    PXEntryStatus lineStatus = graphAppointmentEntry.AppointmentDetails.Cache.GetStatus(fsAppointmentDetRow);

                    if (lineStatus != PXEntryStatus.Inserted
                            && lineStatus != PXEntryStatus.Updated)
                    {
                        continue;
                    }

                    apptGraph.AppointmentDetails.Current = fsAppointmentDetRow;

                    InsertUpdateSODet(graphAppointmentEntry.AppointmentDetails.Cache, fsAppointmentDetRow, graphServiceOrderEntry.ServiceOrderDetails, fsAppointmentRow);

                    List<FSApptLineSplit> apptSplits = apptGraph.Splits.Select().RowCast<FSApptLineSplit>().Where(r => string.IsNullOrEmpty(r.LotSerialNbr) == false).ToList();

                    UpdateSrvOrdSplits(apptGraph, fsAppointmentDetRow, apptSplits, soGraph);

                    processedApptSplits.AddRange(apptSplits);

                    graphServiceOrderEntry.UpdateRelatedApptSummaryFields(Base.AppointmentDetails.Cache, fsAppointmentDetRow, graphServiceOrderEntry.ServiceOrderDetails.Cache, fsAppointmentDetRow.FSSODetRow);
                }

                try
                {
                    graphServiceOrderEntry.GraphAppointmentEntryCaller = graphAppointmentEntry;
                    graphServiceOrderEntry.ForceAppointmentCheckings = forceAppointmentCheckings;

                    if (insertingServiceOrder == true)
                    {
                        graphServiceOrderEntry.Answers.Select();
                        graphServiceOrderEntry.Answers.CopyAttributes(graphServiceOrderEntry, graphServiceOrderEntry.ServiceOrderRecords.Current, graphAppointmentEntry, graphAppointmentEntry.AppointmentRecords.Current, true);
                        insertingServiceOrder = false;
                    }

                    if (graphServiceOrderEntry.ForceAppointmentCheckings == true || graphServiceOrderEntry.IsDirty == true)
                    {
                        graphServiceOrderEntry.SkipTaxCalcTotals = false;
                        ServiceOrderEntry.SalesTax salesExtSrvOrd = graphServiceOrderEntry.GetExtension<ServiceOrderEntry.SalesTax>();
                        salesExtSrvOrd.CalcTaxes();

                        if (graphServiceOrderEntry.RunningPersist == false)
                        {
                            graphServiceOrderEntry.SelectTimeStamp();
                            graphServiceOrderEntry.SkipTaxCalcAndSave();
                            graphServiceOrderEntry.RecalculateExternalTaxes();
                        }
                    }

                    serviceOrderIsAlreadyUpdated = true;
                }
                catch (Exception ex)
                {
                    ReplicateServiceOrderExceptions(graphAppointmentEntry, graphServiceOrderEntry, ex);
                    Base.VerifyIfTransactionWasAborted(graphServiceOrderEntry, ex);
                    return false;
                }
                finally
                {
                    graphServiceOrderEntry.GraphAppointmentEntryCaller = null;
                    graphServiceOrderEntry.ForceAppointmentCheckings = false;
                    graphServiceOrderEntry.SkipTaxCalcTotals = false;
                }

                // Fill the dictionary to update FSAppointmentDet with FSSODet values in its RowPersisting event
                if (ApptLinesWithSrvOrdLineUpdated == null)
                {
                    ApptLinesWithSrvOrdLineUpdated = new Dictionary<FSAppointmentDet, FSSODet>();
                }
                else
                {
                    ApptLinesWithSrvOrdLineUpdated.Clear();
                }

                foreach (FSAppointmentDet fsAppointmentDetRow in apptLines)
                {
                    if (fsAppointmentDetRow.FSSODetRow != null)
                    {
                        ApptLinesWithSrvOrdLineUpdated[fsAppointmentDetRow] = fsAppointmentDetRow.FSSODetRow;
                    }
                }

                // Fill the dictionary to update FSApptLineSplit with FSSODetSplit values in its RowPersisting event
                if (ApptSplitsWithSrvOrdSplitUpdated == null)
                {
                    ApptSplitsWithSrvOrdSplitUpdated = new Dictionary<FSApptLineSplit, FSSODetSplit>();
                }
                else
                {
                    ApptSplitsWithSrvOrdSplitUpdated.Clear();
                }

                foreach (FSApptLineSplit apptSplit in processedApptSplits)
                {
                    if (apptSplit.FSSODetSplitRow != null)
                    {
                        ApptSplitsWithSrvOrdSplitUpdated[apptSplit] = apptSplit.FSSODetSplitRow;
                    }
                }
            }

            if (deletingAppointment == false)
            {
                // Update the ServiceOrderRelated values with the new Service Order values.
                if (Base.ServiceOrderRelated.Current != null && graphServiceOrderEntry.ServiceOrderRecords.Current != null
                    && Base.ServiceOrderRelated.Current.SOID == graphServiceOrderEntry.ServiceOrderRecords.Current.SOID)
                {
                    foreach (var fieldName in Base.ServiceOrderRelated.Cache.Fields)
                    {
                        if (fieldName == nameof(Base.ServiceOrderRelated.Current.AppointmentsCompletedCntr) ||
                            fieldName == nameof(Base.ServiceOrderRelated.Current.AppointmentsCompletedOrClosedCntr))
                        {
                            continue;
                        }

                        Base.ServiceOrderRelated.Cache.SetValue(Base.ServiceOrderRelated.Current, fieldName,
                                                                graphServiceOrderEntry.ServiceOrderRecords.Cache.GetValue(graphServiceOrderEntry.ServiceOrderRecords.Current, fieldName));
                    }
                }
            }

            return true;
        }
        public bool serviceOrderIsAlreadyUpdated = false, insertingServiceOrder = false;

        private Dictionary<FSAppointmentDet, FSSODet> _ApptLinesWithSrvOrdLineUpdated = null;
        protected Dictionary<FSAppointmentDet, FSSODet> ApptLinesWithSrvOrdLineUpdated
        {
            get
            {
                return _ApptLinesWithSrvOrdLineUpdated;
            }
            set
            {
                _ApptLinesWithSrvOrdLineUpdated = value;
            }
        }

        private Dictionary<FSApptLineSplit, FSSODetSplit> _ApptSplitsWithSrvOrdSplitUpdated = null;
        protected Dictionary<FSApptLineSplit, FSSODetSplit> ApptSplitsWithSrvOrdSplitUpdated
        {
            get
            {
                return _ApptSplitsWithSrvOrdSplitUpdated;
            }
            set
            {
                _ApptSplitsWithSrvOrdSplitUpdated = value;
            }
        }

        public int ReplicateServiceOrderExceptions(AppointmentEntry graphAppointmentEntry, ServiceOrderEntry graphServiceOrderEntry, Exception exception)
        {
            int errorCount = 0;

            errorCount += SharedFunctions.ReplicateCacheExceptions(graphAppointmentEntry.AppointmentRecords.Cache,
                                                                   graphAppointmentEntry.AppointmentRecords.Current,
                                                                   graphAppointmentEntry.ServiceOrderRelated.Cache,
                                                                   graphAppointmentEntry.ServiceOrderRelated.Current,
                                                                   graphServiceOrderEntry.ServiceOrderRecords.Cache,
                                                                   graphServiceOrderEntry.ServiceOrderRecords.Current);

            foreach (FSAppointmentDet fsAppointmentDetRow in graphAppointmentEntry.AppointmentDetails.Select())
            {
                if (fsAppointmentDetRow.FSSODetRow != null)
                {
                    errorCount += SharedFunctions.ReplicateCacheExceptions(graphAppointmentEntry.AppointmentDetails.Cache,
                                                                           fsAppointmentDetRow,
                                                                           graphServiceOrderEntry.ServiceOrderDetails.Cache,
                                                                           fsAppointmentDetRow.FSSODetRow);
                }
            }

            if (errorCount == 0)
            {
                throw PXException.PreserveStack(exception);
            }

            return errorCount;
        }

        public bool IsThereAnySODetReferenceBeingDeleted<SODetIDType>(PXCache cache) where SODetIDType : IBqlField
        {
            // Check if some line is being deleted.
            foreach (object row in cache.Deleted)
            {
                return true;
            }

            // Check if some line is changing its SODet reference.
            foreach (object row in cache.Updated)
            {
                if ((int?)cache.GetValue<SODetIDType>(row) != (int?)cache.GetValueOriginal<SODetIDType>(row))
                {
                    return true;
                }
            }

            // Check if some line is changing its SODet reference.
            foreach (object row in cache.Inserted)
            {
                if ((int?)cache.GetValue<SODetIDType>(row) != (int?)cache.GetValueOriginal<SODetIDType>(row))
                {
                    return true;
                }
            }

            return false;
        }

        public void InsertUpdateSODet(PXCache cacheAppointmentDet, FSAppointmentDet fsAppointmentDetRow, PXSelectBase<FSSODet> viewSODet, FSAppointment apptRow)
        {
            PXEntryStatus lineStatus = cacheAppointmentDet.GetStatus(fsAppointmentDetRow);

            if (lineStatus != PXEntryStatus.Inserted
                    && lineStatus != PXEntryStatus.Updated)
            {
                return;
            }

            FSSODet fsSODetRow = null;

            if (fsAppointmentDetRow.SODetID != null)
            {
                fsSODetRow = FSSODet.UK.Find(viewSODet.Cache.Graph, fsAppointmentDetRow.SODetID);

                if (fsSODetRow == null || fsSODetRow.SODetID != fsAppointmentDetRow.SODetID)
                {
                    throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(FSSODet)));
                }
            }

            bool insertedUpdated = false;

            if (fsSODetRow == null)
            {
                fsSODetRow = new FSSODet();

                try
                {
                    fsSODetRow.SkipUnitPriceCalc = true;
                    fsSODetRow.AlreadyCalculatedUnitPrice = fsAppointmentDetRow.CuryUnitPrice;

                    fsSODetRow = AppointmentEntry.InsertDetailLine<FSSODet, FSAppointmentDet>(viewSODet.Cache,
                                                                                 fsSODetRow,
                                                                                 cacheAppointmentDet,
                                                                                 fsAppointmentDetRow,
                                                                                 noteID: null,
                                                                                 soDetID: null,
                                                                                 copyTranDate: true,
                                                                                 tranDate: fsAppointmentDetRow.TranDate,
                                                                                 SetValuesAfterAssigningSODetID: true,
                                                                                 copyingFromQuote: false);

                    fsAppointmentDetRow.SODetCreate = true;
                    insertedUpdated = true;
                }
                finally
                {
                    fsSODetRow.SkipUnitPriceCalc = false;
                    fsSODetRow.AlreadyCalculatedUnitPrice = null;
                }

                fsAppointmentDetRow.FSSODetRow = fsSODetRow;
            }
            else
            {
                fsSODetRow = (FSSODet)viewSODet.Cache.CreateCopy(fsSODetRow);

                if (fsSODetRow.BranchID != fsAppointmentDetRow.BranchID)
                {
                    viewSODet.Cache.SetValue<FSSODet.branchID>(fsSODetRow, fsAppointmentDetRow.BranchID);
                    insertedUpdated = true;
                }

                if (fsSODetRow.SiteID != fsAppointmentDetRow.SiteID)
                {
                    fsSODetRow.SiteID = fsAppointmentDetRow.SiteID;
                    insertedUpdated = true;
                }

                if (fsSODetRow.SiteLocationID != fsAppointmentDetRow.SiteLocationID)
                {
                    fsSODetRow.SiteLocationID = fsAppointmentDetRow.SiteLocationID;
                    insertedUpdated = true;
                }

                if (fsAppointmentDetRow.SODetCreate == true)
                {
                    fsSODetRow.TranDesc = fsAppointmentDetRow.TranDesc;
                    insertedUpdated = true;
                }

                if (fsSODetRow.CuryUnitCost != fsAppointmentDetRow.CuryUnitCost
                    && fsSODetRow.ApptCntr <= 1)
                {
                    fsSODetRow.CuryUnitCost = fsAppointmentDetRow.CuryUnitCost;
                    insertedUpdated = true;
                }

                if (Base.CanEditSrvOrdLineValues(cacheAppointmentDet, fsAppointmentDetRow, fsSODetRow) == true)
                {
                    if (fsSODetRow.POCreate != fsAppointmentDetRow.EnablePO)
                    {
                        fsSODetRow.POCreate = fsAppointmentDetRow.EnablePO;
                        insertedUpdated = true;
                    }

                    if (fsSODetRow.POSource != fsAppointmentDetRow.POSource)
                    {
                        fsSODetRow.POSource = fsAppointmentDetRow.POSource;
                        insertedUpdated = true;
                    }

                    if (fsSODetRow.POVendorID != fsAppointmentDetRow.POVendorID)
                    {
                        fsSODetRow.POVendorID = fsAppointmentDetRow.POVendorID;
                        insertedUpdated = true;
                    }

                    if (fsSODetRow.POVendorLocationID != fsAppointmentDetRow.POVendorLocationID)
                    {
                        fsSODetRow.POVendorLocationID = fsAppointmentDetRow.POVendorLocationID;
                        insertedUpdated = true;
                    }

                    if (fsSODetRow.ManualCost != fsAppointmentDetRow.ManualCost)
                    {
                        fsSODetRow.ManualCost = fsAppointmentDetRow.ManualCost;
                        insertedUpdated = true;
                    }

                    if (fsSODetRow.EstimatedQty != fsAppointmentDetRow.EstimatedQty)
                    {
                        fsSODetRow.EstimatedQty = fsAppointmentDetRow.EstimatedQty;
                        insertedUpdated = true;
                    }
                }

                if (fsAppointmentDetRow.IsExpenseReceiptItem == true)
                {
                    if (fsSODetRow.CuryUnitCost != fsAppointmentDetRow.CuryUnitCost)
                    {
                        fsSODetRow.CuryUnitCost = fsAppointmentDetRow.CuryUnitCost;
                        insertedUpdated = true;
                    }

                    if (fsSODetRow.CuryUnitPrice != fsAppointmentDetRow.CuryUnitPrice)
                    {
                        fsSODetRow.CuryUnitPrice = fsAppointmentDetRow.CuryUnitPrice;
                        insertedUpdated = true;
                    }

                    if (fsSODetRow.EstimatedQty != fsAppointmentDetRow.EstimatedQty)
                    {
                        fsSODetRow.EstimatedQty = fsAppointmentDetRow.EstimatedQty;
                        insertedUpdated = true;
                    }

                    if (fsSODetRow.UOM != fsAppointmentDetRow.UOM)
                    {
                        fsSODetRow.UOM = fsAppointmentDetRow.UOM;
                        insertedUpdated = true;
                    }

                    if (fsSODetRow.IsBillable != fsAppointmentDetRow.IsBillable)
                    {
                        fsSODetRow.IsBillable = fsAppointmentDetRow.IsBillable;
                        insertedUpdated = true;
                    }

                    if (fsSODetRow.CostCodeID != fsAppointmentDetRow.CostCodeID)
                    {
                        fsSODetRow.CostCodeID = fsAppointmentDetRow.CostCodeID;
                        insertedUpdated = true;
                    }

                    if (fsSODetRow.ProjectTaskID != fsAppointmentDetRow.ProjectTaskID)
                    {
                        fsSODetRow.ProjectTaskID = fsAppointmentDetRow.ProjectTaskID;
                        insertedUpdated = true;
                    }

                    if (fsSODetRow.CuryExtCost != fsAppointmentDetRow.CuryExtCost)
                    {
                        fsSODetRow.CuryExtCost = fsAppointmentDetRow.CuryExtCost;
                        insertedUpdated = true;
                    }

                    if (fsSODetRow.CuryBillableExtPrice != fsAppointmentDetRow.CuryBillableExtPrice)
                    {
                        fsSODetRow.CuryBillableExtPrice = fsAppointmentDetRow.CuryBillableExtPrice;
                        insertedUpdated = true;
                    }
                }

                if (insertedUpdated)
                {
                    fsSODetRow = viewSODet.Update(fsSODetRow);
                }
            }

            if (fsSODetRow.LineType == ID.LineType_ALL.SERVICE
                && ID.Status_SODet.CanBeScheduled(fsSODetRow.Status) == true
                    &&
                        (apptRow.NotStarted == true
                        || apptRow.InProcess == true)
                    )
            {
                // Inserting or updating, this SODet line is being Scheduled in this Appointment.
                if (fsSODetRow.Scheduled != true)
                {
                    viewSODet.Cache.SetValue<FSSODet.scheduled>(fsSODetRow, true);
                    insertedUpdated = true;
                }
            }

            if (insertedUpdated == true)
            {
                fsSODetRow = viewSODet.Update(fsSODetRow);
            }

            fsAppointmentDetRow.FSSODetRow = fsSODetRow;
        }

        public void UpdateSrvOrdSplits(AppointmentEntry apptGraph, FSAppointmentDet apptLine, List<FSApptLineSplit> apptSplits, ServiceOrderEntry soGraph)
        {
            if (apptLine.SODetID != null && apptLine.SODetID != apptLine.FSSODetRow.SODetID)
            {
                throw new PXArgumentException();
            }

            FSSODet soLine = soGraph.ServiceOrderDetails.Search<FSSODet.sODetID>(apptLine.FSSODetRow.SODetID);
            if (soLine == null || soLine.SODetID != apptLine.FSSODetRow.SODetID)
            {
                throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(FSSODet)));
            }

            apptLine.FSSODetRow = soGraph.ServiceOrderDetails.Current = soLine;

            decimal origEstimatedQty = (decimal)soLine.EstimatedQty;
            int insertedSplitCount = 0;
            FSSODetSplit firstInsertedSplit = null;

            // Insert splits in Service Order with new LotSerialNbrs
            foreach (FSApptLineSplit apptSplit in apptSplits)
            {
                FSSODetSplit soSplit = soGraph.Splits.Select().RowCast<FSSODetSplit>().Where(r => r.LotSerialNbr == apptSplit.LotSerialNbr).FirstOrDefault();
                if (soSplit == null)
                {
                    var newSOSplit = new FSSODetSplit();

                    newSOSplit = (FSSODetSplit)soGraph.Splits.Cache.CreateCopy(soGraph.Splits.Insert(newSOSplit));

                    newSOSplit.LotSerialNbr = apptSplit.LotSerialNbr;
                    newSOSplit.Qty = apptSplit.Qty;
                    newSOSplit.BaseQty = INUnitAttribute.ConvertToBase(soGraph.Splits.Cache, newSOSplit.InventoryID, newSOSplit.UOM, newSOSplit.Qty.Value, INPrecision.QUANTITY);
                    // Because the service order don't go through sales order -> shipment, try setting the default operation comes from appt line split to bypass standard quantity validation.
                    newSOSplit.Operation = apptSplit.Operation;

                    newSOSplit = soGraph.Splits.Update(newSOSplit);

                    Dictionary<string, string> errors = PXUIFieldAttribute.GetErrors(soGraph.Splits.Cache, newSOSplit, PXErrorLevel.Error, PXErrorLevel.RowError, PXErrorLevel.Undefined);

                    if (errors == null) { return; }

                    string localizedActionMessage = PXMessages.LocalizeFormatNoPrefix(TX.Action.InsertingLotSerialInServiceOrder, apptSplit.LotSerialNbr, apptLine.LineRef);

                    List<string> uiFields = SharedFunctions.GetUIFields(apptGraph.Splits.Cache, apptSplit);
                    bool fieldWithoutMapping = false;

                    foreach (KeyValuePair<string, string> entry in errors)
                    {
                        Exception exception = new PXSetPropertyException(TX.Error.XErrorOccurredDuringActionY, PXErrorLevel.Error,
                                                                        entry.Value, localizedActionMessage);

                        if (uiFields.Any(e => e.Equals(entry.Key, StringComparison.OrdinalIgnoreCase)))
                        {
                            apptGraph.Splits.Cache.RaiseExceptionHandling(entry.Key, apptSplit, null, exception);
                        }
                        else
                        {
                            fieldWithoutMapping = true;
                        }
                    }

                    if (errors.Count > 0)
                    {
                        if (fieldWithoutMapping == false)
                        {
                            throw new ServiceOrderUpdateException(errors,
                                                                  apptGraph.Splits.Cache.Graph.GetType(),
                                                                  apptSplit,
                                                                  TX.Action.InsertingLotSerialInServiceOrder,
                                                                  apptSplit.LotSerialNbr, apptLine.LineRef);
                        }
                        else
                        {
                            throw new PXOuterException(errors,
                                                       apptGraph.Splits.Cache.Graph.GetType(),
                                                       apptSplit,
                                                       TX.Action.InsertingLotSerialInServiceOrder,
                                                       apptSplit.LotSerialNbr, apptLine.LineRef);
                        }
                    }

                    insertedSplitCount++;

                    soSplit = newSOSplit;
                }

                apptSplit.FSSODetSplitRow = soSplit;

                if (firstInsertedSplit == null)
                {
                    firstInsertedSplit = soSplit;
                }
            }

            // Decrease the Qty on the uncompleted splits without LotSerialNbr to restore the original EstimatedQty
            decimal surplusQuantity = (decimal)soLine.EstimatedQty > origEstimatedQty ? (decimal)soLine.EstimatedQty - origEstimatedQty : 0m;
            while (surplusQuantity > 0m)
            {
                FSSODetSplit soSplit = soGraph.Splits.Select().RowCast<FSSODetSplit>().Where(r => string.IsNullOrEmpty(r.LotSerialNbr) == true && r.Completed == false).FirstOrDefault();
                if (soSplit != null)
                {
                    FSSODetSplit soSplitCopy = (FSSODetSplit)soGraph.Splits.Cache.CreateCopy(soSplit);

                    if (soSplitCopy.Qty >= surplusQuantity)
                    {
                        soSplitCopy.Qty -= surplusQuantity;
                        surplusQuantity = 0m;
                    }
                    else
                    {
                        surplusQuantity -= (decimal)soSplitCopy.Qty;
                        soSplitCopy.Qty = 0m;
                    }

                    if (soSplitCopy.Qty == 0m)
                    {
                        soGraph.Splits.Delete(soSplit);
                    }
                    else
                    {
                        soSplitCopy.BaseQty = INUnitAttribute.ConvertToBase(soGraph.Splits.Cache, soSplitCopy.InventoryID, soSplitCopy.UOM, soSplitCopy.Qty.Value, INPrecision.QUANTITY);
                        soSplitCopy = soGraph.Splits.Update(soSplitCopy);
                    }
                }
                else
                {
                    break;
                }
            }

            if (origEstimatedQty != (decimal)soLine.EstimatedQty)
            {
                Exception exception = new PXSetPropertyException(TX.Error.UpdatingTheServiceOrderLotSerialsEndedInAnAttemptToIncreaseTheLineQty, PXErrorLevel.Error);

                apptGraph.AppointmentDetails.Cache.RaiseExceptionHandling<FSAppointmentDet.lotSerialNbr>(apptLine, null, exception);
                throw new ServiceOrderUpdateException2(TX.Error.UpdatingTheServiceOrderLotSerialsEndedInAnAttemptToIncreaseTheLineQty);
            }

            apptLine.FSSODetRow = soGraph.ServiceOrderDetails.Current;
        }
        #endregion

        #endregion

        #region Override DAC (Cache Attached)
        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Contact")]
        [PXSelector(typeof(
                   SelectFrom<Contact>
                   .InnerJoin<BAccount>.On<Contact.bAccountID.IsEqual<BAccount.bAccountID>>
                   .Where<Contact.contactType.IsNotEqual<ContactTypesAttribute.bAccountProperty>
                     .And<BAccount.type.IsEqual<BAccountType.customerType>.Or<BAccount.type.IsEqual<BAccountType.prospectType>.Or<BAccount.type.IsEqual<BAccountType.combinedType>>>>
                     .And<BAccount.bAccountID.IsEqual<FSServiceOrder.customerID.FromCurrent>.Or<FSServiceOrder.customerID.FromCurrent.IsEqual<Null>>>>
                   .SearchFor<Contact.contactID>),
           typeof(Contact.contactID),
           typeof(Contact.displayName),
           typeof(Contact.fullName),
           typeof(Contact.title),
           typeof(Contact.eMail),
           typeof(Contact.phone1),
           typeof(Contact.contactType),
           DescriptionField = typeof(Contact.displayName))]
        [PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual void _(Events.CacheAttached<FSServiceOrder.contactID> e) { }

        [PXDBInt]
        [PXDefault]
        [LUMGetStaffByBranch]
        [PXUIField(DisplayName = "Staff Member", TabOrder = 0)]
        [PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual void _(Events.CacheAttached<FSAppointmentEmployee.employeeID> e) { }

        [PXDBInt]
        [LUMGetStaffByBranch]
        [PXUIField(DisplayName = "Staff Member ID")]
        [PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual void _(Events.CacheAttached<FSAppointmentDet.staffID> e) { }

        [PXRemoveBaseAttribute(typeof(FSApptLotSerialNbrAttribute))]
        [FSApptLotSerialNbrAttribute2(typeof(FSApptLineSplit.siteID), typeof(FSApptLineSplit.inventoryID), typeof(FSApptLineSplit.subItemID),
                                      typeof(FSApptLineSplit.locationID), typeof(FSAppointmentDet.lotSerialNbr), FieldClass = "LotSerial")]
        protected void _(Events.CacheAttached<FSApptLineSplit.lotSerialNbr> e) { }
        #endregion

        #region Event Handlers
        protected void _(Events.RowSelected<FSAppointment> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            EventHistory.AllowDelete = EventHistory.AllowInsert = EventHistory.AllowUpdate = INRegisterView.AllowDelete = INRegisterView.AllowInsert = INRegisterView.AllowUpdate = false;

            LUMHSNSetup hSNSetup = HSNSetupView.Select();

            bool activePartRequest = hSNSetup?.EnablePartReqInAppt == true;
            bool activeRMAProcess = hSNSetup?.EnableRMAProcInAppt == true;
            bool activeWFStageCtrl = hSNSetup?.EnableWFStageCtrlInAppt == true;

            openPartRequest.SetEnabled(activePartRequest);
            openPartReceive.SetEnabled(activePartRequest);
            openInitiateRMA.SetEnabled(activeRMAProcess);
            openReturnRMA.SetEnabled(activeRMAProcess);
            toggleRMA.SetEnabled(activeRMAProcess);

            Base.menuDetailActions.SetVisible(nameof(OpenPartRequest), activePartRequest);
            Base.menuDetailActions.SetVisible(nameof(OpenPartReceive), activePartRequest);
            Base.menuDetailActions.SetVisible(nameof(OpenInitiateRMA), activeRMAProcess);
            Base.menuDetailActions.SetVisible(nameof(OpenReturnRMA), activeRMAProcess);
            Base.menuDetailActions.SetVisible(nameof(ToggleRMA), activeRMAProcess);

            openPartRequest.SetDisplayOnMainToolbar(false);
            openPartReceive.SetDisplayOnMainToolbar(false);
            openInitiateRMA.SetDisplayOnMainToolbar(false);
            openReturnRMA.SetDisplayOnMainToolbar(false);
            toggleRMA.SetDisplayOnMainToolbar(false);

            lumStages.SetVisible(activeWFStageCtrl);

            EventHistory.AllowSelect = activeWFStageCtrl;
            INRegisterView.AllowSelect = activePartRequest;

            PXUIFieldAttribute.SetVisible<FSAppointmentExt.usrTransferToHQ>(e.Cache, e.Row, hSNSetup?.DisplayTransferToHQ ?? false);
            PXUIFieldAttribute.SetVisible<FSAppointmentDetExt.usrRMARequired>(Base.AppointmentDetails.Cache, null, activeRMAProcess);
            PXUIFieldAttribute.SetVisible<FSAppointmentDetExt.usrIsDOA>(Base.AppointmentDetails.Cache, null, activePartRequest);

            SettingStageButton();
        }

        protected void _(Events.RowUpdated<FSServiceOrder> e, PXRowUpdated baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            if (e.OldRow.ContactID != e.Row.ContactID)
            {
                ServiceOrderEntry_Extension.SetSrvContactInfo(Base.ServiceOrder_Contact.Cache, e.Row.ContactID, e.Row.ServiceOrderContactID);
            }
        }

        protected void _(Events.RowDeleting<FSAppointment> e, PXRowDeleting baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            ///<remarks>
            /// We need to add foreign keys to strengthen the relationship between Appointments and tables of Inventory Transactions. 
            /// If any inventory transactions with any status exist, users are only allowed to cancel the Appointment instead of deletion. 
            /// If deletion is needed, users have to remove all the related inventory transactions first. 
            ///</remarks>
            if (INRegisterView.Select().Count > 0)
            {
                throw new PXException(HSNMessages.ApptHasInvetTrans);
            }
        }

        protected void _(Events.FieldUpdated<FSAppointmentExt.usrTransferToHQ> e)
        {
            if (e.NewValue != null && (bool)e.NewValue == true)
            {
                FSWorkflowStageHandler.apptEntry = Base;
                FSWorkflowStageHandler.InsertEventHistory(nameof(AppointmentEntry), new LUMAutoWorkflowStage()
                {
                    SrvOrdType = Base.AppointmentRecords.Current.SrvOrdType,
                    Descr = PXUIFieldAttribute.GetDisplayName(e.Cache, nameof(FSAppointmentExt.UsrTransferToHQ))
                }); ;
            }
        }

        public void _(Events.FieldUpdated<FSAppointment.finished> e, PXFieldUpdated baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);
            var row = Base.AppointmentSelected.Current;
            if ((this.HSNSetupView.Select().TopFirst?.EnableAppointmentUpdateEndDate ?? false) && row != null)
            {
                if ((bool)e.NewValue && !row.ActualDateTimeEnd.HasValue)
                    Base.AppointmentSelected.SetValueExt<FSAppointment.actualDateTimeEnd>(row, PX.Common.PXTimeZoneInfo.Now);
                else if (!(bool)e.NewValue)
                    Base.AppointmentSelected.SetValueExt<FSAppointment.actualDateTimeEnd>(row, null);
            }
        }
        #endregion

        #region Actions
        public PXAction<FSAppointment> openPartRequest;
        [PXUIField(DisplayName = HSNMessages.PartRequest, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenPartRequest()
        {
            INTransferEntry transferEntry = PXGraph.CreateInstance<INTransferEntry>();

            InitTransferEntry(ref transferEntry, Base, HSNMessages.PartRequest);

            OpenNewForm(transferEntry, TransferScr);
        }

        public PXAction<FSAppointment> openPartReceive;
        [PXUIField(DisplayName = HSNMessages.PartReceive, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenPartReceive()
        {
            if (this.INRegisterView.Select().RowCast<INRegister>().Where(x => x.DocType == INDocType.Transfer && x.Released == true &&
                                                                              x.GetExtension<INRegisterExt>().UsrTransferPurp == LUMTransferPurposeType.PartReq).Count() <= 0)
            {
                throw new PXException(HSNMessages.PartReqNotRlsd);
            }

            string transferNbr = null;

            foreach (INRegister row in INRegisterView.Select().RowCast<INRegister>().Where(x => x.DocType == INDocType.Transfer && x.Released == true && x.TransferType == INTransferType.TwoStep))
            {
                transferNbr = row.RefNbr;
            }

            INReceiptEntry receiptEntry = PXGraph.CreateInstance<INReceiptEntry>();

            InitReceiptEntry(ref receiptEntry, Base, transferNbr);

            //BlankReceipt:
            OpenNewForm(receiptEntry, ReceiptScr);
        }

        public PXAction<FSAppointment> openInitiateRMA;
        [PXUIField(DisplayName = HSNMessages.InitiateRMA, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenInitiateRMA()
        {
            var details = Base.AppointmentDetails.Select().RowCast<FSAppointmentDet>().ToList();

            details.RemoveAll(r => r.GetExtension<FSAppointmentDetExt>()?.UsrRMARequired != true || r.Status == FSAppointmentDet.status.CANCELED);

            if (details.Count <= 0)
            {
                throw new PXException(HSNMessages.NoRMARequired);
            }

            INReceiptEntry receiptEntry = PXGraph.CreateInstance<INReceiptEntry>();

            InitReceiptEntry(ref receiptEntry, Base);

            OpenNewForm(receiptEntry, ReceiptScr);
        }

        public PXAction<FSAppointment> openReturnRMA;
        [PXUIField(DisplayName = HSNMessages.ReturnRMA, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenReturnRMA()
        {
            if (new PXView(Base, true, this.INRegisterView.View.BqlSelect).SelectMulti().RowCast<INRegister>().Where(x => x.DocType == INDocType.Receipt &&
                                                                                                                          x.Status == INDocStatus.Released &&
                                                                                                                          x.GetExtension<INRegisterExt>().UsrTransferPurp == LUMTransferPurposeType.RMAInit).Count() <= 0)
            {
                throw new PXException(HSNMessages.ReturnRMAB4Init);
            }

            INTransferEntry transferEntry = PXGraph.CreateInstance<INTransferEntry>();

            InitTransferEntry(ref transferEntry, Base, HSNMessages.RMAReturned);

            OpenNewForm(transferEntry, TransferScr);
        }

        public PXMenuAction<FSAppointment> lumStages;
        [PXUIField(DisplayName = "STAGES", MapEnableRights = PXCacheRights.Select)]
        [PXButton(MenuAutoOpen = true, CommitChanges = true)]
        public virtual void LumStages() { }

        public PXAction<FSAppointment> toggleRMA;
        [PXUIField(DisplayName = HSNMessages.ToggleRMA, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void ToggleRMA()
        {
            var apptDetails = Base.AppointmentDetails.Current;

            if (apptDetails.LineType != ID.LineType_ALL.INVENTORY_ITEM)
            {
                throw new PXSetPropertyException<FSAppointmentDetExt.usrRMARequired>(HSNMessages.ApptLineTypeInvt);
            }

            if (INRegisterView.Select().RowCast<INRegister>().Where(w => w.GetExtension<INRegisterExt>().UsrTransferPurp == LUMTransferPurposeType.RMAInit ||
                                                                         w.GetExtension<INRegisterExt>().UsrTransferPurp == LUMTransferPurposeType.RMARetu).Count() > 0)
            {
                throw new PXException(HSNMessages.CannotToggleRMA);
            }

            bool rMAReq = apptDetails.GetExtension<FSAppointmentDetExt>().UsrRMARequired ?? false;

            if (apptDetails.Status != FSAppointmentDet.status.CANCELED)
            {
                Base.AppointmentDetails.Cache.SetValue<FSAppointmentDetExt.usrRMARequired>(apptDetails, !rMAReq);
                Base.AppointmentDetails.Update(apptDetails);

                FSWorkflowStageHandler.apptEntry = Base;
                FSWorkflowStageHandler.InsertEventHistory(nameof(AppointmentEntry), new LUMAutoWorkflowStage()
                {
                    WFRule = PX.Objects.Common.Messages.Actions,
                    Descr = HSNMessages.ToggleRMA + " [" + apptDetails.InventoryCD + "] To " + (rMAReq == true ? "Unchecked" : "Checked"),
                    CurrentStage = Base.AppointmentRecords.Current?.WFStageID
                });

                Base.Save.Press();
            }
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Manually insert records into the transfer data view from appointment to open a new window.
        /// </summary>
        /// <param name="transferEntry"></param>
        /// <param name="apptEntry"></param>
        public static void InitTransferEntry(ref INTransferEntry transferEntry, AppointmentEntry apptEntry, string descrType = null)
        {
            FSAppointment appointment = apptEntry.AppointmentSelected.Current;
            INRegister register = transferEntry.CurrentDocument.Cache.CreateInstance() as INRegister;
            INRegisterExt regisExt = register.GetExtension<INRegisterExt>();
            LUMBranchWarehouse branchWH = LUMBranchWarehouse.PK.Find(apptEntry, apptEntry.Accessinfo.BranchID);

            bool isRMA = descrType == HSNMessages.RMAReturned;

            register.TransferType = INTransferType.TwoStep;
            register.ExtRefNbr = $"{appointment.SrvOrdType} | {apptEntry.ServiceOrderRelated.Current?.CustWorkOrderRefNbr} | {apptEntry.ServiceOrderRelated.Current?.CustPORefNbr}";
            register.TranDesc = descrType + " | " + appointment.DocDesc;
            regisExt.UsrSrvOrdType = appointment.SrvOrdType;
            regisExt.UsrAppointmentNbr = appointment.RefNbr;
            regisExt.UsrSORefNbr = appointment.SORefNbr;
            regisExt.UsrTransferPurp = isRMA ? LUMTransferPurposeType.RMARetu : LUMTransferPurposeType.PartReq;

            if (apptEntry.ServiceOrderTypeSelected.Current?.GetExtension<FSSrvOrdTypeExt>().UsrBringBrandAttr2Txfr == true)
            {
                transferEntry.CurrentDocument.Cache.SetValueExt(register, CS.Messages.Attribute + BrandAttr, apptEntry.Answers.Select().RowCast<CSAnswers>().Where(x => x.AttributeID == BrandAttr).FirstOrDefault().Value);
            }

            transferEntry.CurrentDocument.Insert(register);

            PXView view = new PXView(apptEntry, true, apptEntry.AppointmentDetails.View.BqlSelect);

            var list = view.SelectMulti().RowCast<FSAppointmentDet>().Where(x => x.LineType == ID.LineType_ALL.INVENTORY_ITEM);

            if (isRMA == true)
            {
                list = view.SelectMulti().RowCast<FSAppointmentDet>().Where(x => x.LineType == ID.LineType_ALL.INVENTORY_ITEM && x.GetExtension<FSAppointmentDetExt>().UsrRMARequired == true);
            }

            transferEntry.CurrentDocument.Current.SiteID = isRMA ? GetFaultyWFByBranch(transferEntry, transferEntry.Accessinfo.BranchID) : branchWH?.SiteID;
            transferEntry.CurrentDocument.Current.ToSiteID = isRMA ? branchWH?.FaultySiteID : list.FirstOrDefault<FSAppointmentDet>()?.SiteID;
            transferEntry.CurrentDocument.UpdateCurrent();

            foreach (FSAppointmentDet row in list)
            {
                if (row.Status != ID.Status_AppointmentDet.CANCELED &&
                    (row.GetExtension<FSAppointmentDetExt>().UsrIsDOA == true || isRMA == true ||

                    SelectFrom<INRegister>.InnerJoin<INTran>.On<INTran.docType.IsEqual<INRegister.docType>
                                                                .And<INTran.refNbr.IsEqual<INRegister.refNbr>>>
                                          .Where<INRegister.docType.IsEqual<INDocType.transfer>
                                                 .And<INRegisterExt.usrSrvOrdType.IsEqual<@P.AsString>
                                                      .And<INRegisterExt.usrAppointmentNbr.IsEqual<@P.AsString>
                                                           .And<INRegisterExt.usrTransferPurp.IsEqual<LUMTransferPurposeType.partReq>
                                                                .And<INTran.inventoryID.IsEqual<@P.AsInt>
                                                                     .And<INTranExt.usrApptLineRef.IsEqual<@P.AsString>>>>>>>.View
                                          .Select(apptEntry, appointment.SrvOrdType, appointment.RefNbr, row.InventoryID, row.LineRef).Count <= 0)
                    )
                {
                    CreateINTran(transferEntry, row, false, isRMA == false);
                }
            }

            if (transferEntry.transactions.Cache.Inserted.Count() <= 0) { throw new PXException(HSNMessages.NoPartRequest); }
        }

        /// <summary>
        /// Manually insert records into the receipt data view from appointment to open a new window.
        /// </summary>
        /// <param name="receiptEntry"></param>
        /// <param name="apptEntry"></param>
        public static void InitReceiptEntry(ref INReceiptEntry receiptEntry, AppointmentEntry apptEntry, string transferNbr = null)
        {
            FSAppointment appointment = apptEntry.AppointmentSelected.Current;

            INRegister register = receiptEntry.CurrentDocument.Cache.CreateInstance() as INRegister;
            INRegisterExt regisExt = register.GetExtension<INRegisterExt>();

            register.ExtRefNbr = $"{appointment.SrvOrdType} | {apptEntry.ServiceOrderRelated.Current?.CustWorkOrderRefNbr} | {apptEntry.ServiceOrderRelated.Current?.CustPORefNbr}";
            register.TranDesc = $"{(!string.IsNullOrEmpty(transferNbr) ? HSNMessages.PartReceive : HSNMessages.RMAInitiated)} | {appointment.DocDesc}";
            regisExt.UsrSrvOrdType = appointment.SrvOrdType;
            regisExt.UsrAppointmentNbr = appointment.RefNbr;
            regisExt.UsrSORefNbr = appointment.SORefNbr;
            regisExt.UsrTransferPurp = !string.IsNullOrEmpty(transferNbr) ? LUMTransferPurposeType.PartRcv : LUMTransferPurposeType.RMAInit;

            if (apptEntry.ServiceOrderTypeSelected.Current?.GetExtension<FSSrvOrdTypeExt>().UsrBringBrandAttr2Txfr == true)
            {
                receiptEntry.CurrentDocument.Cache.SetValueExt(register, CS.Messages.Attribute + BrandAttr, apptEntry.Answers.Select().RowCast<CSAnswers>().Where(x => x.AttributeID == BrandAttr).FirstOrDefault().Value);
            }

            register = receiptEntry.CurrentDocument.Insert(register);

            if (string.IsNullOrEmpty(transferNbr))
            {
                PXView view = new PXView(apptEntry, true, apptEntry.AppointmentDetails.View.BqlSelect);

                var list = view.SelectMulti().RowCast<FSAppointmentDet>().Where(x => x.LineType == ID.LineType_ALL.INVENTORY_ITEM && x.Status != FSAppointmentDet.status.CANCELED && x.GetExtension<FSAppointmentDetExt>().UsrRMARequired == true);

                foreach (FSAppointmentDet row in list)
                {
                    CreateINTran(receiptEntry, row, true);
                }
            }
            else
            {
                register.TransferNbr = transferNbr;

                receiptEntry.CurrentDocument.Update(register);
            }
        }

        /// <summary>
        /// Create IN trans record from appointment.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="apptDet"></param>
        /// <param name="defective"></param>
        public static void CreateINTran(PXGraph graph, FSAppointmentDet apptDet, bool defective = false, bool overrideLocation = false)
        {
            INTran iNTran = new INTran()
            {
                InventoryID = apptDet.InventoryID,
                Qty = apptDet.EstimatedQty
            };

            if (defective == true) { iNTran.SiteID = GetFaultyWFByBranch(graph, apptDet.BranchID); }

            if (overrideLocation == true) { iNTran.ToLocationID = apptDet.SiteLocationID; }

            iNTran = graph.Caches[typeof(INTran)].Insert(iNTran) as INTran;

            iNTran.GetExtension<INTranExt>().UsrApptLineRef = apptDet.LineRef;

            graph.Caches[typeof(INTran)].Update(iNTran);
        }

        /// <summary>
        /// Redirect to the specified form.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="screenID"></param>
        private static void OpenNewForm(PXGraph graph, string screenID)
        {
            throw new PXRedirectRequiredException(graph, false, PXSiteMap.Provider.FindSiteMapNodeByScreenID(screenID).Title)
            {
                Mode = PXBaseRedirectException.WindowMode.New
            };
        }

        /// <summary>
        /// Enable Header Note Sync between Service Order and Appointment.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        public static void SyncNoteApptOrSrvOrd(PXGraph graph, System.Type fromType, System.Type toType) => PXNoteAttribute.CopyNoteAndFiles(graph.Caches[fromType], graph.Caches[fromType].Current,
                                                                                                                                             graph.Caches[toType], graph.Caches[toType].Current, true, false);

        /// <summary>
        /// Get faulty warehouse by branch which only uses for RMA customization.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="branchID"></param>
        /// <returns></returns>
        public static int? GetFaultyWFByBranch(PXGraph graph, int? branchID)
        {
            return SelectFrom<INSite>.Where<INSite.branchID.IsEqual<@P.AsInt>
                                            .And<INSiteExt.usrIsFaultySite.IsEqual<True>>>.View.Select(graph, branchID).TopFirst?.SiteID;
        }
        #endregion

        #region Methods
        /// <summary>Check Status Is Drity </summary>
        public (bool IsDirty, string oldValue, string newValue) CheckStatusIsDirty(FSAppointment row)
        {
            if (row == null)
                return (false, string.Empty, string.Empty);

            string oldVale = SelectFrom<FSAppointment>
                               .Where<FSAppointment.srvOrdType.IsEqual<P.AsString>
                                   .And<FSAppointment.refNbr.IsEqual<P.AsString>>>
                                .View.Select(new PXGraph(), row.SrvOrdType, row.RefNbr)
                                .RowCast<FSAppointment>()?.FirstOrDefault()?.Status;
            string newValue = row.Status;

            return (!string.IsNullOrEmpty(oldVale) && oldVale != newValue, oldVale, newValue);
        }

        /// <summary>Check Stage Is Dirty </summary>
        public (bool IsDirty, int? oldValue, int? newValue) CheckWFStageIsDirty(FSAppointment row)
        {
            if (row == null)
                return (false, null, null);

            int? oldVale = SelectFrom<FSAppointment>
                               .Where<FSAppointment.srvOrdType.IsEqual<P.AsString>
                                   .And<FSAppointment.refNbr.IsEqual<P.AsString>>>
                                .View.Select(new PXGraph(), row.SrvOrdType, row.RefNbr)
                                .RowCast<FSAppointment>()?.FirstOrDefault()?.WFStageID;
            int? newValue = row.WFStageID;

            return (oldVale.HasValue && oldVale != newValue, oldVale, newValue);
        }

        /// <summary> Add All Stage Button </summary>
        public void AddAllStageButton()
        {
            var primatryView = Base.AppointmentRecords.Cache.GetItemType();
            var list = FSWorkflowStageHandler.stageList.Select(x => new { x.WFStageID, x.WFStageCD }).Distinct();
            var actionLst = new List<PXAction>();
            foreach (var item in list)
            {
                var temp = PXNamedAction.AddAction(Base, primatryView, item.WFStageCD, item.WFStageCD,
                    adapter =>
                    {
                        var row = Base.AppointmentRecords.Current;
                        if (row != null)
                        {
                            var srvOrderData = FSSrvOrdType.PK.Find(Base, row.SrvOrdType);
                            var stageList = FSWorkflowStageHandler.stageList.Where(x => x.WFID == srvOrderData.SrvOrdTypeID);
                            var currStageIDByType = stageList.Where(x => x.WFStageCD == item.WFStageCD).FirstOrDefault().WFStageID;
                            Base.AppointmentRecords.Cache.SetValueExt<FSAppointment.wFStageID>(Base.AppointmentRecords.Current, currStageIDByType);
                            Base.AppointmentRecords.Cache.MarkUpdated(Base.AppointmentRecords.Current);
                            Base.AppointmentRecords.Update(Base.AppointmentRecords.Current);

                            Base.Persist();
                        }
                        return adapter.Get();
                    },
                    new PXEventSubscriberAttribute[] { new PXButtonAttribute() { CommitChanges = true } }
                );
                actionLst.Add(temp);
            }
            foreach (var act in actionLst)
            {
                act.SetDisplayOnMainToolbar(false);
                this.lumStages.AddMenuAction(act);
            }
        }

        /// <summary> Setting Stage Button Status </summary>
        public void SettingStageButton()
        {
            var isAdmin = SelectFrom<UsersInRoles>
                              .Where<UsersInRoles.rolename.IsEqual<P.AsString>
                                    .And<UsersInRoles.username.IsEqual<P.AsString>>>
                              .View.Select(Base, "Administrator", PXAccess.GetUserName())
                              .Count > 0;
            var row = Base.AppointmentRecords.Current;

            if (row != null && !string.IsNullOrEmpty(row.SrvOrdType))
            {
                List<PXResult<LumStageControl>> lists = SelectFrom<LumStageControl>.Where<LumStageControl.srvOrdType.IsEqual<P.AsString>
                                                                                          .And<LumStageControl.currentStage.IsEqual<P.AsInt>>>
                                                                                   .View.Select(Base, row.SrvOrdType, row.WFStageID).ToList();
                var btn = this.lumStages.GetState(null) as PXButtonState;

                if (btn.Menus != null)
                {
                    foreach (ButtonMenu btnMenu in btn.Menus)
                    {
                        var isVisible = lists.Exists(x => (!(x.GetItem<LumStageControl>().AdminOnly ?? false) || ((x.GetItem<LumStageControl>().AdminOnly ?? false) && isAdmin ? true : false)) && FSWorkflowStageHandler.GetStageName(x.GetItem<LumStageControl>().ToStage) == btnMenu.Command);
                        this.lumStages.SetVisible(btnMenu.Command, isVisible);
                    }
                }
            }
        }

        /// <summary> Check Equipment ID is Mandatory </summary>
        public void VerifyEquipmentIDMandatory()
        {
            var details = Base.AppointmentDetails.Select();
            foreach (FSAppointmentDet item in details)
            {
                if (item.LineType == "SERVI" && !item.SMEquipmentID.HasValue)
                    throw new PXException("Target Equipment ID cannot be blank for service");
            }
        }

        #endregion
    }
}