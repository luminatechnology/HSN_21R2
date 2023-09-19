using PX.Data;
using PX.Objects.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.SO;
using System.Collections;
using HSNHighcareCistomizations.Descriptor;
using HSNHighcareCistomizations.Entity;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using System.Threading;
using Newtonsoft.Json;
using HSNHighcareCistomizations.DAC;

namespace HSNHighcareCistomizations.Graph
{
    public class HighcareReturnProcess : PXGraph<HighcareReturnProcess>
    {
        public static Object thisLock = new object();

        public PXSave<HighcareReturnFilter> Save;
        public PXCancel<HighcareReturnFilter> Cancel;

        public PXFilter<HighcareReturnFilter> Filter;
        public PXFilteredProcessingJoinGroupBy<ARInvoice, HighcareReturnFilter,
                      InnerJoin<ARTran, On<ARInvoice.docType, Equal<ARTran.tranType>,
                            And<ARInvoice.refNbr, Equal<ARTran.refNbr>>>,
                      InnerJoin<SOInvoice, On<ARTran.origInvoiceType, Equal<SOInvoice.docType>,
                            And<ARTran.origInvoiceNbr, Equal<SOInvoice.refNbr>>>>>,
                      Where<ARInvoice.docType, Equal<ARInvoiceType.creditMemo>,
                          And<SOInvoice.sOOrderType, Equal<EOTypeAttr>>>,
                      Aggregate<GroupBy<ARInvoice.refNbr,
                                GroupBy<ARInvoice.docType,
                                GroupBy<ARInvoice.status,
                                GroupBy<ARInvoice.customerID,
                                Max<ARInvoice.curyOrigDocAmt,
                                GroupBy<SOInvoice.sOOrderNbr>
                          >>>>>>> Transactions;

        #region Delegate Data View

        public IEnumerable transactions()
        {
            PXView select = new PXView(this, true, this.Transactions.View.BqlSelect);
            Int32 totalrow = 0;
            Int32 startrow = PXView.StartRow;
            List<object> result = select.Select(PXView.Currents, PXView.Parameters,
                   PXView.Searches, PXView.SortColumns, PXView.Descendings,
                   PXView.Filters, ref startrow, PXView.MaximumRows, ref totalrow);
            PXView.StartRow = 0;

            var filter = this.Filter.Current;
            foreach (PXResult<ARInvoice, ARTran, SOInvoice> row in result)
            {
                var invRow = (ARInvoice)row;
                var attrHCCRMSENT = this.Transactions.Cache.GetValueExt(invRow, PX.Objects.CS.Messages.Attribute + "HCCRMSENT") as PXFieldState;
                var attrHCCRMRLSED = this.Transactions.Cache.GetValueExt(invRow, PX.Objects.CS.Messages.Attribute + "HCCRMRLSED") as PXFieldState;
                if (filter.ProcessType == HighcareReturnFilter.NewReturn && !((bool?)attrHCCRMSENT?.Value ?? false))
                    yield return row;
                else if (filter.ProcessType == HighcareReturnFilter.ReleaseReturn && ((bool?)attrHCCRMSENT?.Value ?? false) && !((bool?)attrHCCRMRLSED?.Value ?? false))
                    yield return row;
            }
        }

        #endregion

        public HighcareReturnProcess()
        {
            var filter = this.Filter.Current;
            Transactions.SetProcessDelegate(delegate (List<ARInvoice> list)
            {
                GoProcessing(list, filter);
            });
            Transactions.ParallelProcessingOptions = (options) =>
            {
                options.IsEnabled = true;
                options.BatchSize = 10;
            };
        }

        #region Method

        public static void GoProcessing(List<ARInvoice> list, HighcareReturnFilter filter)
        {
            var baseGraph = PXGraph.CreateInstance<HighcareReturnProcess>();

            foreach (var record in list)
            {
                baseGraph.ProcessReturn(record, filter, baseGraph);
            }

        }

