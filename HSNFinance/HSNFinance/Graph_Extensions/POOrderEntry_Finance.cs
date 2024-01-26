using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PO
{
    public class POOrderEntry_Finance : PXGraphExtension<POOrderEntry>
    {
        #region Cache Attached
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault()]
        protected virtual void _(Events.CacheAttached<POOrder.orderDesc> e) { }
        #endregion

        #region Event Handlers
        protected void _(Events.RowPersisting<POOrder> e, PXRowPersisting baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            if (e.Row == null) { return; }

            bool requiredDesc = SelectFrom<POSetup>.View.SelectSingleBound(Base, null).TopFirst?.GetExtension<POSetupExt>().UsrReqHeaderDescInPO ?? false;

            PXDefaultAttribute.SetPersistingCheck<POOrder.orderDesc>(e.Cache, e.Row, requiredDesc && e.Row.Hold == false ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
        }
        #endregion
    }
}
