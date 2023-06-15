using PX.Common;
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

            // Since the field "Approved" would be hidden or skipped updating by standard approval config settings, then change to use status to the condition.
            Base.printPurchaseOrder.SetEnabled(!e.Row.Status.IsIn(POOrderStatus.Hold, POOrderStatus.PendingApproval, POOrderStatus.Rejected, POOrderStatus.Cancelled));
        }
        #endregion
    }
}
