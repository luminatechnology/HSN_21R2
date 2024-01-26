using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.GL;
using HSNFinance.DAC;

namespace HSNFinance
{
    public class LSLedgerStlmtInq : PXGraph<LSLedgerStlmtInq>
    {
        #region Select & Features
        public PXCancel<LSLedgerSettlement> Cancel;
        public PXSavePerRow<LSLedgerSettlement> Save;
      
        [PXFilterable()]
        public SelectFrom<LSLedgerSettlement>.View LedgerStlmt;

        public SelectFrom<GLTran>.Where<GLTran.module.IsEqual<LSLedgerSettlement.module.FromCurrent>
                                        .And<GLTran.batchNbr.IsEqual<LSLedgerSettlement.batchNbr.FromCurrent>
                                             .And<GLTran.lineNbr.IsEqual<LSLedgerSettlement.lineNbr.FromCurrent>>>>.View GLTranView;

        #endregion

        #region Actions
        public PXAction<LSLedgerSettlement> unmatch;
        [PXProcessButton(), PXUIField(DisplayName = "Unmatch")]
        public virtual IEnumerable Unmatch(PXAdapter adapter)
        {
            Dictionary<GLTran, string> trans = new Dictionary<GLTran, string>();

            using (PXTransactionScope ts = new PXTransactionScope())
            {
                foreach (var ls in LedgerStlmt.Cache.Updated.OfType<LSLedgerSettlement>().Where(w => w.Selected == true).GroupBy(g => g.SettlementNbr).Select(s => s.First()))
                {
                    foreach (LSLedgerSettlement delRow in LedgerStlmt.Cache.Cached.OfType<LSLedgerSettlement>().Where(w => w.SettlementNbr == ls.SettlementNbr))
                    {
                        LedgerStlmt.Cache.Delete(delRow);

                        var tran = GLTran.PK.Find(this, delRow.Module, delRow.BatchNbr, delRow.LineNbr);

                        if (LedgerStlmt.Cache.Cached.OfType<LSLedgerSettlement>().Except((IEnumerable<LSLedgerSettlement>)LedgerStlmt.Cache.Deleted)
                                                                                 .Where(w => w.Module == delRow.Module && w.BatchNbr == delRow.BatchNbr && w.LineNbr == delRow.LineNbr)
                                                                                 .Any())
                        {
                            trans.Add(tran, LSLedgerStlmtEntry.YY_UOM);
                        }
                        else
                        {
                            trans[tran] = null;
                        }
                    }
                }

                foreach (GLTran row in trans.Keys)
                {
                    trans.TryGetValue(row, out string uOM);

                    GLTranView.Current = row;

                    LSLedgerStlmtEntry.UpdateGLTranUOM(GLTranView.Cache, uOM);
                }

                Save.Press();

                ts.Complete();
            }

            return adapter.Get();
        }
        #endregion

        #region Event Handlers
        protected virtual void _(Events.RowSelected<LSLedgerSettlement> e)
        {
            LedgerStlmt.Cache.AllowDelete = LedgerStlmt.AllowInsert = false;

            PXUIFieldAttribute.SetEnabled<LSLedgerSettlement.branchID>(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<LSLedgerSettlement.batchNbr>(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<LSLedgerSettlement.lineNbr>(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<LSLedgerSettlement.ledgerID>(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<LSLedgerSettlement.accountID>(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<LSLedgerSettlement.subID>(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<LSLedgerSettlement.origCreditAmt>(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<LSLedgerSettlement.origDebitAmt>(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<LSLedgerSettlement.settledCreditAmt>(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<LSLedgerSettlement.settledDebitAmt>(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<LSLedgerSettlement.tranDesc>(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<LSLedgerSettlement.tranDate>(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<LSLedgerSettlement.refNbr>(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<LSLedgerSettlement.inventoryID>(e.Cache, e.Row, false);
        }
        #endregion
    }
}