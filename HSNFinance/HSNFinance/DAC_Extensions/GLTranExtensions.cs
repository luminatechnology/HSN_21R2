using HSNFinance.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using System;

namespace PX.Objects.GL
{
    public class GLTranExt : PXCacheExtension<PX.Objects.GL.GLTran>
    {
        #region Unbound Custom Fields

        #region UsrRmngCreditAmt
        [PXDecimal()]
        [PXUIField(DisplayName = "Remaining Crdit Amt", Enabled = false)]
        public virtual decimal? UsrRmngCreditAmt { get; set; }
        public abstract class usrRmngCreditAmt : PX.Data.BQL.BqlDecimal.Field<usrRmngCreditAmt> { }
        #endregion

        #region UsrRmngDebitAmt
        [PXDecimal()]
        [PXUIField(DisplayName = "Remaining Debit Amt", Enabled = false)]
        public virtual decimal? UsrRmngDebitAmt { get; set; }
        public abstract class usrRmngDebitAmt : PX.Data.BQL.BqlDecimal.Field<usrRmngDebitAmt> { }
        #endregion

        #region UsrSetldCreditAmt
        [PXDecimal()]
        [PXUIField(DisplayName = "Settle Credit Amt")]
        [PXDBScalar(typeof(SelectFrom<LSLedgerSettlement>.Where<LSLedgerSettlement.branchID.IsEqual<GLTran.branchID>
                                                                .And<LSLedgerSettlement.lineNbr.IsEqual<GLTran.lineNbr>
                                                                     .And<LSLedgerSettlement.module.IsEqual<GLTran.module>
                                                                          .And<LSLedgerSettlement.batchNbr.IsEqual<GLTran.batchNbr>
                                                                               .And<LSLedgerSettlement.settledCreditAmt.IsGreater<decimal0>>>>>>
                                                         .AggregateTo<GroupBy<LSLedgerSettlement.branchID,
                                                                              GroupBy<LSLedgerSettlement.module,
                                                                                      GroupBy<LSLedgerSettlement.batchNbr,
                                                                                              GroupBy<LSLedgerSettlement.lineNbr,
                                                                                                      Sum<LSLedgerSettlement.settledCreditAmt>>>>>>
                           .SearchFor<LSLedgerSettlement.settledCreditAmt>))]
        public virtual decimal? UsrSetldCreditAmt { get; set; }
        public abstract class usrSetldCreditAmt : PX.Data.BQL.BqlDecimal.Field<usrSetldCreditAmt> { }
        #endregion

        #region UsrSetldDebitAmt
        [PXDecimal()]
        [PXUIField(DisplayName = "Settle Debit Amt")]
        [PXDBScalar(typeof(SelectFrom<LSLedgerSettlement>.Where<LSLedgerSettlement.branchID.IsEqual<GLTran.branchID>
                                                                .And<LSLedgerSettlement.lineNbr.IsEqual<GLTran.lineNbr>
                                                                     .And<LSLedgerSettlement.module.IsEqual<GLTran.module>
                                                                          .And<LSLedgerSettlement.batchNbr.IsEqual<GLTran.batchNbr>
                                                                               .And<LSLedgerSettlement.settledDebitAmt.IsGreater<decimal0>>>>>>
                                                         .AggregateTo<GroupBy<LSLedgerSettlement.branchID,
                                                                              GroupBy<LSLedgerSettlement.module,
                                                                                      GroupBy<LSLedgerSettlement.batchNbr,
                                                                                              GroupBy<LSLedgerSettlement.lineNbr,
                                                                                                      Sum<LSLedgerSettlement.settledDebitAmt>>>>>>
                           .SearchFor<LSLedgerSettlement.settledDebitAmt>))]
        public virtual decimal? UsrSetldDebitAmt { get; set; }
        public abstract class usrSetldDebitAmt : PX.Data.BQL.BqlDecimal.Field<usrSetldDebitAmt> { }
        #endregion

        #endregion
    }
}
