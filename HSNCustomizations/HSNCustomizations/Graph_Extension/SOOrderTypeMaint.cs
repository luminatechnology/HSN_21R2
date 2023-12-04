using PX.Data;

namespace PX.Objects.SO
{
    public class SOOrderTypeMaintExt : PXGraphExtension<SOOrderTypeMaint>
    {
        #region Event Handlers
        protected void _(Events.RowSelected<SOOrderType> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            if (e.Row != null)
            {
                PXUIFieldAttribute.SetEnabled<SOOrderTypeExt.usrMandatoryNonStkItem>(e.Cache, e.Row, 
                                                                                     e.Row.GetExtension<SOOrderTypeExt>().UsrRequireAtLeastOneNonStkItemInSO ?? false);
            }
        }
        #endregion
    }
}
