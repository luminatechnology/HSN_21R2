using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.SO;
using PX.Data.BQL.Fluent;
using VFCustomizations.DAC;
using PX.Objects.CR.Standalone;
using PX.Data.BQL;
using System.Collections;
using VFCustomizations.DAC_Extension;

namespace VFCustomizations.Graph_Extension
{
    public class SOOrderEntryExt_VF : PXGraphExtension<SOOrderEntry>
    {
        public SelectFrom<LUMAcquirerItems>
               .Where<LUMAcquirerItems.orderType.IsEqual<SOOrder.orderType.FromCurrent>
                 .And<LUMAcquirerItems.orderNbr.IsEqual<SOOrder.orderNbr.FromCurrent>>>
               .View AcquirerItemList;

        #region Override Method

        public override void Initialize()
        {
            base.Initialize();
            var organizationID = PXAccess.GetParentOrganization(PXAccess.GetBranchID());
            var locationInfo = SelectFrom<Location>
                     .Where<Location.bAccountID.IsEqual<P.AsInt>>
                     .View.SelectSingleBound(Base, null, organizationID?.BAccountID).TopFirst;
            this.AcquirerItemList.AllowSelect = locationInfo?.CAvalaraExemptionNumber == "VF";
        }

        #endregion

        #region Action
        public PXAction<SOOrder> printVFSalesOrderReport;
        [PXButton(Category = "Printing and Emailing")]
        [PXUIField(DisplayName = "Print VF Job Sheet", MapEnableRights = PXCacheRights.Select)]
        protected virtual IEnumerable PrintVFSalesOrderReport(PXAdapter adapter)
        {
            // include SubReport: LM941010
            var doc = Base.Document.Current;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["OrderType"] = doc?.OrderType;
            parameters["OrderNbr"] = doc?.OrderNbr;
            throw new PXReportRequiredException(parameters, "LM641010", "LM641010") { Mode = PXBaseRedirectException.WindowMode.New };
        }
        #endregion

        #region Events
        public virtual void _(Events.RowSelected<SOOrder> e, PXRowSelected baseMethod)
        {
            baseMethod?.Invoke(e.Cache, e.Args);
            // 判斷是否顯示VF - Print VF Sales Report
            var isVisiable = SelectFrom<LUMVerifonePreference>.View.Select(Base).TopFirst?.EnableVFCustomizeField ?? false;
            printVFSalesOrderReport.SetVisible(isVisiable);
        }

        public virtual void _(Events.RowSelected<SOLine> e, PXRowSelected baseMethod)
        {
            baseMethod?.Invoke(e.Cache, e.Args);
            // 判斷是否顯示VF - Print VF Sales Report
            var isVisiable = SelectFrom<LUMVerifonePreference>.View.Select(Base).TopFirst?.EnableVFCustomizeField ?? false;
            PXUIFieldAttribute.SetVisible<SOLineExtension.usrForMerchant>(e.Cache, null, isVisiable);
        }

        public virtual void _(Events.FieldDefaulting<LUMAcquirerItems.lineNbr> e)
        {
            var currentList = this.AcquirerItemList.Select().RowCast<LUMAcquirerItems>();
            var maxLineNbr = currentList.Count() == 0 ? 0 : currentList.Max(x => x?.LineNbr ?? 0);
            e.NewValue = maxLineNbr + 1;
        }

        #endregion
    }
}