        public void ProcessReturn(ARInvoice item, HighcareReturnFilter filter, HighcareReturnProcess baseGraph)
        {
            var invoiceGraph = PXGraph.CreateInstance<SOInvoiceEntry>();
            var arinvoiceGraph = PXGraph.CreateInstance<ARInvoiceEntry>();
            invoiceGraph.Document.Current = item;

            var mapData = SelectFrom<ARInvoice>
                          .InnerJoin<ARTran>.On<ARInvoice.docType.IsEqual<ARTran.tranType>
                                .And<ARInvoice.refNbr.IsEqual<ARTran.refNbr>>>
                          .InnerJoin<SOInvoice>.On<ARTran.origInvoiceNbr.IsEqual<SOInvoice.refNbr>
                                .And<ARTran.origInvoiceType.IsEqual<SOInvoice.docType>>>
                          .InnerJoin<SOOrder>.On<SOInvoice.sOOrderNbr.IsEqual<SOOrder.orderNbr>
                                .And<SOInvoice.sOOrderType.IsEqual<SOOrder.orderType>>>
                          .Where<ARInvoice.refNbr.IsEqual<P.AsString>>
                          .View.Select(invoiceGraph, item?.RefNbr);
            HighcareHelper helper = new HighcareHelper();

            HighcareReturnEntity entity = new HighcareReturnEntity();
            try
            {
                #region Mapping Entity
                var soOrder = mapData.RowCast<SOOrder>().FirstOrDefault();
                entity.Status = filter?.ProcessType == HighcareReturnFilter.NewReturn ? "On Hold" : "Release";
                entity.ModifiedTime = filter.ProcessType == HighcareReturnFilter.NewReturn ? item.CreatedDateTime : item.LastModifiedDateTime;
                entity.Amount = item?.CuryOrigDocAmt;
                entity.ECOrderNumber = soOrder?.CustomerOrderNbr;
                entity.OrderNumber = soOrder?.OrderNbr;
                var soLines = SelectFrom<SOLine>
                              .Where<SOLine.orderNbr.IsEqual<P.AsString>
                                .And<SOLine.orderType.IsEqual<P.AsString>>>
                              .View.Select(invoiceGraph, soOrder?.OrderNbr, soOrder?.OrderType).RowCast<SOLine>();
                entity.Details = new List<Details>();
                foreach (var arItem in mapData.RowCast<ARTran>())
                {
                    var itemInfo = PX.Objects.IN.InventoryItem.PK.Find(invoiceGraph, arItem?.InventoryID);
                    entity.Details.Add(new Details()
                    {
                        InventoryID = itemInfo?.InventoryID,
                        InventoryCD = itemInfo?.InventoryCD,
                        Quantity = arItem?.Qty
                    }
                    );
                }
                #endregion
                var apiResult = helper.CallReturnAPI(entity);
                if (!apiResult.success)
                    throw new PXException(apiResult.errorMessage);
                // Update json to note
                PXNoteAttribute.SetNote(invoiceGraph.Document.Cache, invoiceGraph.Document.Current, JsonConvert.SerializeObject(entity));

                //// Update User-defined
                //if (filter?.ProcessType == HighcareReturnFilter.NewReturn)
                //{
                //    //invoiceGraph.Document.Cache.SetValueExt(item, PX.Objects.CS.Messages.Attribute + "HCCRMSENT", true);
                //    //invoiceGraph.Document.Cache.SetValueExt(item, PX.Objects.CS.Messages.Attribute + "HCCRMSTIME", PX.Common.PXTimeZoneInfo.Now);
                //    InsertOrUpdateKvextManualWithNew(item.NoteID, baseGraph);
                //}
                //else
                //{
                //    //invoiceGraph.Document.Cache.SetValueExt(item, PX.Objects.CS.Messages.Attribute + "HCCRMRLSED", true);
                //    //invoiceGraph.Document.Cache.SetValueExt(item, PX.Objects.CS.Messages.Attribute + "HCCRMRTIME", PX.Common.PXTimeZoneInfo.Now);
                //    InsertOrUpdateKvextManualWithReleased(item.NoteID, baseGraph);
                //}
            }
            catch (Exception ex)
            {
                PXNoteAttribute.SetNote(invoiceGraph.Document.Cache, invoiceGraph.Document.Current, ex.Message);
                PXProcessing.SetError<ARInvoice>(ex.Message);
            }
            finally
            {
                invoiceGraph.Save.Press();
                if (filter?.ProcessType == HighcareReturnFilter.NewReturn)
                    InsertOrUpdateKvextManualWithNew(item.NoteID, baseGraph);
                else
                    InsertOrUpdateKvextManualWithReleased(item.NoteID, baseGraph);
            }
        }

