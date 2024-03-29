﻿using PX.Data;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFCustomizations.DAC_Extension
{
    public class SOShipLineExtension : PXCacheExtension<SOShipLine>
    {
        #region UsrPhoneNo
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Phone Number")]
        public virtual string UsrPhoneNo { get; set; }
        public abstract class usrPhoneNo : PX.Data.BQL.BqlString.Field<usrPhoneNo> { }
        #endregion

        #region UsrNodeName
        [PXString(IsUnicode = true)]
        [PXUIField(DisplayName = "Node Name", Visible = false, Enabled = false)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string UsrNodeName { get; set; }
        public abstract class usrNodeName : PX.Data.BQL.BqlString.Field<usrNodeName> { }
        #endregion

    }
}
