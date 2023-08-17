using PX.Data;

namespace PX.Objects.SO
{
    public class SOOrderTypeExt : PXCacheExtension<SOOrderType>
    {
        #region UsrRequireAtLeastOneNonStkItemInSO
        [PXDBBool()]
        [PXUIField(DisplayName = "Require At Least One Non-Stock Item With Amount Greater Than 0 In SO")]
        public virtual bool? UsrRequireAtLeastOneNonStkItemInSO { get; set; }
        public abstract class usrRequireAtLeastOneNonStkItemInSO : PX.Data.BQL.BqlBool.Field<usrRequireAtLeastOneNonStkItemInSO> { }
        #endregion
    }
}
