using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.FS
{
    [PXNonInstantiatedExtension()]
    public class FSServiceOrderExt_ExistingColumn : PXCacheExtension<FSServiceOrder>
    {
        #region DocDesc
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault()]
        [PXUIRequired(typeof(Where<FSServiceOrderExt_Finance.usrIsRequireHeaderDesc, Equal<True>>))]
        public virtual string DocDesc { get; set; }
        #endregion
    }

    public class FSServiceOrderExt_Finance : PXCacheExtension<FSServiceOrder>
    {
        #region Unbound Fields

        #region UsrIsRequireHeaderDesc
        [PXBool()]
        [PXDBScalar(typeof(SelectFrom<FSSetup>.SearchFor<FSSetupExt.usrReqHeaderDescInSrvOrd>))]
        public virtual bool? UsrIsRequireHeaderDesc { get; set; }
        public abstract class usrIsRequireHeaderDesc : PX.Data.BQL.BqlBool.Field<usrIsRequireHeaderDesc> { }
        #endregion

        #endregion
    }
}
