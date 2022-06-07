using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HSNFinance.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace HSNFinance.Graph
{
    public class LumARAgedPeriodMaint : PXGraph<LumARAgedPeriodMaint>
    {

        public PXFilter<LumARAgedPeriodFilter> MasterFilter;
        public SelectFrom<LumARAgedPeriod>.OrderBy<Asc<LumARAgedPeriod.lineNbr>>.View DetailsView;

        public LumARAgedPeriodMaint()
        {
            DetailsView.AllowInsert = DetailsView.AllowUpdate = DetailsView.AllowDelete = false;
        }

        public PXAction<LumARAgedPeriodFilter> Generate;

        [PXButton]
        [PXUIField(DisplayName = "Generate")]
        protected virtual IEnumerable generate(PXAdapter adapter)
        {
            var periodID = ((LumARAgedPeriodFilter)this.Caches[typeof(LumARAgedPeriodFilter)].Current)?.FinPeriodID;
            if (periodID != null)
            {
                PXLongOperation.StartOperation(this, delegate
                {
                    excuteSP();
                });
            }
            else throw new Exception("Please enter Financial Period.");

            return adapter.Get();
        }

        private void excuteSP()
        {
            var periodID = ((LumARAgedPeriodFilter)this.Caches[typeof(LumARAgedPeriodFilter)].Current)?.FinPeriodID;
            int loopRepeatTime = 6;

            //Truncate LumARAgedPeriod
            var par = new List<PXSPParameter>();
            PXDatabase.Execute("SP_TruncateLumARAgedPeriod", par.ToArray());

            for (int i = 0; i < loopRepeatTime; i++)
            {
                var pars = new List<PXSPParameter>();
                PXSPParameter conditionPeriodID = new PXSPInParameter("@ConditionPeriodID", PXDbType.Char, periodID);
                PXSPParameter companyID = new PXSPInParameter("@companyID", PXDbType.Int, PX.Data.Update.PXInstanceHelper.CurrentCompany);
                pars.Add(conditionPeriodID);
                pars.Add(companyID);

                using (new PXConnectionScope())
                {
                    using (PXTransactionScope ts = new PXTransactionScope())
                    {
                        PXDatabase.Execute("SP_GenerateLumARAgedPeriod_SingleRow", pars.ToArray());
                        ts.Complete();
                    }
                }
                periodID = DateTime.ParseExact(periodID + "01", "yyyyMMdd", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces).AddMonths(-1).ToString("yyyyMM");
            }
        }

        #region Delegate DataView
        public IEnumerable detailsView()
        {
            List<LumARAgedPeriod> result = new List<LumARAgedPeriod>();

            var periodID = ((LumARAgedPeriodFilter)this.Caches[typeof(LumARAgedPeriodFilter)].Current)?.FinPeriodID;
            var curLumARAgedPeriod = SelectFrom<LumARAgedPeriod>.OrderBy<Asc<LumARAgedPeriod.lineNbr>>.View.Select(this);

            if (periodID != null && curLumARAgedPeriod != null && curLumARAgedPeriod.TopFirst?.ConditionPeriodID == periodID)
            {
                result = SelectFrom<LumARAgedPeriod>.OrderBy<Asc<LumARAgedPeriod.lineNbr>>.View.Select(this).RowCast<LumARAgedPeriod>().ToList();
            }

            return result;
        }
        #endregion

        #region LumARAgedPeriodFilter
        [Serializable]
        [PXCacheName("LumARAgedPeriodFilter")]
        public class LumARAgedPeriodFilter : IBqlTable
        {
            #region FinPeriodID
            [FinPeriodNonLockedSelector()]
            [PXUIField(DisplayName = "Financial Period", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual string FinPeriodID { get; set; }
            public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
            #endregion
        }
        #endregion
    }
}