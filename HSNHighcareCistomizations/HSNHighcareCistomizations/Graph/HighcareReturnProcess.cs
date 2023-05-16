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

namespace HSNHighcareCistomizations.Graph
{
    public class HighcareReturnProcess : PXGraph<HighcareReturnProcess>
    {
        public static Object thisLock = new object();

        public PXSave<HighcareReturnFilter> Save;
        public PXCancel<HighcareReturnFilter> Cancel;

        public PXFilter<HighcareReturnFilter> Filter;
        public PXFilteredProcessingJoin<ARInvoice, HighcareReturnFilter,
                      InnerJoin<ARTran, On<ARInvoice.docType, Equal<ARTran.tranType>,
                            And<ARInvoice.refNbr, Equal<ARTran.refNbr>>>,
                      InnerJoin<SOInvoice, On<ARTran.origInvoiceType, Equal<SOInvoice.docType>,
                            And<ARTran.origInvoiceNbr, Equal<SOInvoice.refNbr>>>>>,
                      Where<ARInvoice.docType, Equal<ARInvoiceType.creditMemo>,
                          And<SOInvoice.sOOrderType, Equal<EOTypeAttr>>>> Transactions;

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

            var invoiceGraph = PXGraph.CreateInstance<SOInvoiceEntry>();
            foreach (var record in list)
            {
                baseGraph.ProcessReturn(invoiceGraph, record, filter);
            }

        }

        public void ProcessReturn(SOInvoiceEntry invoiceGraph, ARInvoice item, HighcareReturnFilter filter)
        {
            invoiceGraph.Clear();
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
                entity.Status = filter?.ProcessType == HighcareReturnFilter.NewReturn ? "On Hold" : "Release";
                entity.ModifiedTime = filter.ProcessType == HighcareReturnFilter.NewReturn ? item.CreatedDateTime : item.LastModifiedDateTime;
                entity.Amount = item?.CuryOrigDocAmt;
                entity.EOOrderNbr = mapData.RowCast<SOOrder>().FirstOrDefault()?.CustomerOrderNbr;
                #endregion
                var apiResult = helper.CallReturnAPI(entity);
                if (!apiResult.success)
                    throw new PXException(apiResult.errorMessage);
                if (filter?.ProcessType == HighcareReturnFilter.NewReturn)
                {
                    invoiceGraph.Document.Cache.SetValueExt(invoiceGraph.Document.Current, PX.Objects.CS.Messages.Attribute + "HCCRMSENT", true);
                    invoiceGraph.Document.Cache.SetValueExt(invoiceGraph.Document.Current, PX.Objects.CS.Messages.Attribute + "HCCRMSTIME", DateTime.Now);
                }
                else
                {
                    invoiceGraph.Document.Cache.SetValueExt(invoiceGraph.Document.Current, PX.Objects.CS.Messages.Attribute + "HCCRMRLSED", true);
                    invoiceGraph.Document.Cache.SetValueExt(invoiceGraph.Document.Current, PX.Objects.CS.Messages.Attribute + "HCCRMRTIME", DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                PXNoteAttribute.SetNote(invoiceGraph.Document.Cache, invoiceGraph.Document.Current, ex.Message);
            }
            finally
            {
                invoiceGraph.Actions.PressSave();
            }
        }



        #endregion
    }

    public class EOTypeAttr : PX.Data.BQL.BqlString.Constant<EOTypeAttr>
    {
        public EOTypeAttr() : base("EO") { }
    }
}
