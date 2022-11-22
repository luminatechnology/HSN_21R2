using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS
{
    public class FSSrvOrdTypeExt_Finance : PXCacheExtension<FSSrvOrdType>
    {
        #region UsrEnableSelectRevenueAndCostAcct
        [PXDBBool]
        [PXUIField(DisplayName = "Enable Automation of Revenue and Cost Accounts by Order Types")]
        public virtual bool? UsrEnableSelectRevenueAndCostAcct { get; set; }
        public abstract class usrEnableSelectRevenueAndCostAcct : PX.Data.BQL.BqlBool.Field<usrEnableSelectRevenueAndCostAcct> { }
        #endregion
    }
}
