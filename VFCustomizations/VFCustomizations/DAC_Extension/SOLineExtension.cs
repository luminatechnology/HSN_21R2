using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.SO;
namespace VFCustomizations.DAC_Extension
{
    public class SOLineExtension : PXCacheExtension<SOLine>
    {
        #region UsrForMerchant
        [PXDBString(1024,IsUnicode = true)]
        [PXUIField(DisplayName = "For Merchant", Visible = false)]
        public virtual string UsrForMerchant { get;set;}
        public abstract class usrForMerchant : PX.Data.BQL.BqlString.Field<usrForMerchant> { }
        #endregion
    }
}
