using PX.Data;

namespace PX.Objects.AR
{
    public class ARSetupExt : PXCacheExtension<PX.Objects.AR.ARSetup>
    {
        #region UsrReqHeaderDescInINV
        [PXDBBool]
        [PXUIField(DisplayName = "Require Header Description In Invoices and Memos")]
        public virtual bool? UsrReqHeaderDescInINV { get; set; }
        public abstract class usrReqHeaderDescInINV : PX.Data.BQL.BqlBool.Field<usrReqHeaderDescInINV> { }
        #endregion
    }
}