using System;
using PX.Data;
using PX.Objects.FS;
using PX.Objects.IN;
using PX.Objects.GL;
using PX.Objects.CS;

namespace HSNFinance
{
    [Serializable]
    [PXCacheName("LUMRevenueInventoryAccounts")]
    public class LUMRevenueInventoryAccounts : IBqlTable
    {
        #region SrvOrderType
        [PXDBString(4, IsKey = true, IsFixed = true, InputMask = "")]
        [PXUIField(DisplayName = "Srv Order Type")]
        [PXDefault]
        [FSSelectorSrvOrdTypeNOTQuote]
        public virtual string SrvOrderType { get; set; }
        public abstract class srvOrderType : PX.Data.BQL.BqlString.Field<srvOrderType> { }
        #endregion

        #region AccountID
        [PXDBInt()]
        [PXDefault]
        [PXUIField(DisplayName = "Account")]
        [PXSelector(typeof(Search<Account.accountID>),
                    typeof(Account.accountCD),
                    typeof(Account.description),
                    SubstituteKey = typeof(Account.accountCD))]
        public virtual int? AccountID { get; set; }
        public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
        #endregion

        #region SubAccountID
        [PXDBInt()]
        [PXDefault]
        [PXUIField(DisplayName = "Sub Account")]
        [PXSelector(typeof(Search<Sub.subID>),
                    typeof(Sub.subCD),
                    typeof(Sub.description),
                    SubstituteKey = typeof(Sub.subCD))]
        public virtual int? SubAccountID { get; set; }
        public abstract class subAccountID : PX.Data.BQL.BqlInt.Field<subAccountID> { }
        #endregion

        #region RevenueReasonCode
        [PXDBString(20, IsUnicode = true, InputMask = "")]
        [PXDefault]
        [PXUIField(DisplayName = "Reason Code")]
        [PXSelector(typeof(Search<ReasonCode.reasonCodeID>),
                    typeof(ReasonCode.descr),
                    DescriptionField = typeof(ReasonCode.descr))]
        public virtual string RevenueReasonCode { get; set; }
        public abstract class revenueReasonCode : PX.Data.BQL.BqlString.Field<revenueReasonCode> { }
        #endregion

        #region Noteid
        [PXNote()]
        public virtual Guid? Noteid { get; set; }
        public abstract class noteid : PX.Data.BQL.BqlGuid.Field<noteid> { }
        #endregion

        #region CreatedByID
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID { get; set; }
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        #endregion

        #region CreatedByScreenID
        [PXDBCreatedByScreenID()]
        public virtual string CreatedByScreenID { get; set; }
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        #endregion

        #region CreatedDateTime
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        #endregion

        #region LastModifiedByID
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID { get; set; }
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        #endregion

        #region LastModifiedByScreenID
        [PXDBLastModifiedByScreenID()]
        public virtual string LastModifiedByScreenID { get; set; }
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        #endregion

        #region LastModifiedDateTime
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        #endregion

        #region Tstamp
        [PXDBTimestamp()]
        [PXUIField(DisplayName = "Tstamp")]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion
    }
}