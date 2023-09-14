using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.SO
{
    [PXNonInstantiatedExtension()]
    public class SOOrderExt_ExistingColumn : PXCacheExtension<SOOrder>
    {
        #region OrderDesc
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault()]
        [PXUIRequired(typeof(Where<SOOrderExt_Finance.usrIsRequireHeaderDesc, Equal<True>>))]
        public virtual string OrderDesc { get; set; }
        #endregion
    }

    public class SOOrderExt_Finance : PXCacheExtension<SOOrder>
    {
        #region Unbound Fields

        #region UsrIsRequireHeaderDesc
        [PXBool()]
        [PXDefault(typeof(SelectFrom<SOSetup>.SearchFor<SOSetupExt.usrReqHeaderDescInSO>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBScalar(typeof(SelectFrom<SOSetup>.SearchFor<SOSetupExt.usrReqHeaderDescInSO>))]
        public virtual bool? UsrIsRequireHeaderDesc { get; set; }
        public abstract class usrIsRequireHeaderDesc : PX.Data.BQL.BqlBool.Field<usrIsRequireHeaderDesc> { }
        #endregion

        #endregion
    }
}
