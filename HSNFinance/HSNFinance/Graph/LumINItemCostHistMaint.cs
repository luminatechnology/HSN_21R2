using HSNFinance.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNFinance.Graph
{
    public class LumINItemCostHistMaint : PXGraph<LumINItemCostHistMaint>
    {
        public PXFilter<LumINItemCostFilter> MasterViewFilter;

        [PXFilterable]
        public PXFilteredProcessing<LumINItemCostHist, LumINItemCostFilter, Where<LumINItemCostHist.conditionPeriod, Equal<Current<LumINItemCostFilter.finPeriodID>>>> DetailsView;

        #region Base Table Function Control
        public LumINItemCostHistMaint()
        {
            var filter = this.MasterViewFilter.Current;
            DetailsView.SetProcessVisible(false);
            DetailsView.AllowInsert = DetailsView.AllowUpdate = DetailsView.AllowDelete = false;

            DetailsView.SetProcessDelegate(delegate (List<LumINItemCostHist> list)
            {
                GoProcessing(filter);
            });
        }
        #endregion

        #region Event

        public virtual void _(Events.FieldUpdated<LumINItemCostFilter.finPeriodID> e)
        {
            if (!string.IsNullOrEmpty((string)e.NewValue) && this.DetailsView.Select().Count == 0 && this.DetailsView.Select().TopFirst == null)
                InitialData();
        }

        #endregion

        #region LumINItemCost Filter
        [Serializable]
        [PXCacheName("Lum INItemCostHist Filter")]
        public class LumINItemCostFilter : IBqlTable
        {
            #region FinPeriodID
            [PXString(6, InputMask = "")]
            [FinPeriodID()]
            [PXUIField(DisplayName = "Period ID", Visibility = PXUIVisibility.SelectorVisible)]
            [PXSelector(typeof(Search4<INItemCostHist.finPeriodID, Where<INItemCostHist.finPeriodID.IsEqual<INItemCostHist.finPeriodID>>, Aggregate<GroupBy<INItemCostHist.finPeriodID>>, OrderBy<Desc<INItemCostHist.finPeriodID>>>),
                        typeof(INItemCostHist.finPeriodID))]
            public virtual string FinPeriodID { get; set; }
            public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
            #endregion
        }
        #endregion

        #region Delegate DataView
        //public IEnumerable detailsView()
        //{
        //    PXView select = new PXView(this, true, DetailsView.View.BqlSelect);
        //    int totalrow = 0;
        //    int startrow = PXView.StartRow;
        //    var result = select.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startrow, PXView.MaximumRows, ref totalrow);
        //    PXView.StartRow = 0;
        //    return result;
        //}
        #endregion

        #region Method

        /// <summary> 非同步執行程序 </summary>
        public static void GoProcessing(LumINItemCostFilter filter)
        {
            var baseGraph = CreateInstance<LumINItemCostHistMaint>();
            baseGraph.GenerateAgingReport(baseGraph, filter);
        }

        public virtual void GenerateAgingReport(LumINItemCostHistMaint baseGraph, LumINItemCostFilter filter)
        {
            List<object> result = new List<object>();

            var currentSearchPeriod = filter?.FinPeriodID;

            if (currentSearchPeriod != null)
            {
                var pars = new List<PXSPParameter>();
                PXSPParameter period = new PXSPInParameter("@period", PXDbType.Int, currentSearchPeriod);
                PXSPParameter companyID = new PXSPInParameter("@companyID", PXDbType.Int, PX.Data.Update.PXInstanceHelper.CurrentCompany);
                pars.Add(period);
                pars.Add(companyID);
                using (new PXConnectionScope())
                {
                    using (new PXCommandScope(3000))
                    {
                        using (PXTransactionScope ts = new PXTransactionScope())
                        {
                            PXDatabase.Execute("SP_GenerateLumINItemCostHist", pars.ToArray());
                            ts.Complete();
                        }
                    }
                }
            }
        }

        /// <summary> 產生一筆固定資料 </summary>
        public virtual void InitialData()
        {
            string screenIDWODot = this.Accessinfo.ScreenID.ToString().Replace(".", "");

            PXDatabase.Insert<LumINItemCostHist>(
                new PXDataFieldAssign<LumINItemCostHist.inventoryID>(GetFixInventoryItem()),
                new PXDataFieldAssign<LumINItemCostHist.conditionPeriod>(this.MasterViewFilter.Current.FinPeriodID));
        }


        /// <summary> 取固定資料的InventoryItem </summary>
        public int? GetFixInventoryItem()
        {
            return SelectFrom<InventoryItem>
                   .Where<InventoryItem.itemStatus.IsEqual<P.AsString>>
                   .View.Select(this, "AC").TopFirst.InventoryID;
        }

        #endregion
    }
}
