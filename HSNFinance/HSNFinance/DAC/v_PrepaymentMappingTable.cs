using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.GL;

namespace HSNFinance.DAC
{
    [Serializable]
    [PXCacheName("v_PrepaymentMappingTable")]
    public class v_PrepaymentMappingTable : IBqlTable
    {

        #region Selected
        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        #endregion

        #region CustomerID
        [CustomerActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Filterable = true, TabOrder = 2, IsKey = true)]
        [PXUIField(DisplayName = "Customer ID",Enabled = false)]
        public virtual int? CustomerID { get; set; }
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
        #endregion

        #region PrepaymentRefNbr
        [PXDBString(15, IsUnicode = true, InputMask = "", IsKey = true)]
        [PXUIField(DisplayName = "Prepayment Ref Nbr.", Enabled = false)]
        public virtual string PrepaymentRefNbr { get; set; }
        public abstract class prepaymentRefNbr : PX.Data.BQL.BqlString.Field<prepaymentRefNbr> { }
        #endregion

        #region Status
        [PXDBString(1, IsFixed = true, InputMask = "")]
        [ARDocStatus.List]
        [PXUIField(DisplayName = "Status", Enabled = false)]
        public virtual string Status { get; set; }
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
        #endregion

        #region InvoiceNbr
        [PXDBString(15, IsUnicode = true, InputMask = "",IsKey = true)]
        [PXUIField(DisplayName = "Invoice Nbr", Enabled = false)]
        public virtual string InvoiceNbr { get; set; }
        public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
        #endregion

        #region PendingSettledAmt
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Pending Settled Amt.", Enabled = false)]
        public virtual Decimal? PendingSettledAmt { get; set; }
        public abstract class pendingSettledAmt : PX.Data.BQL.BqlDecimal.Field<pendingSettledAmt> { }
        #endregion

        //#region LineNbr
        //[PXDBInt(IsKey = true)]
        //[PXUIField(DisplayName = "Line Nbr", Enabled = false)]
        //public virtual int? LineNbr { get; set; }
        //public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        //#endregion

        #region CuryTranAmt
        [PXDecimal()]
        [PXUIField(DisplayName = "Prepayment Available Amt.", Enabled = false)]
        public virtual Decimal? CuryUnappliedBal { get; set; }
        public abstract class curyUnappliedBal : PX.Data.BQL.BqlDecimal.Field<curyUnappliedBal> { }
        #endregion

        #region BranchID
        [Branch()]
        public virtual int? BranchID { get; set; }
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        #endregion

        #region AccountID
        [PXDBInt]
        [PXUIField(DisplayName = "AccountID")]
        public virtual int? AccountID { get;set;}
        public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
        #endregion

        #region SubID
        [PXDBInt]
        [PXUIField(DisplayName = "SubID")]
        public virtual int? SubID { get; set; }
        public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
        #endregion

    }
}