using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.AP
{
    [PXNonInstantiatedExtension()]
    public class APPaymentExt_ExistingColumn : PXCacheExtension<APPayment>
    {
        #region DocDesc
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault()]
        [PXUIRequired(typeof(Where<APPaymentExt_Finance.usrIsRequireHeaderDesc, Equal<True>,
                                   And<APPayment.released, Equal<False>>>))]
        public virtual string DocDesc { get; set; }
        #endregion
    }

    public class APPaymentExt_Finance : PXCacheExtension<APPayment>
    {
        #region Unbound Fields

        #region UsrIsRequireHeaderDesc
        [PXBool()]
        [PXDefault(typeof(SelectFrom<APSetup>.SearchFor<APSetupExt.usrReqHeaderDescInCHK>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBScalar(typeof(SelectFrom<APSetup>.SearchFor<APSetupExt.usrReqHeaderDescInCHK>))]
        public virtual bool? UsrIsRequireHeaderDesc { get; set; }
        public abstract class usrIsRequireHeaderDesc : PX.Data.BQL.BqlBool.Field<usrIsRequireHeaderDesc> { }
        #endregion

        #endregion
    }
}
