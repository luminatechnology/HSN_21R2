using System;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.AP
{
    public class APInvoiceExt_Finance : PXCacheExtension<APInvoice>
    {
        #region UsrInvoiceDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Invoice Date")]
        [PXDefault()]
        [PXFormula(typeof(IIf<Where<APRegister.origModule, In3<BatchModule.moduleEP, BatchModule.modulePO>>, APRegister.docDate, Null>))]
        public virtual DateTime? UsrInvoiceDate { get; set; }
        public abstract class usrInvoiceDate : PX.Data.BQL.BqlDateTime.Field<usrInvoiceDate> { }
        #endregion
    }
}