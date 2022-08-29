using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.FS;
using PX.Objects.AP;
using PX.Objects.SO;
using HSNCustomizations.DAC;
using PX.Objects.CR;
using HSNCustomizations.Graph;
using PX.Objects.CS;

namespace HSNCustomizations.Descriptor
{
    #region WorkflowRuleSelectorAttribute
    #region enum
    public enum WFRule
    {
        OPEN01 = 0,
        ASSIGN01 = 1,
        ASSIGN03 = 2,
        START01 = 3,
        QUOTATION01 = 4,
        QUOTATION03 = 5,
        AWSPARE01 = 6,
        AWSPARE03 = 7,
        AWSPARE05 = 8,
        AWSPARE07 = 9,
        FINISH01 = 10,
        COMPLETE01 = 11,
        COMPLETE03 = 12,
        INVOICE01 = 13
    }
    #endregion

    public class WorkflowRuleSelectorAttribute : PXCustomSelectorAttribute
    {
        public WorkflowRuleSelectorAttribute() : base(typeof(LUMWorkflowRule.ruleID),
                                                      typeof(LUMWorkflowRule.ruleID),
                                                      typeof(LUMWorkflowRule.descr))
        {
            DescriptionField = typeof(LUMWorkflowRule.descr);
        }

        public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e) { }

        public static string[] WFRuleDescr =
        {
            "Change to Open Stage when appointment is created",
            "Change to Assigned Stage when staff is assigned",
            "Change to Waiting Stage when driver is arranged to pick up machine",
            "Change to Under Diagnose Stage when appointment is started",
            "Change to Quotation Required Stage when parts is required",
            "Change to Quoted when email is sent to customer",
            "Change to Awaiting Spare Parts Stage when part request is initiated",
            "Change to Under Repair Stage when 1-step transfer is released",
            "Change to Part in Transit Stage when 2-step transfer out is released",
            "Change to Under Repair Stage when 2-step transfer is received and released",
            "Change to Under Testing when Finished Check Box is ticked",
            "Change to Repair Complete when appointment is 'completed' by QC/ Engineer",
            "Change to RTS when service order is 'completed' by",
            "Change to Closed when invoice is generated and released."
        };

        protected virtual IEnumerable GetRecords()
        {
            foreach (string wFRule in Enum.GetNames(typeof(WFRule)))
            {
                LUMWorkflowRule wfRule = new LUMWorkflowRule()
                {
                    RuleID = wFRule,
                    Descr = WFRuleDescr[(int)Enum.Parse(typeof(WFRule), wFRule)]
                };

                yield return wfRule;
            }
        }

        #region Unbound DAC
        [PXHidden]
        [Serializable]
        public partial class LUMWorkflowRule : PX.Data.IBqlTable
        {
            #region RuleID
            [PXString(12, IsUnicode = true, IsKey = true)]
            [PXUIField(DisplayName = "Rule")]
            public virtual string RuleID { get; set; }
            public abstract class ruleID : PX.Data.BQL.BqlString.Field<ruleID> { }
            #endregion

