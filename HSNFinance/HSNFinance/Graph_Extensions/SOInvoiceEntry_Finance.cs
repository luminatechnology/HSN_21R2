using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
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
                                 .View.Select(Base,ARDocType.CashSale, prepayment?.AdjgRefNbr).TopFirst;
                    if(mapARTran != null)
                    {
                        PXDatabase.Update<ARTran>(
                            new PXDataFieldAssign<ARTranExt_Finance.usrSettled>(true),
                            new PXDataFieldRestrict<ARTran.refNbr>(mapARTran?.RefNbr),
                            new PXDataFieldRestrict<ARTran.tranType>(mapARTran?.TranType),
                            new PXDataFieldRestrict<ARTran.lineNbr>(mapARTran?.LineNbr));
                    }
                }
            }
            return releaseResult;
        }
        #endregion
    }
}
