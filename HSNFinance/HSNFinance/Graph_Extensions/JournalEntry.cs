using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.SO;

namespace PX.Objects.GL
{
    public class JournalEntry_Extensions : PXGraphExtension<JournalEntry>
    {
        #region Event Handlers
        protected void _(Events.RowPersisted<Batch> e)
        {
            // Since the description only needs to change when when it's null/blank and then add it to this trigger multiple event.
            if (e.Operation != PXDBOperation.Delete)
            {
                UpdateBatchDesc(e.Row, Base.GLTranModuleBatNbr.Current?.RefNbr);
            }
        }
        protected void _(Events.RowUpdating<GLTran> e, PXRowUpdating baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            // Since the special UOM value only applies to custom settlement ledgers, it cannot be automatically carried over to new journal transactions.
            if (Base.IsCopyPasteContext == true && e.Row?.UOM.IsIn(HSNFinance.LSLedgerStlmtEntry.ZZ_UOM, HSNFinance.LSLedgerStlmtEntry.YY_UOM) == true)
            {
                e.NewRow.UOM = null;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Based on KPMG requirements.
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="iNRefNbr"></param>
        public virtual void UpdateBatchDesc(Batch batch, string iNRefNbr)
        {
            if (batch.Status == BatchStatus.Unposted && string.IsNullOrEmpty(batch.Description))
            {
                const string PurchaseReceipt = "PO302000";
                const string Shipment        = "SO302000";
                
                string description = null;

                switch (batch.CreatedByScreenID)
                {
                    ///<remarks> 
                    /// After enabling the checkbox in Purchase Order Preferences, 
                    /// the Journal Transactions that triggered by Releasing the Purchase Receipts 
                    /// it will copy the Description (POOrder.OrderDesc) to Batch.Description (Inventory Receipts).
                    ///</remarks>
                    case PurchaseReceipt:
                        if (SelectFrom<POSetup>.View.SelectSingleBound(Base, null).TopFirst?.GetExtension<POSetupExt>().UsrCopyHeaderDescFromPO == true)
                        {
                            description = SelectFrom<POOrder>.InnerJoin<POOrderReceipt>.On<POOrderReceipt.orderNoteID.IsEqual<POOrder.noteID>>
                                                             .InnerJoin<INRegister>.On<INRegister.pOReceiptType.IsEqual<POOrderReceipt.receiptType>
                                                                                      .And<INRegister.pOReceiptNbr.IsEqual<POOrderReceipt.receiptNbr>>>
                                                             .Where<INRegister.refNbr.IsEqual<@P.AsString>>.View
                                                             .SelectSingleBound(Base, null, iNRefNbr).TopFirst?.OrderDesc;
                        }
                        break;
                    ///<remarks>
                    /// After enabling the checkbox in Sales Order Preferences, 
                    /// the Journal Transactions that triggered by Releasing the Shipment or related Invoices,
                    /// it will copy the Description (SOOrder.OrderDesc) to Batch.Description (Inventory Issues).
                    /// </remarks>
                    case Shipment:
                        if (SelectFrom<SOSetup>.View.SelectSingleBound(Base, null).TopFirst?.GetExtension<SOSetupExt>().UsrCopyHeaderDescFromSO == true)
                        {
                            description = SelectFrom<SOOrder>.InnerJoin<SOOrderShipment>.On<SOOrderShipment.orderNoteID.IsEqual<SOOrder.noteID>>
                                                             .InnerJoin<INRegister>.On<INRegister.sOShipmentType.IsEqual<SOOrderShipment.shipmentType>
                                                                                      .And<INRegister.sOShipmentNbr.IsEqual<SOOrderShipment.shipmentNbr>>>
                                                             .Where<INRegister.refNbr.IsEqual<@P.AsString>>.View
                                                             .SelectSingleBound(Base, null, iNRefNbr).TopFirst?.OrderDesc;
                        }
                        break;
                    default:
                        description = batch.Module == BatchModule.FA ? "Monthly Depreciation" : batch.Module == BatchModule.DR ? "Deferred Revenue Recognition" : null;
                        break;
                }

                if (!string.IsNullOrEmpty(description))
                {
                    if (batch.Module == BatchModule.FA)
                    {
                        PXUpdate<Set<Batch.description, Required<Batch.description>>,
                                 Batch,
                                 Where<Batch.module, Equal<Required<Batch.module>>,
                                       And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>>>>
                                .Update(Base, description, batch.Module, batch.BatchNbr);
                    }
                    else
                    {
                        Base.BatchModule.Cache.SetValue<Batch.description>(batch, description);
                        Base.BatchModule.Cache.MarkUpdated(batch);
                    }
                }
            }
        }
        #endregion
    }
}
