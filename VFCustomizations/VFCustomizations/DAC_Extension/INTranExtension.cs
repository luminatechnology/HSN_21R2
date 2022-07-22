using PX.Data;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFCustomizations.DAC_Extension
{
    public class INTranExtension : PXCacheExtension<INTran>
    {
        #region UsrJobNo
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Job No", Visible = false)]
        public virtual string UsrJobNo { get; set; }
        public abstract class usrJobNo : PX.Data.BQL.BqlString.Field<usrJobNo> { }
        #endregion

        #region UsrPhoneNo
        [PXDBString(20, IsUnicode = true)]
        [PXUIField(DisplayName = "Phone No", Visible = false)]
        public virtual string UsrPhoneNo { get; set; }
        public abstract class usrPhoneNo : PX.Data.BQL.BqlString.Field<usrPhoneNo> { }
        #endregion

        #region UsrQtySend
        [PXDBDecimal]
        [PXUIField(DisplayName = "Qty Send", Visible = false)]
        public virtual decimal? UsrQtySend { get; set; }
        public abstract class usrQtySend : PX.Data.BQL.BqlDecimal.Field<usrQtySend> { }
        #endregion
    }
}
