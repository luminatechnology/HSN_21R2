using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.SO;

namespace PX.Objects.AR
{
    [PXNonInstantiatedExtension()]
    public class ARPaymentExt_ExistingColumn : PXCacheExtension<ARPayment>
    {
        #region DocDesc
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault()]
        [PXUIRequired(typeof(Where<ARPaymentExt_Finance.usrIsRequireHeaderDesc, Equal<True>,
                                   And<ARPayment.released, Equal<False>>>))]
        public virtual string DocDesc { get; set; }
        #endregion
    }

    public class ARPaymentExt_Finance : PXCacheExtension<ARPayment>
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
