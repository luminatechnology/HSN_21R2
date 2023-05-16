using System;
using PX.Data;

namespace HSNHighcareCistomizations.DAC
{
    [Serializable]
    [PXCacheName("LUMHighcarePreference")]
    public class LUMHighcarePreference : IBqlTable
    {
        #region ReturnUrl
        [PXDBString(1024, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Return Url")]
        public virtual string ReturnUrl { get; set; }
        public abstract class returnUrl : PX.Data.BQL.BqlString.Field<returnUrl> { }
        #endregion

        #region SecretKey
        [PXDBString(1024, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Secret Key")]
        public virtual string SecretKey { get; set; }
        public abstract class secretKey : PX.Data.BQL.BqlString.Field<secretKey> { }
        #endregion
    }
}