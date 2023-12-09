using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.SO;
using PX.Objects.PM;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    public class SM_SOOrderEntryExt : PXGraphExtension<SM_SOOrderEntry, SOOrderEntry>
    {
        public delegate void CreateInvoiceDelegate(PXGraph graphProcess, List<DocLineExt> docLines, short invtMult, DateTime? invoiceDate, string invoiceFinPeriodID, OnDocumentHeaderInsertedDelegate onDocumentHeaderInserted, OnTransactionInsertedDelegate onTransactionInserted, PXQuickProcess.ActionFlow quickProcessFlow);
        [PXOverride]
        public virtual void CreateInvoice(PXGraph graphProcess, List<DocLineExt> docLines, short invtMult, DateTime? invoiceDate, string invoiceFinPeriodID, OnDocumentHeaderInsertedDelegate onDocumentHeaderInserted, OnTransactionInsertedDelegate onTransactionInserted, PXQuickProcess.ActionFlow quickProcessFlow, CreateInvoiceDelegate baseMethod)
        {
            if (docLines.Count == 0)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = docLines[0].fsServiceOrder;
            FSSrvOrdType fsSrvOrdTypeRow = docLines[0].fsSrvOrdType;
            FSPostDoc fsPostDocRow = docLines[0].fsPostDoc;
            FSAppointment fsAppointmentRow = docLines[0].fsAppointment;

            bool? initialHold = false;

            //// This event is raised so the Sales Order page can be opened with the branch indicated in the Appointment
            //// WARNING: Assigning the BranchID directly, behaves incorrectly
            //// Note: The AddHandler method is run before the corresponding method defined in the original graph
            var cancel_defaulting = new PXFieldDefaulting((sender, e) =>
            {
                e.NewValue = fsServiceOrderRow.BranchID;
                e.Cancel = true;
            });

            try
            {
                Base.FieldDefaulting.AddHandler<SOOrder.branchID>(cancel_defaulting);

                SOOrder sOOrderRow = new SOOrder();

                if (invtMult >= 0)
                {
                    if (string.IsNullOrEmpty(fsPostDocRow.PostOrderType))
                    {
                        throw new PXException(TX.Error.POST_ORDER_TYPE_MISSING_IN_SETUP);
                    }

                    sOOrderRow.OrderType = fsPostDocRow.PostOrderType;
                }
                else
                {
                    if (string.IsNullOrEmpty(fsPostDocRow.PostOrderTypeNegativeBalance))
                    {
                        throw new PXException(TX.Error.POST_ORDER_NEGATIVE_BALANCE_TYPE_MISSING_IN_SETUP);
                    }

                    sOOrderRow.OrderType = fsPostDocRow.PostOrderTypeNegativeBalance;
                }

                sOOrderRow.InclCustOpenOrders = true;
                sOOrderRow.CustomerOrderNbr = fsServiceOrderRow.CustPORefNbr;

                Base1.CheckAutoNumbering(Base.soordertype.SelectSingle(sOOrderRow.OrderType).OrderNumberingID);
                sOOrderRow = Base.Document.Current = Base.Document.Insert(sOOrderRow);

                initialHold = sOOrderRow.Hold;
                sOOrderRow.NoteID = null;
                PXNoteAttribute.GetNoteIDNow(Base.Document.Cache, sOOrderRow);
                Base.Document.Cache.SetValueExtIfDifferent<SOOrder.hold>(sOOrderRow, true);
                Base.Document.Cache.SetValueExtIfDifferent<SOOrder.orderDate>(sOOrderRow, invoiceDate);

                // TODO: What to do if there are different dates between the different documents?
                DateTime? requestDate = fsServiceOrderRow.OrderDate;

                // TODO: AC-169637 - Uncomment this line and delete the previous one.
                //DateTime? requestDate = GetShipDate(fsServiceOrderRow, fsAppointmentRow);

                Base.Document.Cache.SetValueExtIfDifferent<SOOrder.requestDate>(sOOrderRow, requestDate);

                Base.Document.Cache.SetValueExtIfDifferent<SOOrder.customerID>(sOOrderRow, fsServiceOrderRow.BillCustomerID);
                Base.Document.Cache.SetValueExtIfDifferent<SOOrder.customerLocationID>(sOOrderRow, fsServiceOrderRow.BillLocationID);
                Base.Document.Cache.SetValueExtIfDifferent<SOOrder.curyID>(sOOrderRow, fsServiceOrderRow.CuryID);

                string docTaxZoneID = fsAppointmentRow != null ? fsAppointmentRow.TaxZoneID : fsServiceOrderRow.TaxZoneID;
                if (sOOrderRow.TaxZoneID != docTaxZoneID)
                {
                    Base.Document.Cache.SetValueExtIfDifferent<SOOrder.overrideTaxZone>(sOOrderRow, true);
                    Base.Document.Cache.SetValueExtIfDifferent<SOOrder.taxZoneID>(sOOrderRow, docTaxZoneID);
                }

                Base.Document.Cache.SetValueExtIfDifferent<SOOrder.taxCalcMode>(sOOrderRow, fsAppointmentRow != null ? fsAppointmentRow.TaxCalcMode : fsServiceOrderRow.TaxCalcMode);

                Base.Document.Cache.SetValueExtIfDifferent<SOOrder.orderDesc>(sOOrderRow, fsServiceOrderRow.DocDesc);

                if (fsServiceOrderRow.ProjectID != null)
                {
                    Base.Document.Cache.SetValueExtIfDifferent<SOOrder.projectID>(sOOrderRow, fsServiceOrderRow.ProjectID);
                }

                string termsID = Base1.GetTermsIDFromCustomerOrVendor(Base, fsServiceOrderRow.BillCustomerID, null);
                if (termsID != null)
                {
                    Base.Document.Cache.SetValueExtIfDifferent<SOOrder.termsID>(sOOrderRow, termsID);
                }
                else
                {
                    Base.Document.Cache.SetValueExtIfDifferent<SOOrder.termsID>(sOOrderRow, fsSrvOrdTypeRow.DfltTermIDARSO);
                }

                Base.Document.Cache.SetValueExtIfDifferent<SOOrder.ownerID>(sOOrderRow, null);

                Base.Document.Cache.SetValueExtIfDifferent<SOOrder.salesPersonID>(sOOrderRow, fsServiceOrderRow?.SalesPersonID);

                sOOrderRow = Base.Document.Update(sOOrderRow);

                Base1.SetContactAndAddress(Base, fsServiceOrderRow);

                if (onDocumentHeaderInserted != null)
                {
                    onDocumentHeaderInserted(Base, sOOrderRow);
                }

                IDocLine docLine = null;
                FSSODet soDet = null;
                FSAppointmentDet appointmentDet = null;
                SOLine sOLineRow = null;
                FSxSOLine fSxSOLineRow = null;
                PMTask pmTaskRow = null;
                FSAppointmentDet fsAppointmentDetRow = null;
                FSSODet fsSODetRow = null;
                List<SharedClasses.SOARLineEquipmentComponent> componentList = new List<SharedClasses.SOARLineEquipmentComponent>();

                foreach (DocLineExt line in docLines)
                {
                    docLine = line.docLine;
                    soDet = line.fsSODet;
                    appointmentDet = line.fsAppointmentDet;
                    fsPostDocRow = line.fsPostDoc;
                    fsServiceOrderRow = line.fsServiceOrder;
                    fsSrvOrdTypeRow = line.fsSrvOrdType;
                    fsAppointmentRow = line.fsAppointment;
                    pmTaskRow = line.pmTask;
                    fsAppointmentDetRow = line.fsAppointmentDet;

                    var itemInfomation = PX.Objects.IN.InventoryItem.PK.Find(Base, docLine?.InventoryID);
                    var lotserialclass = PX.Objects.IN.INLotSerClass.PK.Find(Base, itemInfomation?.LotSerClassID);

                    if (pmTaskRow != null && pmTaskRow.Status == ProjectTaskStatus.Completed)
                    {
                        throw new PXException(TX.Error.POSTING_PMTASK_ALREADY_COMPLETED, fsServiceOrderRow.RefNbr, PX.Objects.FS.MessageHelper.GetLineDisplayHint(Base, docLine.LineRef, docLine.TranDesc, docLine.InventoryID), pmTaskRow.TaskCD);
                    }

                    sOLineRow = new SOLine();
                    sOLineRow = Base.Transactions.Current = Base.Transactions.Insert((SOLine)sOLineRow);
                    sOLineRow = (SOLine)Base.Transactions.Cache.CreateCopy(sOLineRow);

                    sOLineRow.BranchID = docLine.BranchID;
                    sOLineRow.InventoryID = docLine.InventoryID;

                    if (sOLineRow.LineType == SOLineType.Inventory && PXAccess.FeatureInstalled<FeaturesSet.subItem>())
                    {
                        sOLineRow.SubItemID = docLine.SubItemID;
                    }

                    sOLineRow.SiteID = docLine.SiteID;
                    #region Fix Create Sales lot Serial Number can not be empty bug from Appointment (Customization)
                    // [Customize] Fix Create Sales lot Serial Number can not be empty bug from Appointment( LotSerialClass == When Used will happen) 
                    if (docLine.SiteLocationID != null && lotserialclass?.LotSerAssign == PX.Objects.IN.INLotSerAssign.WhenReceived)
                    {
                        sOLineRow.LocationID = docLine.SiteLocationID;
                    }
                    #endregion

                    sOLineRow.UOM = docLine.UOM;

                    if (docLine.ProjectID != null && docLine.ProjectTaskID != null)
                    {
                        sOLineRow.TaskID = docLine.ProjectTaskID;
                    }

                    sOLineRow.SalesAcctID = docLine.AcctID;
                    sOLineRow.TaxCategoryID = docLine.TaxCategoryID;
                    sOLineRow.TranDesc = docLine.TranDesc;
                    sOLineRow = Base.Transactions.Update(sOLineRow);

                    bool isLotSerialRequired = SharedFunctions.IsLotSerialRequired(Base.Transactions.Cache, sOLineRow.InventoryID);
                    bool validateLotSerQty = false;

                    if (isLotSerialRequired == true && fsAppointmentRow == null) // is posted by Service Order
                    {
                        foreach (FSSODetSplit split in PXSelect<FSSODetSplit,
                                                       Where<
                                                            FSSODetSplit.srvOrdType, Equal<Required<FSSODetSplit.srvOrdType>>,
                                                            And<FSSODetSplit.refNbr, Equal<Required<FSSODetSplit.refNbr>>,
                                                            And<FSSODetSplit.lineNbr, Equal<Required<FSSODetSplit.lineNbr>>>>>,
                                                       OrderBy<Asc<FSSODetSplit.splitLineNbr>>>
                                                       .Select(Base, soDet.SrvOrdType, soDet.RefNbr, soDet.LineNbr))
                        {
                            if (split.POCreate == false && string.IsNullOrEmpty(split.LotSerialNbr) == false)
                            {
                                SOLineSplit newSplit = new SOLineSplit();
                                newSplit = (SOLineSplit)Base.splits.Cache.CreateCopy(Base.splits.Insert(newSplit));

                                newSplit.SiteID = split.SiteID != null ? split.SiteID : newSplit.SiteID;
                                newSplit.LocationID = split.LocationID != null ? split.LocationID : newSplit.LocationID;
                                newSplit.LotSerialNbr = split.LotSerialNbr;
                                newSplit.Qty = split.Qty;

                                newSplit = Base.splits.Update(newSplit);
                                validateLotSerQty = true;
                            }
                        }

                        sOLineRow = (SOLine)Base.Transactions.Cache.CreateCopy(sOLineRow);
                    }
                    else if (isLotSerialRequired == true && fsAppointmentRow != null)
                    {
                        foreach (FSApptLineSplit split in PXSelect<FSApptLineSplit,
                                                           Where<
                                                                FSApptLineSplit.srvOrdType, Equal<Required<FSApptLineSplit.srvOrdType>>,
                                                                And<FSApptLineSplit.apptNbr, Equal<Required<FSApptLineSplit.apptNbr>>,
                                                                And<FSApptLineSplit.lineNbr, Equal<Required<FSApptLineSplit.lineNbr>>>>>,
                                                           OrderBy<Asc<FSApptLineSplit.splitLineNbr>>>
                                                           .Select(Base, appointmentDet.SrvOrdType, appointmentDet.RefNbr, appointmentDet.LineNbr))
                        {
                            if (string.IsNullOrEmpty(split.LotSerialNbr) == false)
                            {
                                SOLineSplit newSplit = new SOLineSplit();
                                newSplit = (SOLineSplit)Base.splits.Cache.CreateCopy(Base.splits.Insert(newSplit));

                                newSplit.SiteID = split.SiteID != null ? split.SiteID : newSplit.SiteID;

                                #region [Customize]Fix Create Sales lot Serial Number can not be empty bug from Appointment (Customization)
                                // Fix Create Sales lot Serial Number can not be empty bug from Appointment( LotSerialClass == When Used will happen) 
                                if (lotserialclass?.LotSerAssign == PX.Objects.IN.INLotSerAssign.WhenReceived)
                                {
                                    newSplit.LocationID = split.LocationID != null ? split.LocationID : newSplit.LocationID;
                                    newSplit.LotSerialNbr = split.LotSerialNbr;
                                }
                                #endregion

                                newSplit.Qty = split.Qty;

                                newSplit = Base.splits.Update(newSplit);
                                validateLotSerQty = true;
                            }
                        }

                        sOLineRow = (SOLine)Base.Transactions.Cache.CreateCopy(sOLineRow);
                    }

                    if (validateLotSerQty == false)
                    {
                        sOLineRow.OrderQty = docLine.GetQty(FieldType.BillableField);
                    }
                    else if (sOLineRow.OrderQty != docLine.GetQty(FieldType.BillableField))
                    {
                        throw new PXException(TX.Error.QTY_POSTED_ERROR);
                    }

                    sOLineRow.IsFree = docLine.IsFree;
                    sOLineRow.ManualPrice = docLine.ManualPrice;
                    sOLineRow.CuryUnitPrice = docLine.CuryUnitPrice * invtMult;

                    sOLineRow = Base.Transactions.Update(sOLineRow);
                    sOLineRow = (SOLine)Base.Transactions.Cache.CreateCopy(sOLineRow);

                    sOLineRow.SalesPersonID = fsServiceOrderRow?.SalesPersonID;

                    bool manualDisc = false;
                    if (appointmentDet != null)
                    {
                        manualDisc = (bool)appointmentDet.ManualDisc;
                    }
                    else if (soDet != null)
                    {
                        manualDisc = (bool)soDet.ManualDisc;
                    }

                    sOLineRow.ManualDisc = manualDisc;
                    if (sOLineRow.ManualDisc == true)
                    {
                        sOLineRow.DiscPct = docLine.DiscPct;
                    }

                    sOLineRow.CuryExtPrice = docLine.CuryBillableExtPrice * invtMult;
                    sOLineRow.CuryExtPrice = CM.PXCurrencyAttribute.Round(Base.Transactions.Cache, sOLineRow, sOLineRow.CuryExtPrice ?? 0m, CM.CMPrecision.TRANCURY);

                    sOLineRow = Base.Transactions.Update(sOLineRow);

                    if (docLine.SubID != null)
                    {
                        try
                        {
                            Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.salesSubID>(sOLineRow, docLine.SubID);
                        }
                        catch (PXException)
                        {
                            sOLineRow.SalesSubID = null;
                            sOLineRow = Base.Transactions.Update(sOLineRow);
                        }
                    }
                    else
                    {
                        Base1.SetCombinedSubID(Base,
                                        Base.Transactions.Cache,
                                        null,
                                        null,
                                        sOLineRow,
                                        fsSrvOrdTypeRow,
                                        sOLineRow.BranchID,
                                        sOLineRow.InventoryID,
                                        fsServiceOrderRow.BillLocationID,
                                        fsServiceOrderRow.BranchLocationID,
                                        fsServiceOrderRow.SalesPersonID,
                                        docLine.IsService);

                        sOLineRow = Base.Transactions.Update(sOLineRow);
                    }

                    // TODO: Add TaxCategoryID in Service Contract line definition
                    //Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.taxCategoryID>(sOLineRow, docLine.TaxCategoryID);
                    Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.commissionable>(sOLineRow, fsServiceOrderRow?.Commissionable);

                    Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.costCodeID>(sOLineRow, docLine.CostCodeID);

                    //Set the line as a posted
                    fSxSOLineRow = Base.Transactions.Cache.GetExtension<FSxSOLine>(sOLineRow);

                    fSxSOLineRow.SDPosted = true;

                    fSxSOLineRow.SrvOrdType = fsServiceOrderRow.SrvOrdType;
                    fSxSOLineRow.ServiceOrderRefNbr = fsServiceOrderRow.RefNbr;
                    fSxSOLineRow.AppointmentRefNbr = fsAppointmentRow?.RefNbr;
                    fSxSOLineRow.ServiceOrderLineNbr = soDet?.LineNbr;
                    fSxSOLineRow.AppointmentLineNbr = fsAppointmentDetRow?.LineNbr;

                    if (PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>())
                    {
                        if (docLine.EquipmentAction != null)
                        {
                            Base.Transactions.Cache.SetValueExtIfDifferent<FSxSOLine.equipmentAction>(sOLineRow, docLine.EquipmentAction);
                            Base.Transactions.Cache.SetValueExtIfDifferent<FSxSOLine.sMEquipmentID>(sOLineRow, docLine.SMEquipmentID);
                            Base.Transactions.Cache.SetValueExtIfDifferent<FSxSOLine.equipmentComponentLineNbr>(sOLineRow, docLine.EquipmentLineRef);

                            fSxSOLineRow.Comment = docLine.Comment;

                            if (docLine.EquipmentAction == ID.Equipment_Action.SELLING_TARGET_EQUIPMENT
                                || ((docLine.EquipmentAction == ID.Equipment_Action.CREATING_COMPONENT
                                     || docLine.EquipmentAction == ID.Equipment_Action.UPGRADING_COMPONENT
                                     || docLine.EquipmentAction == ID.Equipment_Action.NONE)
                                        && string.IsNullOrEmpty(docLine.NewTargetEquipmentLineNbr) == false))
                            {
                                componentList.Add(new SharedClasses.SOARLineEquipmentComponent(docLine, sOLineRow, fSxSOLineRow));
                            }
                            else
                            {
                                fSxSOLineRow.ComponentID = docLine.ComponentID;
                            }
                        }
                    }

                    SharedFunctions.CopyNotesAndFiles(Base.Transactions.Cache, sOLineRow, docLine, fsSrvOrdTypeRow);
                    fsPostDocRow.DocLineRef = sOLineRow = Base.Transactions.Update(sOLineRow);

                    if (onTransactionInserted != null)
                    {
                        onTransactionInserted(Base, sOLineRow);
                    }
                }

                if (componentList.Count > 0)
                {
                    //Assigning the NewTargetEquipmentLineNbr field value for the component type records
                    foreach (SharedClasses.SOARLineEquipmentComponent currLineModel in componentList.Where(x => x.equipmentAction == ID.Equipment_Action.SELLING_TARGET_EQUIPMENT))
                    {
                        foreach (SharedClasses.SOARLineEquipmentComponent currLineComponent in componentList.Where(x => (x.equipmentAction == ID.Equipment_Action.CREATING_COMPONENT
                                                                                                                        || x.equipmentAction == ID.Equipment_Action.UPGRADING_COMPONENT
                                                                                                                        || x.equipmentAction == ID.Equipment_Action.NONE)))
                        {
                            if (currLineComponent.sourceNewTargetEquipmentLineNbr == currLineModel.sourceLineRef)
                            {
                                currLineComponent.fsxSOLineRow.ComponentID = currLineComponent.componentID;
                                currLineComponent.fsxSOLineRow.NewEquipmentLineNbr = currLineModel.currentLineRef;
                            }
                        }
                    }
                }

                if (Base.soordertype.Current.RequireControlTotal == true)
                {
                    Base.Document.Cache.SetValueExtIfDifferent<SOOrder.curyControlTotal>(sOOrderRow, sOOrderRow.CuryOrderTotal);
                }

                if (initialHold != true || quickProcessFlow != PXQuickProcess.ActionFlow.NoFlow)
                {
                    Base.Document.Cache.SetValueExtIfDifferent<SOOrder.hold>(sOOrderRow, false);
                }

                sOOrderRow = Base.Document.Update(sOOrderRow);
            }
            finally
            {
                Base.FieldDefaulting.RemoveHandler<SOOrder.branchID>(cancel_defaulting);
            }
        }
    }
}
