using PX.Data;
using PX.Objects.AR;
using eGUICustomization4HSN.StringList;
using eGUICustomization4HSN.Descriptor;

namespace PX.Objects.SO
{
    public class SOInvoiceEntry_Extension : PXGraphExtension<SOInvoiceEntry>
    {
        #region Event Handlers
        protected virtual void _(Events.RowUpdating<SOOrder> e, PXRowUpdating baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            var invoice = Base.Document.Current;

            if (e.Row != null && invoice != null && TWNGUIValidation.ActivateTWGUI(Base))
            {
                Base.Document.Cache.SetValue<ARRegisterExt.usrTaxNbr>(invoice, GetSOrderUDFValue(e.Row, ARRegisterExt.TaxNbrName));
                Base.Document.Cache.SetValue(invoice, "UsrGUITitle", GetSOrderUDFValue(e.Row, "GUITITLE"));

                string carrierID = (string)GetSOrderUDFValue(e.Row, "GUICARRIER");
                Base.Document.Cache.SetValue<ARRegisterExt.usrCarrierID>(invoice, carrierID);

                string nPONbr = (string)GetSOrderUDFValue(e.Row, "GUINPONBR");
                Base.Document.Cache.SetValue<ARRegisterExt.usrNPONbr>(invoice, nPONbr);
                Base.Document.Cache.SetValue<ARRegisterExt.usrB2CType>(invoice, !string.IsNullOrEmpty(carrierID) ? TWNB2CType.MC :
                                                                                                                 !string.IsNullOrEmpty(nPONbr) ? TWNB2CType.NPO :
                                                                                                                                                 TWNB2CType.DEF);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Return the User-Defined Field value on Sales Order depend on differnt attribute ID.
        /// </summary>
        public virtual object GetSOrderUDFValue(SOOrder order, string SOrderUDF_Attr, PXCache sOCache = null)
        {
            sOCache = sOCache ?? Base.soorder.Cache;

            var order_State = sOCache.GetValueExt(order, $"{CS.Messages.Attribute}{SOrderUDF_Attr}") as PXFieldState;

            return order_State?.Value;
        }
        #endregion
    }
}