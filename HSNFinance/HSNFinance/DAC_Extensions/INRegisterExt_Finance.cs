using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.IN
{
    [PXNonInstantiatedExtension()]
    public class INRegisterExt_ExistingColumn : PXCacheExtension<INRegister>
    {
        #region TranDesc
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault()]
        [PXUIRequired(typeof(Where<INRegisterExt_Finance.usrIsRequireHeaderDesc, Equal<True>,
                                   And<INRegister.docType.IsIn<INDocType.receipt, INDocType.issue, INDocType.transfer>>>))]
        public virtual string TranDesc { get; set; }
        #endregion
    }

    public class INRegisterExt_Finance : PXCacheExtension<INRegister>
    {
        #region Unbound Fields

        #region UsrIsRequireHeaderDesc
        [PXBool()]
        [PXDBScalar(typeof(SelectFrom<INSetup>.SearchFor<INSetupExt.usrReqHeaderDescInInventoryTran>))]
        public virtual bool? UsrIsRequireHeaderDesc { get; set; }
        public abstract class usrIsRequireHeaderDesc : PX.Data.BQL.BqlBool.Field<usrIsRequireHeaderDesc> { }
        #endregion

        #endregion
    }
}
