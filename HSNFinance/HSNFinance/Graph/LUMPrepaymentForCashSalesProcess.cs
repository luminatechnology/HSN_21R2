using HSNFinance.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNFinance.Graph
{
    public class LUMPrepaymentForCashSalesProcess : PXGraph<LUMPrepaymentForCashSalesProcess>
    {
        public PXSave<PrepaymentFilter> Save;
        public PXCancel<PrepaymentFilter> Cancel;
        public PXFilter<PrepaymentFilter> Filter;
        public SelectFrom<v_PrepaymentMappingTable>
              .Where<v_PrepaymentMappingTable.customerID.IsEqual<PrepaymentFilter.customerID.FromCurrent>>
              .View Transactions;

        public IEnumerable transactions()
        {
            var arPaymentEntry = PXGraph.CreateInstance<ARPaymentEntry>();
            PXView select = new PXView(this, true, Transactions.View.BqlSelect);
            Int32 totalrow = 0;
            Int32 startrow = PXView.StartRow;
            List<object> result = select.Select(PXView.Currents, PXView.Parameters,
                   PXView.Searches, PXView.SortColumns, PXView.Descendings,
                   PXView.Filters, ref startrow, PXView.MaximumRows, ref totalrow);
            PXView.StartRow = 0;
            foreach (v_PrepaymentMappingTable row in result)
            {
                arPaymentEntry.Document.Current = arPaymentEntry.Document.Search<ARPayment.refNbr>(row.PrepaymentRefNbr, "PPM");
                row.CuryUnappliedBal = arPaymentEntry.Document.Current.CuryUnappliedBal;
                yield return row;
            }
        }

        #region Action
        public PXAction<PrepaymentFilter> Process;
        [PXButton]
        [PXUIField(DisplayName = "PROCESS", MapEnableRights = PXCacheRights.Select)]
        public IEnumerable process(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, delegate ()
            {
                CreateDebitMemo(false);
            });
            return adapter.Get();
        }

        public PXAction<PrepaymentFilter> ProcessAll;
        [PXButton]
        [PXUIField(DisplayName = "PROCESS ALL", MapEnableRights = PXCacheRights.Select)]
        public IEnumerable processAll(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, delegate ()
            {
                CreateDebitMemo(true);
            });
            return adapter.Get();
        }

        #endregion

        #region Method

        public virtual void CreateDebitMemo(bool IsProcessAll)
        {
            var filter = this.Filter.Current;
            IEnumerable<v_PrepaymentMappingTable> selectedTrans;
            if (IsProcessAll)
                selectedTrans = this.Transactions.View.SelectMulti().RowCast<v_PrepaymentMappingTable>();
            else
                selectedTrans = this.Transactions.Cache.Updated.RowCast<v_PrepaymentMappingTable>().Where(x => x.Selected ?? false);

            if (selectedTrans.Count() == 0)
            {
                PXProcessing.SetProcessed<v_PrepaymentMappingTable>();
                return;
            }

            var soInvEntry = PXGraph.CreateInstance<SOInvoiceEntry>();
            #region Debit memo Header
            var doc = soInvEntry.Document.Cache.CreateInstance() as ARInvoice;
            doc.DocType = "DRM";
            doc = soInvEntry.Document.Insert(doc);
            doc.CustomerID = filter?.CustomerID;
            doc.DocDesc = "Settle Open Prepayment";
            soInvEntry.Document.Update(doc);
            #endregion

            #region Debit memo Details & Applications
            foreach (var item in selectedTrans)
            {
                #region Details
                var line = soInvEntry.Transactions.Cache.CreateInstance() as ARTran;
                line.BranchID = item?.BranchID;
                line.TranDesc = "Settle Open Prepayment";
                line.CuryExtPrice = item?.CuryExtPrice;
                line.AccountID = item?.AccountID;
                line.SubID = item?.SubID;
                line = soInvEntry.Transactions.Insert(line);
                #endregion

                #region Applications
                var appt = soInvEntry.Adjustments.Cache.CreateInstance() as ARAdjust2;
                appt.AdjgDocType = ARPaymentType.Prepayment;
                appt.AdjgRefNbr = item?.PrepaymentRefNbr;
                appt.CuryAdjdAmt = item?.CuryExtPrice;
                appt = soInvEntry.Adjustments.Insert(appt);
                #endregion
            }
            #endregion

            // Save Invoice
            soInvEntry.Save.Press();
            PXProcessing.SetProcessed<v_PrepaymentMappingTable>();
            throw new PXRedirectRequiredException(soInvEntry, true, "Invoice");
        }

        #endregion

    }

    [Serializable]
    public class PrepaymentFilter : IBqlTable
    {
        #region CustomerID
        [CustomerActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Filterable = true, TabOrder = 2)]
        [PXUIField(DisplayName = "Customer ID")]
        public virtual int? CustomerID { get; set; }
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
        #endregion
    }

}
