using PX.Data;

namespace PX.Objects.IN
{
    public class INSetupExt : PXCacheExtension<PX.Objects.IN.INSetup>
    {
        #region UsrReqHeaderDescInInventoryTran
        [PXDBBool]
        [PXUIField(DisplayName = "Require Header Description in Inventory Transactions")]
        public virtual bool? UsrReqHeaderDescInInventoryTran { get; set; }
        public abstract class usrReqHeaderDescInInventoryTran : PX.Data.BQL.BqlBool.Field<usrReqHeaderDescInInventoryTran> { }
        #endregion
    }
}