using System.Collections;
using PX.Data;
using PX.Data.BQL;
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

        #region Action
        public PXAction<LSLedgerSettlement> Unmatch;
        [PXProcessButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Unmatch")]
        public virtual IEnumerable unmatch (PXAdapter adapter)
        {
            foreach (LSLedgerSettlement ls in LedgerStlmt.Cache.Updated)
            {
                if (ls.Selected == true)
                {
                    foreach (LSLedgerSettlement delRow in SelectFrom<LSLedgerSettlement>
                                                                     .Where<LSLedgerSettlement.settlementNbr.IsEqual<@P.AsString>>.View.Select(this, ls.SettlementNbr))
                    {
                        LedgerStlmt.Cache.Delete(delRow);

                        GLTranView.Current = SelectFrom<GLTran>.Where<GLTran.module.IsEqual<@P.AsString>
                                                                      .And<GLTran.batchNbr.IsEqual<@P.AsString>
                                                                           .And<GLTran.lineNbr.IsEqual<@P.AsInt>>>>
                                                               .View.ReadOnly.SelectSingleBound(this, null, delRow.Module, delRow.BatchNbr, delRow.LineNbr);

                        LSLedgerStlmtEntry.UpdateGLTranUOM(GLTranView.Cache, null);
                    }
                }
            }

            this.Save.Press();

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