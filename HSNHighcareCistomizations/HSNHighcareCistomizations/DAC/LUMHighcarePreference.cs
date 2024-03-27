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
        [PXUIField(DisplayName = "Access Token")]
        public virtual string SecretKey { get; set; }
        public abstract class secretKey : PX.Data.BQL.BqlString.Field<secretKey> { }
        #endregion

        #region LoginTokenUrl
        [PXDBString(1024, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Login Token Url")]
        public virtual string LoginTokenUrl { get; set; }
        public abstract class loginTokenUrl : PX.Data.BQL.BqlString.Field<loginTokenUrl> { }
        #endregion

        #region ShowCouponUrl
        [PXDBString(1024, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Show Coupon Url")]
        public virtual string ShowCouponUrl { get; set; }
        public abstract class showCouponUrl : PX.Data.BQL.BqlString.Field<showCouponUrl> { }
        #endregion

        #region RedeemCouponUrl
        [PXDBString(1024, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Redeem Coupon Url")]
        public virtual string RedeemCouponUrl { get; set; }
        public abstract class redeemCouponUrl : PX.Data.BQL.BqlString.Field<redeemCouponUrl> { }
        #endregion

        #region Email
        [PXDBString(256, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Email")]
        public virtual string Email { get; set; }
        public abstract class email : PX.Data.BQL.BqlString.Field<email> { }
        #endregion

        #region Password
        [PXDBString(256, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Password")]
        public virtual string Password { get; set; }
        public abstract class password : PX.Data.BQL.BqlString.Field<password> { }
        #endregion

        #region ExpiresTime
        [PXDBDateAndTime]
        [PXUIField(DisplayName = "Expires Time", Enabled = false, Visible = false)]
        public virtual DateTime? ExpiresTime { get; set; }
        public abstract class expiresTime : PX.Data.BQL.BqlDateTime.Field<expiresTime> { }
        #endregion
    }
}