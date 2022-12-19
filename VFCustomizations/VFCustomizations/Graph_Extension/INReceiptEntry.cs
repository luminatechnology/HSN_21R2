using PX.Data;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFCustomizations.DAC;
using VFCustomizations.DAC_Extension;

namespace PX.Objects.IN
{
    public class INReceiptEntry_VF_Ext : PXGraphExtension<INReceiptEntry>
    {
        public virtual void _(Events.RowSelected<INRegister> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache,e.Args);

            var isVisiable = SelectFrom<LUMVerifonePreference>.View.Select(Base).TopFirst?.EnableVFCustomizeField ?? false;
            PXUIFieldAttribute.SetVisible<INTranExtension.usrJobNo>(Base.transactions.Cache, null, isVisiable);
            PXUIFieldAttribute.SetVisible<INTranExtension.usrPhoneNo>(Base.transactions.Cache, null, isVisiable);
            PXUIFieldAttribute.SetVisible<INTranExtension.usrQtySend>(Base.transactions.Cache, null, isVisiable);
            PXUIFieldAttribute.SetVisible<INTranExtension.usrSymptom>(Base.transactions.Cache, null, isVisiable);
            PXUIFieldAttribute.SetVisible<INTranExtension.usrResolution>(Base.transactions.Cache, null, isVisiable);
            PXUIFieldAttribute.SetVisible<INTranExtension.usrOwner>(Base.transactions.Cache, null, isVisiable);
        }
    }
}