            #region Descr
            [PXString(256, IsUnicode = true)]
            [PXUIField(DisplayName = "Description")]
            public virtual string Descr { get; set; }
            public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
            #endregion
        }
        #endregion
    }
    #endregion

    #region ARPymtNumberingAttribute
    /// <summary>
    /// If “Customer Prepayment Numbering Sequence” is blank, follow the standard numbering sequence when the transaction Type is “Prepayment”.
    /// </summary>
    public class ARPymtNumberingAttribute : ARPaymentType.NumberingAttribute
    {
        public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            string curDoType = (string)sender.GetValue<ARPayment.docType>(e.Row);

            LUMHSNSetup hSNSetup = SelectFrom<LUMHSNSetup>.View.Select(sender.Graph);

            if (curDoType == ARDocType.Prepayment && !string.IsNullOrEmpty(hSNSetup?.CPrepaymentNumberingID) && this.UserNumbering == false && e.Operation == PXDBOperation.Insert)
            {
                string generated = PX.Objects.CS.AutoNumberAttribute.GetNextNumber(sender, e.Row, hSNSetup.CPrepaymentNumberingID, (e.Row as ARPayment).DocDate);

                sender.SetValue(e.Row, _FieldName, generated);
            }
            else
            { base.RowPersisting(sender, e); }
        }
    }
    #endregion

    #region INTotalQtyVerificationAttribute
    /// <summary>
    /// If LUMHSNSetup.EnablePartReqInAppt=True, then if INRegister.TotalQty=0, then display error “System cannot save records with 0 quantity” when user click SAVE button.
    /// This rule should apply to Inventory Receipts, Issue, and Transfer.
    /// </summary>
    public class INTotalQtyVerificationAttribute : PXDBQuantityAttribute, IPXRowPersistingSubscriber
    {
        public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            string docType = (string)sender.GetValue<INTran.docType>(e.Row);
            decimal totalQty = (decimal)sender.GetValue<INTran.qty>(e.Row);

            if (e.Operation != PXDBOperation.Delete && docType.IsIn(INDocType.Receipt, INDocType.Issue, INDocType.Transfer) &&
                SelectFrom<LUMHSNSetup>.View.Select(sender.Graph).TopFirst?.EnablePartReqInAppt == true && totalQty <= 0)
            {
                sender.RaiseExceptionHandling<INTran.qty>(e.Row, totalQty, new PXSetPropertyException(HSNMessages.TotalQtyIsZero, PXErrorLevel.Error));
            }
        }
    }
    #endregion

    #region LUMCSAttributeListAttribute
    public class LUMCSAttributeListAttribute : PXStringListAttribute
    {
        public string _attribtueID;

        public LUMCSAttributeListAttribute(string attributeID) : base()
        {
            _attribtueID = attributeID;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            var data = SelectFrom<PX.CS.CSAttributeDetail>
                       .Where<PX.CS.CSAttributeDetail.attributeID.IsEqual<P.AsString>>
                       .View.Select(new PXGraph(), this._attribtueID).RowCast<PX.CS.CSAttributeDetail>();
            if (data != null)
            {
                this._AllowedLabels = data.Select(x => x.Description).ToArray();
                this._AllowedValues = data.Select(x => x.ValueID).ToArray();
            }
        }
    }
    #endregion

    #region LUMGetStaffByBranchAttribute
    // [All-Phase2] Add a Control to enable the staff selection by Branch in Appointments
    public class LUMGetStaffByBranchAttribute : PXCustomSelectorAttribute
    {
        public LUMGetStaffByBranchAttribute() : base(
                           typeof(BAccountStaffMember.bAccountID),
                           new Type[]
            {
                           typeof(BAccountStaffMember.acctCD),
                           typeof(BAccountStaffMember.acctName),
                           typeof(BAccountStaffMember.type),
                           typeof(BAccountStaffMember.status)
            })
        {
            DescriptionField = typeof(BAccountStaffMember.acctName);
            SubstituteKey = typeof(BAccountStaffMember.acctCD);
        }

        public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e) { }

        protected virtual IEnumerable GetRecords()
        {
            #region Standard BQL (Select Staff)
            PXSelectBase<BAccountStaffMember> staffSelect =
                   new PXSelectJoin<BAccountStaffMember,
                          LeftJoin<Vendor,
                          On<
                              Vendor.bAccountID, Equal<BAccountStaffMember.bAccountID>,
                           And<Vendor.vStatus, NotEqual<VendorStatus.inactive>>>,
                          LeftJoin<PX.Objects.EP.EPEmployee,
                          On<
                              PX.Objects.EP.EPEmployee.bAccountID, Equal<BAccountStaffMember.bAccountID>,
                              And<PX.Objects.EP.EPEmployee.vStatus, NotEqual<VendorStatus.inactive>>>,
                          LeftJoin<PX.Objects.PM.PMProject,
                          On<
                              PX.Objects.PM.PMProject.contractID, Equal<Current<FSServiceOrder.projectID>>>,
                          LeftJoin<PX.Objects.EP.EPEmployeeContract,
                          On<
                              PX.Objects.EP.EPEmployeeContract.contractID, Equal<PX.Objects.PM.PMProject.contractID>,
                              And<PX.Objects.EP.EPEmployeeContract.employeeID, Equal<BAccountStaffMember.bAccountID>>>,
                          LeftJoin<PX.Objects.EP.EPEmployeePosition,
                          On<
                              PX.Objects.EP.EPEmployeePosition.employeeID, Equal<PX.Objects.EP.EPEmployee.bAccountID>,
                              And<PX.Objects.EP.EPEmployeePosition.isActive, Equal<True>>>>>>>>,
                          Where<
                              PX.Objects.PM.PMProject.isActive, Equal<True>,
                          And<
                              PX.Objects.PM.PMProject.baseType, Equal<PX.Objects.CT.CTPRType.project>,
                          And<
                              Where2<
                                  Where<
                                      FSxVendor.sDEnabled, Equal<True>>,
                                  Or<
                                      Where<
                                          FSxEPEmployee.sDEnabled, Equal<True>,
                                      And<
                                          Where<
                                              PX.Objects.PM.PMProject.restrictToEmployeeList, Equal<False>,
                                          Or<
                                              PX.Objects.EP.EPEmployeeContract.employeeID, IsNotNull>>>>>>>>>,
                          OrderBy<
                              Asc<BAccountStaffMember.acctCD>>>(this._Graph);
            #endregion

            // Appointment Record
            var apppintmentRecord = this._Graph.Caches[typeof(FSAppointment)].Current as FSAppointment;
            // Appointment Branch LocationID
            var branchLocationID = FSServiceOrder.PK.Find(this._Graph, apppintmentRecord?.SrvOrdType, apppintmentRecord?.SORefNbr)?.BranchLocationID;
            // BranchLocationID 實際的BranchID
            var currentBranchID = FSBranchLocation.PK.Find(this._Graph, branchLocationID)?.BranchID;
            // 是否篩選Staff
            var isFilter = apppintmentRecord == null ? false : (FSSrvOrdType.PK.Find(this._Graph, apppintmentRecord.SrvOrdType)?.GetExtension<FSSrvOrdTypeExt>()?.UsrStaffFilterByBranch ?? false);
            Dictionary<string, string> statusDic = new Dictionary<string, string>()
            {
                {"R","Prospect"},
                {"A","Active" },
                {"H","Hold" },
                {"C","CreditHold" },
                {"T","OneTime" },
                {"I","Inactive" }
            };
            foreach (PXResult<BAccountStaffMember, Vendor, PX.Objects.EP.EPEmployee, PX.Objects.PM.PMProject, PX.Objects.EP.EPEmployeeContract, PX.Objects.EP.EPEmployeePosition> it in staffSelect.Select())
            {
                BAccountStaffMember staff = it;
                PX.Objects.EP.EPEmployeePosition post = it;
                PX.Objects.EP.EPEmployee employee = it;
                var staffBranchID = SelectFrom<PX.Objects.GL.Branch>
                               .Where<PX.Objects.GL.Branch.bAccountID.IsEqual<P.AsInt>>
                               .View.SelectSingleBound(this._Graph, null, employee.ParentBAccountID)
                               .TopFirst?.BranchID;

                // 只回傳Branch ID相同的(需篩選)
                if (apppintmentRecord != null && isFilter)
                {
                    if (staffBranchID == currentBranchID)
                        yield return new BAccountStaffMember
                        {
                            AcctCD = staff.AcctCD,
                            BAccountID = staff.BAccountID,
                            AcctName = staff.AcctName,
                            Type = staff.Type,
                            Status = statusDic[staff.Status]
                        };
                }
                // 如果找不到Appointment header or 不需要篩選則回傳標準
                else
                    yield return new BAccountStaffMember
                    {
                        AcctCD = staff.AcctCD,
                        BAccountID = staff.BAccountID,
                        AcctName = staff.AcctName,
                        Type = staff.Type,
                        Status = statusDic[staff.Status]
                    };
            }
        }
    }
    #endregion

    #region FSApptLotSerialNbrAttribute2
    public class FSApptLotSerialNbrAttribute2 : SOShipLotSerialNbrAttribute
    {
        public FSApptLotSerialNbrAttribute2(Type SiteID, Type InventoryType, Type SubItemType, Type LocationType) : base(SiteID, InventoryType, SubItemType, LocationType)
        {
            CreateCustomSelector(SiteID, InventoryType, SubItemType, LocationType);
        }

        public FSApptLotSerialNbrAttribute2(Type SiteID, Type InventoryType, Type SubItemType, Type LocationType, Type ParentLotSerialNbrType) : base(SiteID, InventoryType, SubItemType, LocationType, ParentLotSerialNbrType)
        {
            CreateCustomSelector(SiteID, InventoryType, SubItemType, LocationType);
        }

        protected virtual void CreateCustomSelector(Type SiteID, Type InventoryType, Type SubItemType, Type LocationType)
        {
            var customSelector = new FSINLotSerialNbrAttribute(SiteID, InventoryType, SubItemType, LocationType, SrvOrdLineID: null);

            _Attributes[_SelAttrIndex] = customSelector;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            sender.Graph.FieldUpdated.AddHandler<FSApptLineSplit.lotSerialNbr>(LotSerialNumberUpdated);
        }

        protected override void LotSerialNumberUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            FSApptLineSplit row = e.Row as FSApptLineSplit;

            FSAppointmentDet parentLine = PXParentAttribute.SelectParent(sender, e.Row, typeof(FSAppointmentDet)) as FSAppointmentDet;

            if (row == null || string.IsNullOrEmpty(row.LotSerialNbr) || parentLine == null || parentLine.IsLotSerialRequired != true) { return; }

            // Because the service order don't go through sales order -> shipment, try setting the default operation is Receipt to bypass standard quantity validation.
            sender.SetValueExt<FSApptLineSplit.operation>(row, SOOperation.Receipt);

            if (row.LocationID == null)
            {
                PXResultset<INLotSerialStatus> res = PXSelect<INLotSerialStatus, Where<INLotSerialStatus.inventoryID, Equal<Required<INLotSerialStatus.inventoryID>>,
                                                                                       And<INLotSerialStatus.subItemID, Equal<Required<INLotSerialStatus.subItemID>>,
                                                                                           And<INLotSerialStatus.siteID, Equal<Required<INLotSerialStatus.siteID>>,
                                                                                               And<INLotSerialStatus.lotSerialNbr, Equal<Required<INLotSerialStatus.lotSerialNbr>>,
                                                                                                   And<INLotSerialStatus.qtyHardAvail, Greater<Zero>>>>>>>
                                                                                 .SelectWindowed(sender.Graph, 0, 1, row.InventoryID, row.SubItemID, row.SiteID, row.LotSerialNbr);
                if (res.Count == 1)
                {
                    sender.SetValueExt<FSApptLineSplit.locationID>(row, ((INLotSerialStatus)res).LocationID);
                }
            }
        }

        public override void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            base.RowSelected(sender, e);
        }

        public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e) { }

        // TODO:
        [Obsolete("This method will be deleted in the next major release.")]
        public virtual void GetLotSerialAvailability(PXGraph graphToQuery, FSAppointmentDet apptLine, int? soDetID, int? apptDetID, string lotSerialNbr, out decimal lotSerialAvailQty, out decimal lotSerialUsedQty, out bool foundServiceOrderAllocation)
        => GetLotSerialAvailabilityInt(graphToQuery, apptLine, soDetID, apptDetID, lotSerialNbr, out lotSerialAvailQty, out lotSerialUsedQty, out foundServiceOrderAllocation);

        public virtual void GetLotSerialAvailability(PXGraph graphToQuery, FSAppointmentDet apptLine, string lotSerialNbr, bool ignoreUseByApptLine, out decimal lotSerialAvailQty, out decimal lotSerialUsedQty, out bool foundServiceOrderAllocation)
        => GetLotSerialAvailabilityInt(graphToQuery, apptLine, lotSerialNbr, ignoreUseByApptLine, out lotSerialAvailQty, out lotSerialUsedQty, out foundServiceOrderAllocation);

        // TODO:
        [Obsolete("This method will be deleted in the next major release.")]
        public static void GetLotSerialAvailabilityInt(PXGraph graphToQuery, FSAppointmentDet apptLine, int? soDetID, int? apptDetID, string lotSerialNbr, out decimal lotSerialAvailQty, out decimal lotSerialUsedQty, out bool foundServiceOrderAllocation)
        {
            GetLotSerialAvailabilityInt(graphToQuery, apptLine, lotSerialNbr, true, out lotSerialAvailQty, out lotSerialUsedQty, out foundServiceOrderAllocation);
        }

        // TODO: Rename this method to GetLotSerialAvailabilityStatic
        public static void GetLotSerialAvailabilityInt(PXGraph graphToQuery, FSAppointmentDet apptLine, string lotSerialNbr, bool ignoreUseByApptLine, out decimal lotSerialAvailQty, out decimal lotSerialUsedQty, out bool foundServiceOrderAllocation)
        {
            GetLotSerialAvailabilityStatic(graphToQuery, apptLine, lotSerialNbr, null, ignoreUseByApptLine, out lotSerialAvailQty, out lotSerialUsedQty, out foundServiceOrderAllocation);
        }

        public static void GetLotSerialAvailabilityStatic(PXGraph graphToQuery, FSAppointmentDet apptLine, string lotSerialNbr, int? splitLineNbr, bool ignoreUseByApptLine, out decimal lotSerialAvailQty, out decimal lotSerialUsedQty, out bool foundServiceOrderAllocation)
        {
            lotSerialAvailQty = 0m;
            lotSerialUsedQty = 0m;
            foundServiceOrderAllocation = false;

            if (string.IsNullOrEmpty(lotSerialNbr) == true && splitLineNbr == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(lotSerialNbr) == false)
            {
                splitLineNbr = null;
            }

            bool searchINAvailQty = true;
            FSSODetSplit soDetSplit = null;

            if (apptLine.SODetID != null && apptLine.SODetID > 0)
            {
                FSSODet fsSODetRow = FSSODet.UK.Find(graphToQuery, apptLine.SODetID);

                if (fsSODetRow != null)
                {
                    BqlCommand bqlCommand = new Select<FSSODetSplit,
                                    Where<
                                        FSSODetSplit.srvOrdType, Equal<Required<FSSODetSplit.srvOrdType>>,
                                        And<FSSODetSplit.refNbr, Equal<Required<FSSODetSplit.refNbr>>,
                                    And<FSSODetSplit.lineNbr, Equal<Required<FSSODetSplit.lineNbr>>>>>>();

                    List<object> parameters = new List<object>();
                    parameters.Add(fsSODetRow.SrvOrdType);
                    parameters.Add(fsSODetRow.RefNbr);
                    parameters.Add(fsSODetRow.LineNbr);

                    if (splitLineNbr == null)
                    {
                        bqlCommand = bqlCommand.WhereAnd(typeof(Where<FSSODetSplit.lotSerialNbr, Equal<Required<FSSODetSplit.lotSerialNbr>>>));
                        parameters.Add(lotSerialNbr);
                    }
                    else
                    {
                        bqlCommand = bqlCommand.WhereAnd(typeof(Where<FSSODetSplit.splitLineNbr, Equal<Required<FSSODetSplit.splitLineNbr>>>));
                        parameters.Add(splitLineNbr);
                    }

                    soDetSplit = (FSSODetSplit)new PXView(graphToQuery, false, bqlCommand).SelectSingle(parameters.ToArray());

                    if (soDetSplit != null)
                    {
                        searchINAvailQty = false;
                    }
                }
            }

            if (searchINAvailQty == true && string.IsNullOrEmpty(lotSerialNbr) == false)
            {
                INLotSerialStatus lotSerialStatus = PX.Objects.IN.INLotSerialStatus.PK.Find(graphToQuery, apptLine.InventoryID, apptLine.SubItemID, apptLine.SiteID, apptLine.LocationID, lotSerialNbr);

                if (lotSerialStatus != null)
                {
                    lotSerialAvailQty = (decimal)lotSerialStatus.QtyAvail;
                }
            }
            else if (soDetSplit != null)
            {
                lotSerialAvailQty = (decimal)soDetSplit.Qty;

                BqlCommand bqlCommand = new Select4<FSApptLineSplit,
                                                     Where<
                                                         FSApptLineSplit.origSrvOrdType, Equal<Required<FSApptLineSplit.origSrvOrdType>>,
                                                         And<FSApptLineSplit.origSrvOrdNbr, Equal<Required<FSApptLineSplit.origSrvOrdNbr>>,
                                And<FSApptLineSplit.origLineNbr, Equal<Required<FSApptLineSplit.origLineNbr>>>>>,
                                                     Aggregate<
                                                         Sum<FSApptLineSplit.qty>>>();

                List<object> parameters = new List<object>();
                parameters.Add(soDetSplit.SrvOrdType);
                parameters.Add(soDetSplit.RefNbr);
                parameters.Add(soDetSplit.LineNbr);

                if (splitLineNbr == null)
                {
                    bqlCommand = bqlCommand.WhereAnd(typeof(Where<FSApptLineSplit.lotSerialNbr, Equal<Required<FSApptLineSplit.lotSerialNbr>>>));
                    parameters.Add(soDetSplit.LotSerialNbr);
                }
                else
                {
                    bqlCommand = bqlCommand.WhereAnd(typeof(Where<FSApptLineSplit.origSplitLineNbr, Equal<Required<FSApptLineSplit.origSplitLineNbr>>>));
                    parameters.Add(soDetSplit.SplitLineNbr);
                }

                if (ignoreUseByApptLine == true)
                {
                    bqlCommand = bqlCommand.WhereAnd(typeof(Where<
                                                                 FSApptLineSplit.srvOrdType, NotEqual<Required<FSApptLineSplit.srvOrdType>>,
                                                                 Or<FSApptLineSplit.apptNbr, NotEqual<Required<FSApptLineSplit.apptNbr>>,
                                                                 Or<FSApptLineSplit.lineNbr, NotEqual<Required<FSApptLineSplit.lineNbr>>>>>));
                    parameters.Add(apptLine.SrvOrdType);
                    parameters.Add(apptLine.RefNbr);
                    parameters.Add(apptLine.LineNbr);
                }

                FSApptLineSplit otherSplitsSum = (FSApptLineSplit)new PXView(graphToQuery, false, bqlCommand).SelectSingle(parameters.ToArray());

                decimal? usedQty = otherSplitsSum != null ? otherSplitsSum.Qty : 0m;
                lotSerialUsedQty = usedQty != null ? (decimal)usedQty : 0m;
                foundServiceOrderAllocation = true;
            }
        }
    }
    #endregion

    // [PhaseII -  Appointment Questionnaire]
    #region QuestionnaireAttributeList

    public class QuestionnaireAttributeList<TEntity> : CRAttributeList<TEntity>
    {
        #region SelectDelegate - Redefined
        // This is the actual customization that is enabling the Attributes tab to populate for InventoryItem Attributes instead of MyDAC
        protected new virtual IEnumerable SelectDelegate()
        {
            var currentObject = (LUMApptQuestionnaire)_Graph.Caches<LUMApptQuestionnaire>()?.Current;
            var questionnaireType = SelectFrom<LUMQuestionnaireType>.Where<LUMQuestionnaireType.questionnaireType.IsEqual<P.AsString>>
                       .View.Select(_Graph, currentObject.QuestionnaireType);
            return SelectInternal(questionnaireType);
        }
        #endregion

        #region Copied from CRAttributeList to make this work
        private readonly EntityHelper _helper;
        private const string CbApiValueFieldName = "Value$value";
        private const string CbApiAttributeIDFieldName = "AttributeID$value";

        public QuestionnaireAttributeList(PXGraph graph) : base(graph)
        {
            _Graph = graph;
            _helper = new EntityHelper(graph);

            View = graph.IsExport
                ? GetExportOptimizedView()
                : new PXView(graph, false,
                new Select3<CSAnswers, OrderBy<Asc<CSAnswers.order>>>(),
                new PXSelectDelegate(SelectDelegate));

            PX.Api.PXDependToCacheAttribute.AddDependencies(View, new[] { typeof(TEntity) });

            _Graph.EnsureCachePersistence(typeof(CSAnswers));
            PXDBAttributeAttribute.Activate(_Graph.Caches[typeof(TEntity)]);
            _Graph.FieldUpdating.AddHandler<CSAnswers.value>(FieldUpdatingHandler);
            _Graph.FieldSelecting.AddHandler<CSAnswers.value>(FieldSelectingHandler);
            _Graph.FieldSelecting.AddHandler<CSAnswers.isRequired>(IsRequiredSelectingHandler);
            _Graph.FieldSelecting.AddHandler<CSAnswers.attributeCategory>(AttributeCategorySelectingHandler);
            _Graph.FieldSelecting.AddHandler<CSAnswers.attributeID>(AttrFieldSelectingHandler);
            _Graph.RowPersisting.AddHandler<CSAnswers>(RowPersistingHandler);
            _Graph.RowPersisting.AddHandler<TEntity>(ReferenceRowPersistingHandler);
            _Graph.RowUpdating.AddHandler<TEntity>(ReferenceRowUpdatingHandler);
            _Graph.RowDeleted.AddHandler<TEntity>(ReferenceRowDeletedHandler);
            _Graph.RowInserted.AddHandler<TEntity>(RowInsertedHandler);

            _Graph.Caches<CSAnswers>().Fields.Add(CbApiValueFieldName);
            _Graph.Caches<CSAnswers>().Fields.Add(CbApiAttributeIDFieldName);
            _Graph.FieldSelecting.AddHandler(typeof(CSAnswers), CbApiValueFieldName, CbApiValueFieldSelectingHandler);
            _Graph.FieldSelecting.AddHandler(typeof(CSAnswers), CbApiAttributeIDFieldName, CbApiAttributeIdFieldSelectingHandler);
        }

        private PXView GetExportOptimizedView()
        {
            var instance = _Graph.Caches[typeof(TEntity)].CreateInstance();
            var classIdField = GetClassIdField(instance);
            var noteIdField = typeof(TEntity).GetNestedType(nameof(CSAttribute.noteID));

            var command = BqlTemplate.OfCommand<
                    Select2<CSAnswers,
                        InnerJoin<CSAttribute,
                            On<CSAnswers.attributeID, Equal<CSAttribute.attributeID>>,
                        InnerJoin<CSAttributeGroup,
                            On<CSAnswers.attributeID, Equal<CSAttributeGroup.attributeID>>>>,
                        Where<CSAttributeGroup.isActive, Equal<True>,
                            And<CSAttributeGroup.entityType, Equal<TypeNameConst>,
                            And<CSAttributeGroup.entityClassID, Equal<Current<BqlPlaceholder.A>>,
                            And<CSAnswers.refNoteID, Equal<Current<BqlPlaceholder.B>>>>>>>>
                    .Replace<BqlPlaceholder.A>(classIdField)
                    .Replace<BqlPlaceholder.B>(noteIdField)
                    .ToCommand();

            return new PXView(_Graph, true, command);
        }

        internal virtual void CbApiValueFieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (e.Row is CSAnswers answer)
            {
                // set in FieldUpdating
                if (sender.GetAttributes<CSAnswers.value>(answer).OfType<PXUIFieldAttribute>().FirstOrDefault() is IPXInterfaceField uiField)
                {
                    if (uiField.ErrorText != null)
                    {
                        e.ReturnState = PXFieldState.CreateInstance(uiField.ErrorValue, typeof(String),
                            errorLevel: PXErrorLevel.Error,
                            error: uiField.ErrorText);
                    }
                }
                e.ReturnValue = answer.Value;

            }
        }

        internal virtual void CbApiAttributeIdFieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (e.Row is CSAnswers answer)
                e.ReturnValue = answer.AttributeID;
        }
        #endregion
    }
    #endregion
}
