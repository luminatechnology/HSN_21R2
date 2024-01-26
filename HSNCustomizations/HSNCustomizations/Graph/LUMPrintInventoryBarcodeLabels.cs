using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;

namespace HSNCustomizations.Graph
{
    public class LUMPrintInventoryBarcodeLabels : PXGraph<LUMPrintInventoryBarcodeLabels>
    {
        public const decimal MaxNbrPerSheet = 48;

        #region Features & Selects
        public PXCancel<LUMBarcodeSeqFilter> Cancel;
        public PXFilter<LUMBarcodeSeqFilter> Filter;
        [PXImport(typeof(LUMInventBarcodeResult))]
        public PXSelectOrderBy<LUMInventBarcodeResult, OrderBy<Asc<LUMInventBarcodeResult.sortOrder>>> Result;
        #endregion

        #region Delegate Date View
        protected virtual IEnumerable result()
        {
            return Result.Cache.Inserted;
        }
        #endregion

        #region Actions
        public PXAction<LUMBarcodeSeqFilter> printLabels;
        [PXButton(), PXUIField(DisplayName = "Print", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual void PrintLabels()
        {
            VerifyTotalQty();

            throw RunReport(Filter.Current?.StartSeq);
        }
        #endregion

        #region Event Handlers
        protected virtual void _(Events.RowSelected<LUMBarcodeSeqFilter> e)
        {
            printLabels.SetEnabled(Result.Current != null);
        }

        protected virtual void _(Events.RowInserting<LUMInventBarcodeResult> e)
        {
            var row = e.Row;

            if (row != null)
            {
                row.SortOrder = (int)Result.Cache.Inserted.Count();
                //row.SortOrder = Result.Cache.Inserted.OfType<LUMInventBarcodeResult>().Where(w => w.InventoryID != row.InventoryID).FirstOrDefault()?.SortOrder ?? (int)Result.Cache.Inserted.Count();
            }
        }
        #endregion

        #region Methods
        public virtual void VerifyTotalQty()
        {
            if (Result.Cache.Inserted.OfType<LUMInventBarcodeResult>().Sum(s => s.PrintQty) > MaxNbrPerSheet)
            {
                const string ExceedMaxNQty = "Total Print Qty Can't Exceed The Maximum Qty Per Sheet(48).";

                throw new PXException(ExceedMaxNQty);
            }
        }

        public virtual PXReportRequiredException RunReport(int? startSeq)
        {
            int? inventoryID;
            int  qtyPerColumn = (int)(MaxNbrPerSheet / 3), startPosition = startSeq.Value;

            Dictionary<int, int?> dic = new Dictionary<int, int?>();

            foreach (LUMInventBarcodeResult row in Result.Select())
            {
                for (int i = startPosition; i < startPosition + (int)row.PrintQty; i++)
                {
                    dic.Add(i, row.InventoryID);
                }

                startPosition += (int)row.PrintQty;
            }

            PXReportResultset reportData = new PXReportResultset(typeof(LUMBarcodeReportData));

            // 3 means three columns in each sheet of paper.
            for (int i = 1; i <= qtyPerColumn; i++)
            {
                LUMBarcodeReportData data = new LUMBarcodeReportData()
                {
                    LineNbr = i
                };

                dic.TryGetValue(i, out inventoryID);
                data.InventoryID1 = inventoryID;
                dic.TryGetValue(i + qtyPerColumn, out inventoryID);
                data.InventoryID2 = inventoryID;
                dic.TryGetValue(i + (qtyPerColumn * 2), out inventoryID);
                data.InventoryID3 = inventoryID;

                reportData.Add(data);
            }

            return new PXReportRequiredException(reportData, "LM641001", PXBaseRedirectException.WindowMode.Same);
        }
        #endregion
    }

    #region Unbound DACs
    [PXHidden()]
    [PXCacheName("Barcode Sequence Filter")]
    public class LUMBarcodeSeqFilter : IBqlTable
    {
        #region StartSeq
        [PXInt(MinValue = 1, MaxValue = 48)]
        [PXUIField(DisplayName = "Start From Sequence")] 
        [PXDefault(1)]
        public virtual int? StartSeq { get; set; }
        public abstract class startSeq : PX.Data.BQL.BqlInt.Field<startSeq> { }
        #endregion
    }

    [PXHidden()]
    [PXCacheName("Inventory Barcode Printing Result")]
    public class LUMInventBarcodeResult : IBqlTable
    {
        #region InventoryID
        [AnyInventory(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.stkItem, NotEqual<boolFalse>,
                                                                     And<Where<Match<Current<AccessInfo.userName>>>>>>),
                      typeof(InventoryItem.inventoryCD),
                      typeof(InventoryItem.descr), 
                      IsKey = true)]
        [PXDefault()]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion

        #region SortOrder
        [PXInt(IsKey = true)]
        [PXUIField(DisplayName = "Sorting", Visible = false)]
        public virtual int? SortOrder { get; set; }
        public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
        #endregion

        #region PrintQty
        [PXQuantity(0)]
        [PXUIField(DisplayName = "Print Quantity")]
        public virtual decimal? PrintQty { get; set; }
        public abstract class printQty : PX.Data.BQL.BqlDecimal.Field<printQty> { }
        #endregion
    }

    [PXCacheName("Inventory Barcode Report Date")]
    public class LUMBarcodeReportData : IBqlTable
    {
        #region LineNbr
        [PXInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Number")]
        public virtual int? LineNbr { get; set; }
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        #endregion

        #region InventoryID1
        [AnyInventory(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.stkItem, NotEqual<boolFalse>,
                                                                     And<Where<Match<Current<AccessInfo.userName>>>>>>),
                      typeof(InventoryItem.inventoryCD),
                      typeof(InventoryItem.descr))]
        public virtual int? InventoryID1 { get; set; }
        public abstract class inventoryID1 : PX.Data.BQL.BqlInt.Field<inventoryID1> { }
        #endregion

        #region InventoryID2
        [AnyInventory(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.stkItem, NotEqual<boolFalse>,
                                                                     And<Where<Match<Current<AccessInfo.userName>>>>>>),
                      typeof(InventoryItem.inventoryCD),
                      typeof(InventoryItem.descr))]
        public virtual int? InventoryID2 { get; set; }
        public abstract class inventoryID2 : PX.Data.BQL.BqlInt.Field<inventoryID2> { }
        #endregion

        #region InventoryID3
        [AnyInventory(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.stkItem, NotEqual<boolFalse>,
                                                                     And<Where<Match<Current<AccessInfo.userName>>>>>>),
                      typeof(InventoryItem.inventoryCD),
                      typeof(InventoryItem.descr))]
        public virtual int? InventoryID3 { get; set; }
        public abstract class inventoryID3 : PX.Data.BQL.BqlInt.Field<inventoryID3> { }
        #endregion
    }
    #endregion
}