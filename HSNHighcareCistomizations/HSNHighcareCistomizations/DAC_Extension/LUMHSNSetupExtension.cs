using HSNCustomizations.DAC;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNHighcareCistomizations.DAC
{
    public class LUMHSNSetupExtension : PXCacheExtension<LUMHSNSetup>
    {
        #region EnableHighcareFunction
        [PXDBBool()]
        [PXUIField(DisplayName = "Enable Highcare Function")]
        public virtual bool? EnableHighcareFunction { get; set; }
        public abstract class enableHighcareFunction : PX.Data.BQL.BqlBool.Field<enableHighcareFunction> { }
        #endregion

        #region EnableOverridePINCodetDate
        [PXDBBool()]
        [PXUIField(DisplayName = "Enable Override PIN Code Start Date")]
        public virtual bool? EnableOverridePINCodetDate { get; set; }
        public abstract class enableOverridePINCodetDate : PX.Data.BQL.BqlBool.Field<enableOverridePINCodetDate> { }
        #endregion
    }
}
