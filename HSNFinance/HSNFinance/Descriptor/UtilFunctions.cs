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
        /// Check whether the settlement number has a corresponding record that is greater than the end date. 
        /// --Summarize all settlement records by the report parameter of "End Date", If the report parameter of the "Settlement Nbr." has the same balance as the aggregated record, then return true.--
        /// </summary>
        public bool IncludeSettledAmt(DateTime endDate, string settlementNbr)
        {
            if (string.IsNullOrEmpty(settlementNbr)) { return true; }

            return SelectFrom<LSLedgerSettlement>.Where<LSLedgerSettlement.tranDate.IsGreater<@P.AsDateTime>
                                                        .And<LSLedgerSettlement.settlementNbr.IsEqual<@P.AsString>>>
                                                 .OrderBy<Asc<LSLedgerSettlement.tranDate,
                                                              Asc<LSLedgerSettlement.batchNbr>>>.View.Select(PXGraph.CreateInstance<PXGraph>(), endDate, settlementNbr).Any();
            //List<LSLedgerSettlement> list = SelectFrom<LSLedgerSettlement>.Where<LSLedgerSettlement.tranDate.IsLessEqual<@P.AsDateTime>>
            //                                                                        .OrderBy<Asc<LSLedgerSettlement.tranDate,
            //                                                                                    Asc<LSLedgerSettlement.batchNbr>>>.View.Select(new PXGraph(), endDate).RowCast<LSLedgerSettlement>().ToList();
            //var aggregate = list.GroupBy(x => new { x.SettlementNbr }).Select(x => new  {
            //                                                                                SettlementNbr = x.Key.SettlementNbr,
            //                                                                                TotalDebit = x.Sum(y => y.SettledDebitAmt),
            //                                                                                TotalCredit = x.Sum(y => y.SettledCreditAmt)
            //                                                                            }).ToList();
            //return aggregate.Find(x => x.TotalCredit - x.TotalDebit == decimal.Zero && x.SettlementNbr == settlementNbr)?.SettlementNbr != null;
        }

        /// <summary>
        /// Summarize all settlement records by the report parameter of "End Date", and return true to display line if the report parameter of the "Settlement Nbr." is the same as the aggregation record and has no balance.
        /// </summary>
        public bool DisplayGLTran(DateTime endDate, string settlementNbr)
        {
            if (string.IsNullOrEmpty(settlementNbr)) { return true; }

            LSLedgerSettlement aggregate = SelectFrom<LSLedgerSettlement>.Where<LSLedgerSettlement.tranDate.IsLessEqual<@P.AsDateTime>
                                                                                .And<LSLedgerSettlement.settlementNbr.IsEqual<@P.AsString>>>
                                                                         .AggregateTo<GroupBy<LSLedgerSettlement.settlementNbr,
                                                                                              Sum<LSLedgerSettlement.settledDebitAmt,
                                                                                                  Sum<LSLedgerSettlement.settledCreditAmt>>>>
                                                                         .OrderBy<Asc<LSLedgerSettlement.tranDate,
                                                                                      Asc<LSLedgerSettlement.batchNbr>>>
                                                                         .View.Select(PXGraph.CreateInstance<PXGraph>(), endDate, settlementNbr);

            return (aggregate.SettledCreditAmt - aggregate.SettledDebitAmt) != decimal.Zero;

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
