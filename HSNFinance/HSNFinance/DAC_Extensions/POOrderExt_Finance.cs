using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PO
{
    [PXNonInstantiatedExtension()]
    public class POOrderExt_ExistingColumn : PXCacheExtension<POOrder>
    {
        #region OrderDesc
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault()]
        [PXUIRequired(typeof(Where<POOrderExt_Finance.usrIsRequireHeaderDesc, Equal<True>>))]
        public virtual string OrderDesc { get; set; }
        #endregion
    }

    public class POOrderExt_Finance : PXCacheExtension<POOrder>
    {
        #region Unbound Fields

        #region UsrIsRequireHeaderDesc
        [PXBool()]
        [PXDefault(typeof(SelectFrom<POSetup>.SearchFor<POSetupExt.usrReqHeaderDescInPO>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBScalar(typeof(SelectFrom<POSetup>.SearchFor<POSetupExt.usrReqHeaderDescInPO>))]
        public virtual bool? UsrIsRequireHeaderDesc { get; set; }
        public abstract class usrIsRequireHeaderDesc : PX.Data.BQL.BqlBool.Field<usrIsRequireHeaderDesc> { }
        #endregion

        #endregion
    }
}
