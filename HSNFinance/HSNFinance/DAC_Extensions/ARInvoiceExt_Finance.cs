using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.AR
{
    [PXNonInstantiatedExtension()]
    public class ARInvoiceExt_ExistingColumn : PXCacheExtension<ARInvoice>
    {
        #region DocDesc
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault()]
        [PXUIRequired(typeof(Where<ARInvoiceExt_Finance.usrIsRequireHeaderDesc, Equal<True>>))]
        public virtual string DocDesc { get; set; }
        #endregion
    }
    public class ARInvoiceExt_Finance : PXCacheExtension<ARInvoice>
    {
        #region Unbound Fields

        #region UsrIsRequireHeaderDesc
        [PXBool()]
        [PXDefault(typeof(SelectFrom<ARSetup>.SearchFor<ARSetupExt.usrReqHeaderDescInINV>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBScalar(typeof(SelectFrom<ARSetup>.SearchFor<ARSetupExt.usrReqHeaderDescInINV>))]
        public virtual bool? UsrIsRequireHeaderDesc { get; set; }
        public abstract class usrIsRequireHeaderDesc : PX.Data.BQL.BqlBool.Field<usrIsRequireHeaderDesc> { }
        #endregion

        #endregion
    }
}
