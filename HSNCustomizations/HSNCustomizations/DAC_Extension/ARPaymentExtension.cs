using PX.Data;
using PX.Objects.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNCustomizations.DAC_Extension
{
    public class ARPaymentExt : PXCacheExtension<ARPayment>
    {
        #region [Phase II - SCB Refund]

        #region UsrSCBPaymentRefundDateTime
        [PXDBDate(PreserveTime = true, InputMask = "g")]
        [PXUIField(DisplayName = "SCB Payment Refund DateTime", Enabled = false)]
        public virtual DateTime? UsrSCBPaymentRefundDateTime { get; set; }
        public abstract class usrSCBPaymentRefundDateTime : PX.Data.BQL.BqlDateTime.Field<usrSCBPaymentRefundDateTime> { }
        #endregion

        #region UsrSCBPaymentRefundExported
        [PXDBBool()]
        [PXUIField(DisplayName = "SCB Payment Refund Exported", Enabled = false)]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? UsrSCBPaymentRefundExported { get; set; }
        public abstract class usrSCBPaymentRefundExported : PX.Data.BQL.BqlBool.Field<usrSCBPaymentRefundExported> { }
        #endregion

        #region UsrBankSwiftAttributes
        [PXString(255)]
        [PXUIField(DisplayName = "Bank Swift Name", Enabled = false)]
        public virtual string UsrBankSwiftAttributes { get; set; }
        public abstract class usrBankSwiftAttributes : PX.Data.BQL.BqlString.Field<usrBankSwiftAttributes> { }
        #endregion

        #region UsrBankAccnamattributes
        [PXString(255)]
        [PXUIField(DisplayName = "Bank Account Name", Enabled = false)]
        public virtual string UsrBankAccnamattributes { get; set; }
        public abstract class usrBankAccnamattributes : PX.Data.BQL.BqlString.Field<usrBankAccnamattributes> { }
        #endregion

        #region UsrBankAccNbrttributes
        [PXString(255)]
        [PXUIField(DisplayName = "Bank Account Number", Enabled = false)]
        public virtual string UsrBankAccNbrttributes { get; set; }
        public abstract class usrBankAccNbrttributes : PX.Data.BQL.BqlString.Field<usrBankAccNbrttributes> { }
        #endregion

        #endregion
    }
}
