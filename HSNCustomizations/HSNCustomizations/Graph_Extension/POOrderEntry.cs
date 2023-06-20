using PX.Data;

namespace PX.Objects.PO
{
    public class POOrderEntry_Extensions : PXGraphExtension<PX.Objects.PO.POOrderEntry>
    {
        #region Event Handlers
        protected void _(Events.RowSelected<POOrder> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            if (e.Row == null) { return; }

            Base.printPurchaseOrder.SetEnabled((e.Row?.Approved ?? false) == true);
        }
        #endregion
    }
}
