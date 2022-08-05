using System;
using PX.Data;

namespace VFCustomizations.DAC
{
    [Serializable]
    [PXCacheName("LUMVerifonePreference")]
    public class LUMVerifonePreference : IBqlTable
    {
        #region Username
        [PXDBString(100, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Username")]
        public virtual string Username { get; set; }
        public abstract class username : PX.Data.BQL.BqlString.Field<username> { }
        #endregion

        #region Password
        [PXRSACryptString(200, IsUnicode = true)]
        //[PXDBString(200, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Password")]
        public virtual string Password { get; set; }
        public abstract class password : PX.Data.BQL.BqlString.Field<password> { }
        #endregion

        #region AccessTokenURL
        [PXDBString(200, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Access Token URL")]
        public virtual string AccessTokenURL { get; set; }
        public abstract class accessTokenURL : PX.Data.BQL.BqlString.Field<accessTokenURL> { }
        #endregion

        #region Api3001url
        [PXDBString(200, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Api3001url")]
        public virtual string Api3001url { get; set; }
        public abstract class api3001url : PX.Data.BQL.BqlString.Field<api3001url> { }
        #endregion

        #region Api6001url
        [PXDBString(200, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Api6001url")]
        public virtual string Api6001url { get; set; }
        public abstract class api6001url : PX.Data.BQL.BqlString.Field<api6001url> { }
        #endregion

        #region Api2101url
        [PXDBString(200, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Api2101url")]
        public virtual string Api2101url { get; set; }
        public abstract class api2101url : PX.Data.BQL.BqlString.Field<api2101url> { }
        #endregion

        #region Api2102url
        [PXDBString(200, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Api2102url")]
        public virtual string Api2102url { get; set; }
        public abstract class api2102url : PX.Data.BQL.BqlString.Field<api2102url> { }
        #endregion
    }
}