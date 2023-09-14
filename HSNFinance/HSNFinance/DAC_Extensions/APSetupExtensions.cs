using PX.Data;

namespace PX.Objects.AP
{
    public class APSetupExt : PXCacheExtension<PX.Objects.AP.APSetup>
    {
        #region UsrReqHeaderDescInCHK
        [PXDBBool]
        [PXUIField(DisplayName = "Require Header Description In Checks and Payments")]
        public virtual bool? UsrReqHeaderDescInCHK { get; set; }
        public abstract class usrReqHeaderDescInCHK : PX.Data.BQL.BqlBool.Field<usrReqHeaderDescInCHK> { }
        #endregion

        #region UsrReqHeaderDescInINV
        [PXDBBool]
        [PXUIField(DisplayName = "Require Header Description In Bills and Adjustments")]
        public virtual bool? UsrReqHeaderDescInINV { get; set; }
        public abstract class usrReqHeaderDescInINV : PX.Data.BQL.BqlBool.Field<usrReqHeaderDescInINV> { }
        #endregion
    }
}