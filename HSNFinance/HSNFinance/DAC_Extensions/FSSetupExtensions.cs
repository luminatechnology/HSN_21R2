using PX.Data;

namespace PX.Objects.FS
{
    public class FSSetupExt : PXCacheExtension<PX.Objects.FS.FSSetup>
    {
        #region UsrReqHeaderDescInSrvOrd
        [PXDBBool]
        [PXUIField(DisplayName = "Require Header Description In Service Orders")]
        public virtual bool? UsrReqHeaderDescInSrvOrd { get; set; }
        public abstract class usrReqHeaderDescInSrvOrd : PX.Data.BQL.BqlBool.Field<usrReqHeaderDescInSrvOrd> { }
        #endregion
    }
}