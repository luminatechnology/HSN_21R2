using System;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.AP
{
    public class APRegisterExt : PXCacheExtension<PX.Objects.AP.APRegister>
    {
        #region UsrInvoiceDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Invoice Date")]
        [PXDefault()]
        [PXFormula(typeof(IIf<Where<APRegister.origModule, In3<BatchModule.moduleEP, BatchModule.modulePO>>, APRegister.docDate, 
                              IIf<Where<APPayment.docType, In3<APDocType.check, APDocType.refund, APDocType.voidCheck, APDocType.prepayment, APDocType.debitAdj, APDocType.voidRefund>>, APPayment.adjDate, Null>>))]
        public virtual DateTime? UsrInvoiceDate { get; set; }
        public abstract class usrInvoiceDate : PX.Data.BQL.BqlDateTime.Field<usrInvoiceDate> { }
        #endregion
    }
}