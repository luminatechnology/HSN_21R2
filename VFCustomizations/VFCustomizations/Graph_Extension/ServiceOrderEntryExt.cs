using PX.Data;
using PX.Objects.FS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.IN;
using VFCustomizations.DAC_Extension;

namespace VFCustomizations.Graph_Extension
{
    public class ServiceOrderEntry_VF_Ext : PXGraphExtension<ServiceOrderEntry>
    {
        #region Event
        public virtual void _(Events.RowDeleted<FSServiceOrder> e, PXRowDeleted baseMethod)
        {
            baseMethod?.Invoke(e.Cache,e.Args);
            // VF Customization - Update INTranExtension.usrServiceOrder if this order created from Receipt
            PXDatabase.Update<INTran>(
                             new PXDataFieldAssign<INTranExtension.usrServiceOrderNbr>(null),
                             new PXDataFieldRestrict<INTranExtension.usrServiceOrderNbr>(e.Row.RefNbr));
        }
        #endregion
    }
}
