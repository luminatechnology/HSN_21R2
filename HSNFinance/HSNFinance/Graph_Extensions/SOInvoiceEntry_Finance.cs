using HSNCustomizations.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.FS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO
{
    public class SOInvoiceEntry_Finance : PXGraphExtension<SOInvoiceEntry>
    {
        #region Delegate Method
        public delegate IEnumerable ReleaseDelegate(PXAdapter adapter);
        [PXOverride]
        public virtual IEnumerable Release(PXAdapter adapter, ReleaseDelegate baseMethod)
        {
            var preference = SelectFrom<LUMHSNSetup>.View.Select(Base).TopFirst;
            var doc = Base.Document.Current;
            var releaseResult = baseMethod.Invoke(adapter);
            /// HSNM：Add a Process ‘Process Open Prepayments for Cash Sales’
            /// Settled ARTran when Debie memo release, according to prepayment reference
            if (doc.DocType == ARDocType.DebitMemo)
            {
                foreach (var prepayment in Base.Adjustments.View.SelectMulti().RowCast<ARAdjust2>())
                {
                    var mapARTran = SelectFrom<ARTran>
                                 .InnerJoin<ARInvoice>.On<ARTran.refNbr.IsEqual<ARInvoice.refNbr>
                                                     .And<ARInvoice.docType.IsEqual<P.AsString>>>
                                 .Where<ARTran.tranDesc.IsEqual<P.AsString>>
                                 .View.Select(Base, ARDocType.CashSale, prepayment?.AdjgRefNbr).TopFirst;
                    if (mapARTran != null)
                    {
                        PXDatabase.Update<ARTran>(
                            new PXDataFieldAssign<ARTranExt_Finance.usrSettled>(true),
                            new PXDataFieldRestrict<ARTran.refNbr>(mapARTran?.RefNbr),
                            new PXDataFieldRestrict<ARTran.tranType>(mapARTran?.TranType),
                            new PXDataFieldRestrict<ARTran.lineNbr>(mapARTran?.LineNbr));
                    }
                }
            }

            #region [HSNM] Process Open Prepayments for Cash Sales
            // Cash Sales Mapping Service Order
            if (preference?.EnableChgInvTypeOnBill ?? false)
            {
                var mapFSARTran = SelectFrom<FSARTran>
                                 .Where<FSARTran.tranType.IsEqual<P.AsString>
                                   .And<FSARTran.refNbr.IsEqual<P.AsString>>>
                                 .View.Select(Base, ARDocType.CashSale, doc?.RefNbr).TopFirst;
                if (mapFSARTran != null && doc.DocType == ARDocType.CashSale)
                {
                    try
                    {
                        // Service Order/Appointment Mapping Prepayment
                        var mapFSAdjust = SelectFrom<FSAdjust>
                                         .Where<FSAdjust.adjdOrderNbr.IsEqual<P.AsString>
                                           .And<FSAdjust.adjgDocType.IsEqual<P.AsString>>>
                                         .View.Select(Base, mapFSARTran?.ServiceOrderRefNbr, ARDocType.Prepayment).RowCast<FSAdjust>();

                        // Create Debit Memo
                        var soInvEntry = PXGraph.CreateInstance<SOInvoiceEntry>();
                        var arPaymentEntry = PXGraph.CreateInstance<ARPaymentEntry>();

                        #region Debit memo Header
                        var debitDoc = soInvEntry.Document.Cache.CreateInstance() as ARInvoice;
                        debitDoc.DocType = "DRM";
                        debitDoc = soInvEntry.Document.Insert(debitDoc);
                        debitDoc.Status = ARDocStatus.Hold;
                        debitDoc.CustomerID = Base.Document.Current?.CustomerID;
                        debitDoc.DocDesc = "Settle Open Prepayment";
                        soInvEntry.Document.Update(debitDoc);
                        #endregion

                        #region Debit memo Details & Applications
                        foreach (var item in mapFSAdjust)
                        {
                            arPaymentEntry.Document.Current = arPaymentEntry.Document.Search<ARPayment.refNbr>(item?.AdjgRefNbr, "PPM");

                            #region Details
                            var line = soInvEntry.Transactions.Cache.CreateInstance() as ARTran;
                            line.BranchID = arPaymentEntry.Document.Current?.BranchID;
                            line.TranDesc = "Settle Open Prepayment";
                            line.CuryExtPrice = arPaymentEntry.Document.Current?.CuryUnappliedBal;
                            line.AccountID = arPaymentEntry.Document.Current?.ARAccountID;
                            line.SubID = arPaymentEntry.Document.Current?.ARSubID;
                            line = soInvEntry.Transactions.Insert(line);
                            #endregion

                            #region Applications
                            var appt = soInvEntry.Adjustments.Cache.CreateInstance() as ARAdjust2;
                            appt.AdjgDocType = ARPaymentType.Prepayment;
                            appt.AdjgRefNbr = arPaymentEntry.Document.Current?.RefNbr;
                            appt.CuryAdjdAmt = arPaymentEntry.Document.Current?.CuryUnappliedBal;
                            appt = soInvEntry.Adjustments.Insert(appt);
                            #endregion
                        }
                        #endregion

                        // Save Invoice
                        soInvEntry.Save.Press();
                        soInvEntry.releaseFromHold.Press();
                        //throw new PXRedirectRequiredException(soInvEntry, true, "Invoice");
                    }
                    catch (PXRedirectRequiredException er)
                    {
                        throw er;
                    }
                    catch (Exception ex)
                    {
                        PXTrace.WriteError($"Create Debit memo failed : {ex.Message}");
                    }

                }
            }

            #endregion

            return releaseResult;
        }
        #endregion
    }
}
