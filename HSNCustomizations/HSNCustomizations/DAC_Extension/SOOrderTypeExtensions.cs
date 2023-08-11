using PX.Data;

namespace PX.Objects.SO
{
    public class SOOrderTypeExt : PXCacheExtension<SOOrderType>
    {
        #region UsrRequireAtLeastOneSrvItemInSO
        [PXDBBool()]
        [PXUIField(DisplayName = "Require At Least One Service Item With Amount Greater Than 0 In Sales Order")]
        public virtual bool? UsrRequireAtLeastOneSrvItemInSO { get; set; }
        public abstract class usrRequireAtLeastOneSrvItemInSO : PX.Data.BQL.BqlBool.Field<usrRequireAtLeastOneSrvItemInSO> { }
        #endregion
    }
}
