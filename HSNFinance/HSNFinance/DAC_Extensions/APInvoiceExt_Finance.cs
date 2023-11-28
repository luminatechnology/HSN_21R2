using System;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.GL;

namespace PX.Objects.AP
{
    [PXNonInstantiatedExtension()]
    public class APInvoiceExt_ExistingColumn : PXCacheExtension<APInvoice>
    {
        #region DocDesc
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault()]
        [PXUIRequired(typeof(Where<APInvoiceExt_Finance.usrIsRequireHeaderDesc, Equal<True>,
                                   And<APInvoice.released, Equal<False>>>))]
        public virtual string DocDesc { get; set; }
        #endregion
    }

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

        #region Unbound Fields

        #region UsrIsRequireHeaderDesc
        [PXBool()]
        [PXDefault(typeof(SelectFrom<APSetup>.SearchFor<APSetupExt.usrReqHeaderDescInINV>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBScalar(typeof(SelectFrom<APSetup>.SearchFor<APSetupExt.usrReqHeaderDescInINV>))]
        public virtual bool? UsrIsRequireHeaderDesc { get; set; }
        public abstract class usrIsRequireHeaderDesc : PX.Data.BQL.BqlBool.Field<usrIsRequireHeaderDesc> { }
        #endregion

        #endregion
    }
}