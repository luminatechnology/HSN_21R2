using PX.Data;

namespace PX.Objects.SO
{
    public class SOSetupExt : PXCacheExtension<PX.Objects.SO.SOSetup>
    {
        #region UsrReqHeaderDescInSO
        [PXDBBool]
        [PXUIField(DisplayName = "Require Header Description In Sales Orders")]
        public virtual bool? UsrReqHeaderDescInSO { get; set; }
        public abstract class usrReqHeaderDescInSO : PX.Data.BQL.BqlBool.Field<usrReqHeaderDescInSO> { }
        #endregion

        #region UsrCopyHeaderDescFromSO
        [PXDBBool]
        [PXUIField(DisplayName = "Copy Header Description From Sales Orders")]
        public virtual bool? UsrCopyHeaderDescFromSO { get; set; }
        public abstract class usrCopyHeaderDescFromSO : PX.Data.BQL.BqlBool.Field<usrCopyHeaderDescFromSO> { }
        #endregion
    }
}