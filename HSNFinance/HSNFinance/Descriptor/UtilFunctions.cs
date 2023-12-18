using System;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using HSNFinance.DAC;

namespace ReportUDF
{
    public class UtilFunctions
    {
        /// <summary>
        /// Check whether the settlement number has a corresponding record that is greater than the end date and all have been settled. 
        /// </summary>
        public bool IncludeSettledAmt(DateTime endDate, string settlementNbr)
        {
            if (string.IsNullOrEmpty(settlementNbr)) { return true; }

            return SelectFrom<LSLedgerSettlement>.Where<LSLedgerSettlement.tranDate.IsGreater<@P.AsDateTime>
                                                        .And<LSLedgerSettlement.settlementNbr.IsEqual<@P.AsString>>>
                                                 .OrderBy<Asc<LSLedgerSettlement.tranDate,
                                                              Asc<LSLedgerSettlement.batchNbr>>>.View.Select(PXGraph.CreateInstance<PXGraph>(), endDate, settlementNbr).Any();
        }

        /// <summary>
        /// Summarize all settlement records by the report parameter of "End Date", and return true to display line if the report parameter of GLTran PK is the same as the aggregation record and has no balance.
        /// </summary>
        public bool DisplayGLTran(DateTime endDate, string module, string batchNbr, int lineNbr)
        {
            //if (string.IsNullOrEmpty(settlementNbr)) { return true; }

            LSLedgerSettlement aggregate = SelectFrom<LSLedgerSettlement>.Where<LSLedgerSettlement.tranDate.IsLessEqual<@P.AsDateTime>
                                                                                .And<LSLedgerSettlement.module.IsEqual<@P.AsString>
                                                                                     .And<LSLedgerSettlement.batchNbr.IsEqual<@P.AsString>
                                                                                          .And<LSLedgerSettlement.lineNbr.IsEqual<@P.AsInt>>>>>
                                                                         .AggregateTo<GroupBy<LSLedgerSettlement.batchNbr,
                                                                                      GroupBy<LSLedgerSettlement.lineNbr,
                                                                                              Sum<LSLedgerSettlement.settledDebitAmt,
                                                                                              Sum<LSLedgerSettlement.settledCreditAmt,
                                                                                              Avg<LSLedgerSettlement.origDebitAmt,
                                                                                              Avg<LSLedgerSettlement.origCreditAmt>>>>>>>
                                                                         .OrderBy<Asc<LSLedgerSettlement.tranDate,
                                                                                      Asc<LSLedgerSettlement.batchNbr>>>
                                                                         .View.Select(PXGraph.CreateInstance<PXGraph>(), endDate, module, batchNbr, lineNbr);

            return (aggregate?.OrigCreditAmt - aggregate?.SettledCreditAmt != decimal.Zero) ||
                   (aggregate?.OrigDebitAmt- aggregate?.SettledDebitAmt != decimal.Zero);

            //List<LSLedgerSettlement> list = SelectFrom<LSLedgerSettlement>.Where<LSLedgerSettlement.tranDate.IsLessEqual<@P.AsDateTime>>
            //                                                                        .OrderBy<Asc<LSLedgerSettlement.tranDate,
            //                                                                                    Asc<LSLedgerSettlement.batchNbr>>>
            //                                                                .View.Select(new PXGraph(), endDate).RowCast<LSLedgerSettlement>().ToList();
            //var aggregate = list.GroupBy(x => new { x.SettlementNbr }).Select(x => new
            //{
            //    SettlementNbr = x.Key.SettlementNbr,
            //    TotalDebit = x.Sum(y => y.SettledDebitAmt),
            //    TotalCredit = x.Sum(y => y.SettledCreditAmt)
            //}).ToList();
            //return aggregate.Find(x => x.TotalCredit - x.TotalDebit != decimal.Zero && x.SettlementNbr == settlementNbr)?.SettlementNbr != null;
        }
    }
}