        /// <summary> 手動Insert/Update Attribute (Filter = New)</summary>
        public void InsertOrUpdateKvextManualWithNew(Guid? noteid, HighcareReturnProcess baseGraph)
        {
            var kvextInfo = SelectFrom<ARRegisterKvExt>
                            .Where<ARRegisterKvExt.recordID.IsEqual<P.AsGuid>>
                            .View.Select(this, noteid).RowCast<ARRegisterKvExt>();

            #region Insert or Update Attribute HCCRMSENT and HCCRMSTIME
            if (kvextInfo.Any(x => x.FieldName == PX.Objects.CS.Messages.Attribute + "HCCRMSENT"))
            {
                // Update
                PXUpdate<Set<ARRegisterKvExt.valueNumeric, Required<ARRegisterKvExt.valueNumeric>>,
                    ARRegisterKvExt,
                    Where<ARRegisterKvExt.recordID, Equal<Required<ARRegisterKvExt.recordID>>,
                      And<ARRegisterKvExt.fieldName, Equal<Required<ARRegisterKvExt.fieldName>>>>>
                    .Update(this, 1, noteid, PX.Objects.CS.Messages.Attribute + "HCCRMSENT");
            }
            else
            {
                // Insert
                List<PXDataFieldAssign> assigns = new List<PXDataFieldAssign>();
                assigns.Add(new PXDataFieldAssign<ARRegisterKvExt.recordID>(noteid));
                assigns.Add(new PXDataFieldAssign<ARRegisterKvExt.valueNumeric>(1));
                assigns.Add(new PXDataFieldAssign<ARRegisterKvExt.fieldName>(PX.Objects.CS.Messages.Attribute + "HCCRMSENT"));
                PXDatabase.Insert<ARRegisterKvExt>(assigns.ToArray());
            }
            if (kvextInfo.Any(x => x.FieldName == PX.Objects.CS.Messages.Attribute + "HCCRMSTIME"))
            {
                // Update
                PXUpdate<Set<ARRegisterKvExt.valueDate, Required<ARRegisterKvExt.valueDate>>,
                    ARRegisterKvExt,
                    Where<ARRegisterKvExt.recordID, Equal<Required<ARRegisterKvExt.recordID>>,
                      And<ARRegisterKvExt.fieldName, Equal<Required<ARRegisterKvExt.fieldName>>>>>
                    .Update(baseGraph, PX.Common.PXTimeZoneInfo.Now, noteid, PX.Objects.CS.Messages.Attribute + "HCCRMSTIME");
            }
            else
            {
                // Insert
                List<PXDataFieldAssign> assigns = new List<PXDataFieldAssign>();
                assigns.Add(new PXDataFieldAssign<ARRegisterKvExt.recordID>(noteid));
                assigns.Add(new PXDataFieldAssign<ARRegisterKvExt.valueDate>(PX.Common.PXTimeZoneInfo.Now));
                assigns.Add(new PXDataFieldAssign<ARRegisterKvExt.fieldName>(PX.Objects.CS.Messages.Attribute + "HCCRMSTIME"));
                PXDatabase.Insert<ARRegisterKvExt>(assigns.ToArray());
            }
            #endregion
        }

        /// <summary> 手動Insert/Update Attribute (Filter = Released)</summary>
        public void InsertOrUpdateKvextManualWithReleased(Guid? noteid, HighcareReturnProcess baseGraph)
        {
            var kvextInfo = SelectFrom<ARRegisterKvExt>
                            .Where<ARRegisterKvExt.recordID.IsEqual<P.AsGuid>>
                            .View.Select(baseGraph, noteid).RowCast<ARRegisterKvExt>();

            #region Insert or Update Attribute HCCRMRLSED and HCCRMRTIME
            if (kvextInfo.Any(x => x.FieldName == PX.Objects.CS.Messages.Attribute + "HCCRMRLSED"))
            {
                // Update
                PXUpdate<Set<ARRegisterKvExt.valueNumeric, Required<ARRegisterKvExt.valueNumeric>>,
                    ARRegisterKvExt,
                    Where<ARRegisterKvExt.recordID, Equal<Required<ARRegisterKvExt.recordID>>,
                      And<ARRegisterKvExt.fieldName, Equal<Required<ARRegisterKvExt.fieldName>>>>>
                    .Update(baseGraph, 1, noteid, PX.Objects.CS.Messages.Attribute + "HCCRMRLSED");
            }
            else
            {
                // Insert
                List<PXDataFieldAssign> assigns = new List<PXDataFieldAssign>();
                assigns.Add(new PXDataFieldAssign<ARRegisterKvExt.recordID>(noteid));
                assigns.Add(new PXDataFieldAssign<ARRegisterKvExt.valueNumeric>(1));
                assigns.Add(new PXDataFieldAssign<ARRegisterKvExt.fieldName>(PX.Objects.CS.Messages.Attribute + "HCCRMRLSED"));
                PXDatabase.Insert<ARRegisterKvExt>(assigns.ToArray());
            }
            if (kvextInfo.Any(x => x.FieldName == PX.Objects.CS.Messages.Attribute + "HCCRMRTIME"))
            {
                // Update
                PXUpdate<Set<ARRegisterKvExt.valueDate, Required<ARRegisterKvExt.valueDate>>,
                    ARRegisterKvExt,
                    Where<ARRegisterKvExt.recordID, Equal<Required<ARRegisterKvExt.recordID>>,
                      And<ARRegisterKvExt.fieldName, Equal<Required<ARRegisterKvExt.fieldName>>>>>
                    .Update(this, PX.Common.PXTimeZoneInfo.Now, noteid, PX.Objects.CS.Messages.Attribute + "HCCRMRTIME");
            }
            else
            {
                // Insert
                List<PXDataFieldAssign> assigns = new List<PXDataFieldAssign>();
                assigns.Add(new PXDataFieldAssign<ARRegisterKvExt.recordID>(noteid));
                assigns.Add(new PXDataFieldAssign<ARRegisterKvExt.valueDate>(PX.Common.PXTimeZoneInfo.Now));
                assigns.Add(new PXDataFieldAssign<ARRegisterKvExt.fieldName>(PX.Objects.CS.Messages.Attribute + "HCCRMRTIME"));
                PXDatabase.Insert<ARRegisterKvExt>(assigns.ToArray());
            }
            #endregion
        }


        #endregion
    }

    public class EOTypeAttr : PX.Data.BQL.BqlString.Constant<EOTypeAttr>
    {
        public EOTypeAttr() : base("EO") { }
    }
}
