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

        #region Events

        public virtual void _(Events.FieldDefaulting<LUMAcquirerItems.lineNbr> e)
        {
            var currentList = this.AcquirerItemList.Select().RowCast<LUMAcquirerItems>();
            var maxLineNbr = currentList.Count() == 0 ? 0 : currentList.Max(x => x?.LineNbr ?? 0);
            e.NewValue = maxLineNbr + 1;
        }

        #endregion
    }
}
