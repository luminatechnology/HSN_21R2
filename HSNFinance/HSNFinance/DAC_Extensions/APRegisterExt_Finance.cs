using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.AP
{
    [PXNonInstantiatedExtension()]
    public class APRegisterExt_ExistingColumn : PXCacheExtension<APRegister>
    {
        #region DocDesc
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault()]
        [PXUIRequired(typeof(Where<APRegisterExt_Finance.usrIsRequireHeaderDesc, Equal<True>>))]
        public virtual string DocDesc { get; set; }
        #endregion
    }

    public class APRegisterExt_Finance : PXCacheExtension<APRegister>
    {
        #region Unbound Fields

        #region UsrIsRequireHeaderDesc
        [PXBool()]
        [PXDBScalar(typeof(SelectFrom<APSetup>.SearchFor<APSetupExt.usrReqHeaderDescInINV>))]
        public virtual bool? UsrIsRequireHeaderDesc { get; set; }
        public abstract class usrIsRequireHeaderDesc : PX.Data.BQL.BqlBool.Field<usrIsRequireHeaderDesc> { }
        #endregion

        #endregion
    }
}
