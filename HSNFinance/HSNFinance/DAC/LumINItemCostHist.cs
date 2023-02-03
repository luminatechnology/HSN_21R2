using PX.Data;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNFinance.DAC
{
    [Serializable]
    [PXCacheName("LumINItemCostHist")]
    public class LumINItemCostHist : IBqlTable
    {
        #region Selected
        /// <summary>
        /// Indicates whether the record is selected for processing.
        /// </summary>
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        #endregion

        #region Id
        [PXDBIdentity()]
        [PXUIField(DisplayName = "ID", Visible = false)]
        public virtual int? Id { get; set; }
        public abstract class id : PX.Data.BQL.BqlInt.Field<id> { }
        #endregion

        #region InventoryID
        [Inventory(Filterable = true, DirtyRead = true, Enabled = false, DisplayName = "Inventory ID", IsKey = true)]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion

        #region InventoryCD
        [PXDBString(30, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Inventory CD")]
        public virtual string InventoryCD { get; set; }
        public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }
        #endregion

        #region ItemDescr
        [PXDBString(256, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Item Description")]
        public virtual string ItemDescr { get; set; }
        public abstract class itemDescr : PX.Data.BQL.BqlString.Field<itemDescr> { }
        #endregion

        #region ItemClassID
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Item Class ID")]
        public virtual int? ItemClassID { get; set; }
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
        #endregion

        #region ItemClassCD
        [PXDBString(30, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Item Class")]
        public virtual string ItemClassCD { get; set; }
        public abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }
        #endregion

        #region ItemClassDescr
        [PXDBString(256, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Item Class Description")]
        public virtual string ItemClassDescr { get; set; }
        public abstract class itemClassDescr : PX.Data.BQL.BqlString.Field<itemClassDescr> { }
        #endregion

        #region WareHouseID_SiteID
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Ware House ID")]
        public virtual int? WareHouseID_SiteID { get; set; }
        public abstract class wareHouseID_SiteID : PX.Data.BQL.BqlInt.Field<wareHouseID_SiteID> { }
        #endregion
        #region WareHouseID_SiteCD
        [PXDBString(30, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "WareHouse")]
        public virtual string WareHouseID_SiteCD { get; set; }
        public abstract class wareHouseID_SiteCD : PX.Data.BQL.BqlString.Field<wareHouseID_SiteCD> { }
        #endregion
        #region WareHouse_SiteID_Descr
        [PXDBString(128, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "WareHouse Description")]
        public virtual string WareHouse_SiteID_Descr { get; set; }
        public abstract class wareHouse_SiteID_Descr : PX.Data.BQL.BqlString.Field<wareHouse_SiteID_Descr> { }
        #endregion

        #region EndingQty_FinYtdQty
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Ending Qty")]
        public virtual Decimal? EndingQty_FinYtdQty { get; set; }
        public abstract class endingQty_FinYtdQty : PX.Data.BQL.BqlDecimal.Field<endingQty_FinYtdQty> { }
        #endregion

        #region EndingCost_FinYtdCost
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Ending Cost")]
        public virtual Decimal? EndingCost_FinYtdCost { get; set; }
        public abstract class endingCost_FinYtdCost : PX.Data.BQL.BqlDecimal.Field<endingCost_FinYtdCost> { }
        #endregion

        #region PeriodQtyWithin30D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty 1 Month")]
        public virtual Decimal? PeriodQtyWithin30D { get; set; }
        public abstract class periodQtyWithin30D : PX.Data.BQL.BqlDecimal.Field<periodQtyWithin30D> { }
        #endregion

        #region PeriodQtyFrom30Dto60D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty 2 Month")]
        public virtual Decimal? PeriodQtyFrom30Dto60D { get; set; }
        public abstract class periodQtyFrom30Dto60D : PX.Data.BQL.BqlDecimal.Field<periodQtyFrom30Dto60D> { }
        #endregion

        #region PeriodQtyFrom60Dto90D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty 3 Month")]
        public virtual Decimal? PeriodQtyFrom60Dto90D { get; set; }
        public abstract class periodQtyFrom60Dto90D : PX.Data.BQL.BqlDecimal.Field<periodQtyFrom60Dto90D> { }
        #endregion

        #region PeriodQtyFrom90Dto120D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty 4 Month")]
        public virtual Decimal? PeriodQtyFrom90Dto120D { get; set; }
        public abstract class periodQtyFrom90Dto120D : PX.Data.BQL.BqlDecimal.Field<periodQtyFrom90Dto120D> { }
        #endregion

        #region PeriodQtyFrom120Dto150D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty 5 Month")]
        public virtual Decimal? PeriodQtyFrom120Dto150D { get; set; }
        public abstract class periodQtyFrom120Dto150D : PX.Data.BQL.BqlDecimal.Field<periodQtyFrom120Dto150D> { }
        #endregion

        #region PeriodQtyFrom150Dto180D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty 6 Month")]
        public virtual Decimal? PeriodQtyFrom150Dto180D { get; set; }
        public abstract class periodQtyFrom150Dto180D : PX.Data.BQL.BqlDecimal.Field<periodQtyFrom150Dto180D> { }
        #endregion

        #region PeriodQtyFrom180Dto210D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty 7 Month")]
        public virtual Decimal? PeriodQtyFrom180Dto210D { get; set; }
        public abstract class periodQtyFrom180Dto210D : PX.Data.BQL.BqlDecimal.Field<periodQtyFrom180Dto210D> { }
        #endregion

        #region PeriodQtyFrom210Dto240D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty 8 Month")]
        public virtual Decimal? PeriodQtyFrom210Dto240D { get; set; }
        public abstract class periodQtyFrom210Dto240D : PX.Data.BQL.BqlDecimal.Field<periodQtyFrom210Dto240D> { }
        #endregion

        #region PeriodQtyFrom240Dto270D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty 9 Month")]
        public virtual Decimal? PeriodQtyFrom240Dto270D { get; set; }
        public abstract class periodQtyFrom240Dto270D : PX.Data.BQL.BqlDecimal.Field<periodQtyFrom240Dto270D> { }
        #endregion

        #region PeriodQtyFrom270Dto300D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty 10 Month")]
        public virtual Decimal? PeriodQtyFrom270Dto300D { get; set; }
        public abstract class periodQtyFrom270Dto300D : PX.Data.BQL.BqlDecimal.Field<periodQtyFrom270Dto300D> { }
        #endregion

        #region PeriodQtyFrom300Dto330D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty 11 Month")]
        public virtual Decimal? PeriodQtyFrom300Dto330D { get; set; }
        public abstract class periodQtyFrom300Dto330D : PX.Data.BQL.BqlDecimal.Field<periodQtyFrom300Dto330D> { }
        #endregion

        #region PeriodQtyFrom330Dto360D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty 12 Month")]
        public virtual Decimal? PeriodQtyFrom330Dto360D { get; set; }
        public abstract class periodQtyFrom330Dto360D : PX.Data.BQL.BqlDecimal.Field<periodQtyFrom330Dto360D> { }
        #endregion

        #region PeriodQtyFrom360Dto390D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty 13 Month")]
        public virtual Decimal? PeriodQtyFrom360Dto390D { get; set; }
        public abstract class periodQtyFrom360Dto390D : PX.Data.BQL.BqlDecimal.Field<periodQtyFrom360Dto390D> { }
        #endregion

        #region PeriodQtyFrom4Mto6M
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty 4M - 6M")]
        public virtual Decimal? PeriodQtyFrom4Mto6M { get; set; }
        public abstract class periodQtyFrom4Mto6M : PX.Data.BQL.BqlDecimal.Field<periodQtyFrom4Mto6M> { }
        #endregion

        #region PeriodQtyFrom7Mto12M
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty 7M - 12M")]
        public virtual Decimal? PeriodQtyFrom7Mto12M { get; set; }
        public abstract class periodQtyFrom7Mto12M : PX.Data.BQL.BqlDecimal.Field<periodQtyFrom7Mto12M> { }
        #endregion

        #region PeriodQtyFrom13Mto24M
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty 13-24 Month")]
        public virtual Decimal? PeriodQtyFrom13Mto24M { get; set; }
        public abstract class periodQtyFrom13Mto24M : PX.Data.BQL.BqlDecimal.Field<periodQtyFrom13Mto24M> { }
        #endregion

        #region PeriodQtyFrom25Mto36M
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty 25-36 Month")]
        public virtual Decimal? PeriodQtyFrom25Mto36M { get; set; }
        public abstract class periodQtyFrom25Mto36M : PX.Data.BQL.BqlDecimal.Field<periodQtyFrom25Mto36M> { }
        #endregion

        #region PeriodQtyFrom37Mto48M
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty 37-48 Month")]
        public virtual Decimal? PeriodQtyFrom37Mto48M { get; set; }
        public abstract class periodQtyFrom37Mto48M : PX.Data.BQL.BqlDecimal.Field<periodQtyFrom37Mto48M> { }
        #endregion

        #region PeriodQtyOver1Y
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty > 1 Year")]
        public virtual Decimal? PeriodQtyOver1Y { get; set; }
        public abstract class periodQtyOver1Y : PX.Data.BQL.BqlDecimal.Field<periodQtyOver1Y> { }
        #endregion

        #region PeriodQtyOver4Y
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty Over 49 Month")]
        public virtual Decimal? PeriodQtyOver4Y { get; set; }
        public abstract class periodQtyOver4Y : PX.Data.BQL.BqlDecimal.Field<periodQtyOver4Y> { }
        #endregion

        #region PeriodCostWithin30D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost 1 Month")]
        public virtual Decimal? PeriodCostWithin30D { get; set; }
        public abstract class periodCostWithin30D : PX.Data.BQL.BqlDecimal.Field<periodCostWithin30D> { }
        #endregion

        #region PeriodCostFrom30Dto60D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost 2 Month")]
        public virtual Decimal? PeriodCostFrom30Dto60D { get; set; }
        public abstract class periodCostFrom30Dto60D : PX.Data.BQL.BqlDecimal.Field<periodCostFrom30Dto60D> { }
        #endregion

        #region PeriodCostFrom60Dto90D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost 3 Month")]
        public virtual Decimal? PeriodCostFrom60Dto90D { get; set; }
        public abstract class periodCostFrom60Dto90D : PX.Data.BQL.BqlDecimal.Field<periodCostFrom60Dto90D> { }
        #endregion

        #region PeriodCostFrom90Dto120D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost 4 Month")]
        public virtual Decimal? PeriodCostFrom90Dto120D { get; set; }
        public abstract class periodCostFrom90Dto120D : PX.Data.BQL.BqlDecimal.Field<periodCostFrom90Dto120D> { }
        #endregion

        #region PeriodCostFrom120Dto150D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost 5 Month")]
        public virtual Decimal? PeriodCostFrom120Dto150D { get; set; }
        public abstract class periodCostFrom120Dto150D : PX.Data.BQL.BqlDecimal.Field<periodCostFrom120Dto150D> { }
        #endregion

        #region PeriodCostFrom150Dto180D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost 6 Month")]
        public virtual Decimal? PeriodCostFrom150Dto180D { get; set; }
        public abstract class periodCostFrom150Dto180D : PX.Data.BQL.BqlDecimal.Field<periodCostFrom150Dto180D> { }
        #endregion

        #region PeriodCostFrom180Dto210D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost 7 Month")]
        public virtual Decimal? PeriodCostFrom180Dto210D { get; set; }
        public abstract class periodCostFrom180Dto210D : PX.Data.BQL.BqlDecimal.Field<periodCostFrom180Dto210D> { }
        #endregion

        #region PeriodCostFrom210Dto240D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost 8 Month")]
        public virtual Decimal? PeriodCostFrom210Dto240D { get; set; }
        public abstract class periodCostFrom210Dto240D : PX.Data.BQL.BqlDecimal.Field<periodCostFrom210Dto240D> { }
        #endregion

        #region PeriodCostFrom240Dto270D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost 9 Month")]
        public virtual Decimal? PeriodCostFrom240Dto270D { get; set; }
        public abstract class periodCostFrom240Dto270D : PX.Data.BQL.BqlDecimal.Field<periodCostFrom240Dto270D> { }
        #endregion

        #region PeriodCostFrom270Dto300D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost 10 Month")]
        public virtual Decimal? PeriodCostFrom270Dto300D { get; set; }
        public abstract class periodCostFrom270Dto300D : PX.Data.BQL.BqlDecimal.Field<periodCostFrom270Dto300D> { }
        #endregion

        #region PeriodCostFrom300Dto330D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost 11 Month")]
        public virtual Decimal? PeriodCostFrom300Dto330D { get; set; }
        public abstract class periodCostFrom300Dto330D : PX.Data.BQL.BqlDecimal.Field<periodCostFrom300Dto330D> { }
        #endregion

        #region PeriodCostFrom330Dto360D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost 12 Month")]
        public virtual Decimal? PeriodCostFrom330Dto360D { get; set; }
        public abstract class periodCostFrom330Dto360D : PX.Data.BQL.BqlDecimal.Field<periodCostFrom330Dto360D> { }
        #endregion

        #region PeriodCostFrom360Dto390D
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost 13 Month")]
        public virtual Decimal? PeriodCostFrom360Dto390D { get; set; }
        public abstract class periodCostFrom360Dto390D : PX.Data.BQL.BqlDecimal.Field<periodCostFrom360Dto390D> { }
        #endregion

        #region PeriodCostFrom4Mto6M
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost 4M - 6M")]
        public virtual Decimal? PeriodCostFrom4Mto6M { get; set; }
        public abstract class periodCostFrom4Mto6M : PX.Data.BQL.BqlDecimal.Field<periodCostFrom4Mto6M> { }
        #endregion

        #region PeriodCostFrom7Mto12M
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost 7M - 12M")]
        public virtual Decimal? PeriodCostFrom7Mto12M { get; set; }
        public abstract class periodCostFrom7Mto12M : PX.Data.BQL.BqlDecimal.Field<periodCostFrom7Mto12M> { }
        #endregion

        #region PeriodCostFrom13Mto24M
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost 13-24 Month")]
        public virtual Decimal? PeriodCostFrom13Mto24M { get; set; }
        public abstract class periodCostFrom13Mto24M : PX.Data.BQL.BqlDecimal.Field<periodCostFrom13Mto24M> { }
        #endregion

        #region PeriodCostFrom25Mto36M
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost 25-36 Month")]
        public virtual Decimal? PeriodCostFrom25Mto36M { get; set; }
        public abstract class periodCostFrom25Mto36M : PX.Data.BQL.BqlDecimal.Field<periodCostFrom25Mto36M> { }
        #endregion

        #region PeriodCostFrom37Mto48M
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost 37-48 Month")]
        public virtual Decimal? PeriodCostFrom37Mto48M { get; set; }
        public abstract class periodCostFrom37Mto48M : PX.Data.BQL.BqlDecimal.Field<periodCostFrom37Mto48M> { }
        #endregion

        #region PeriodCostOver1Y
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost > 1 Year")]
        public virtual Decimal? PeriodCostOver1Y { get; set; }
        public abstract class periodCostOver1Y : PX.Data.BQL.BqlDecimal.Field<periodCostOver1Y> { }
        #endregion

        #region PeriodCostOver4Y
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Cost Over 49 Month")]
        public virtual Decimal? PeriodCostOver4Y { get; set; }
        public abstract class periodCostOver4Y : PX.Data.BQL.BqlDecimal.Field<periodCostOver4Y> { }
        #endregion

        #region LastActivityPeriod
        [PXDBString(6, IsFixed = true, InputMask = "")]
        [PXUIField(DisplayName = "Last Activity Period")]
        public virtual string LastActivityPeriod { get; set; }
        public abstract class lastActivityPeriod : PX.Data.BQL.BqlString.Field<lastActivityPeriod> { }
        #endregion

        #region ConditionPeriod
        [PXDBString(6, IsFixed = true, InputMask = "")]
        [PXUIField(DisplayName = "Condition Period")]
        public virtual string ConditionPeriod { get; set; }
        public abstract class conditionPeriod : PX.Data.BQL.BqlString.Field<conditionPeriod> { }
        #endregion

        //#region LastSalesDate
        //[PXDBDate()]
        //[PXUIField(DisplayName = "Last Sales Date")]
        //public virtual DateTime? LastSalesDate { get; set; }
        //public abstract class lastSalesDate : PX.Data.BQL.BqlDateTime.Field<lastSalesDate> { }
        //#endregion

        //#region LastSalesDoc
        //[PXDBString(256, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Last Sales Document")]
        //public virtual string LastSalesDoc { get; set; }
        //public abstract class lastSalesDoc : PX.Data.BQL.BqlString.Field<lastSalesDoc> { }
        //#endregion

        //#region LastReceiptDate
        //[PXDBDate()]
        //[PXUIField(DisplayName = "Last Receipt Date")]
        //public virtual DateTime? LastReceiptDate { get; set; }
        //public abstract class lastReceiptDate : PX.Data.BQL.BqlDateTime.Field<lastReceiptDate> { }
        //#endregion

        //#region LastReceiptDoc
        //[PXDBString(256, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Last Receipt Document")]
        //public virtual string LastReceiptDoc { get; set; }
        //public abstract class lastReceiptDoc : PX.Data.BQL.BqlString.Field<lastReceiptDoc> { }
        //#endregion
    }
}
