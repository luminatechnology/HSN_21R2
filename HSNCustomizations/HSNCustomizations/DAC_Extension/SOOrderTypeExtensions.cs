using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.SO
{
    public class SOOrderTypeExt : PXCacheExtension<SOOrderType>
    {
        #region UsrRequireAtLeastOneNonStkItemInSO
        [PXDBBool()]
        [PXUIField(DisplayName = "Require At Least One Non-Stock Item In Sales Order")]
        public virtual bool? UsrRequireAtLeastOneNonStkItemInSO { get; set; }
        public abstract class usrRequireAtLeastOneNonStkItemInSO : PX.Data.BQL.BqlBool.Field<usrRequireAtLeastOneNonStkItemInSO> { }
        #endregion

        #region UsrMandatoryNonStkItem
        [NonStockItem(DisplayName = "Mandatory Non-Stock Item")]
        public virtual int? UsrMandatoryNonStkItem { get; set; }
        public abstract class usrMandatoryNonStkItem : PX.Data.BQL.BqlInt.Field<usrMandatoryNonStkItem> { }
        #endregion
    }
}
