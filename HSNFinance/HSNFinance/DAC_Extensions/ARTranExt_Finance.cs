using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AR
{
    public class ARTranExt_Finance : PXCacheExtension<ARTran>
    {
        #region UsrReasonCode
        [PXDBString(20)]
        [PXUIField(DisplayName = "Reason Code", Enabled = true)]
        public virtual string UsrReasonCode { get; set; }
        public abstract class usrReasonCode : PX.Data.BQL.BqlString.Field<usrReasonCode> { }
        #endregion

        #region UsrSettled
        [PXDBBool]
        [PXUIField(DisplayName = "Settled", Enabled = false, Visible = true)]
        public virtual bool? UsrSettled { get; set; }
        public abstract class usrSettled : PX.Data.BQL.BqlBool.Field<usrSettled> { }
        #endregion
    }
}
