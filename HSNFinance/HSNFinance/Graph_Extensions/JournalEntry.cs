using PX.Common;
using PX.Data;
using PX.Objects.GL;
using static HSNFinance.LSLedgerStlmtEntry;

namespace HSNFinance.Graph_Extensions
{
    public class JournalEntry_Extensions : PXGraphExtension<PX.Objects.GL.JournalEntry>
    {
        #region Event Handlers
        protected void _(Events.RowUpdating<GLTran> e, PXRowUpdating baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            // Since the special UOM value only applies to custom settlement ledgers, it cannot be automatically carried over to new journal transactions.
            if (Base.IsCopyPasteContext == true && e.Row?.UOM.IsIn(ZZ_UOM, YY_UOM) == true)
            {
                e.NewRow.UOM = null;
            }
        }
        #endregion
    }
}
