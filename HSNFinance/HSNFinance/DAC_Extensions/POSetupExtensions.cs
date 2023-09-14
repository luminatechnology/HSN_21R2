using PX.Data;

namespace PX.Objects.PO
{
    public class POSetupExt : PXCacheExtension<PX.Objects.PO.POSetup>
    {
        #region UsrReqHeaderDescInPO
        [PXDBBool]
        [PXUIField(DisplayName = "Require Header Description In Purchase Orders")]
        public virtual bool? UsrReqHeaderDescInPO { get; set; }
        public abstract class usrReqHeaderDescInPO : PX.Data.BQL.BqlBool.Field<usrReqHeaderDescInPO> { }
        #endregion

        #region UsrCopyHeaderDescFromPO
        [PXDBBool]
        [PXUIField(DisplayName = "Copy Header Description From Purchase Orders")]
        public virtual bool? UsrCopyHeaderDescFromPO { get; set; }
        public abstract class usrCopyHeaderDescFromPO : PX.Data.BQL.BqlBool.Field<usrCopyHeaderDescFromPO> { }
        #endregion
    }
}