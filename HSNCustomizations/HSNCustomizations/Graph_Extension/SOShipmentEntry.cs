using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.FS;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNCustomizations.Graph_Extension
{
    public class SOShipmentEntryExt : PXGraphExtension<SOShipmentEntry>
    {
        #region Delegate Method

        public delegate void CreateShipmentDelegate(CreateShipmentArgs args);
        [PXOverride]
        public virtual void CreateShipment(CreateShipmentArgs args, CreateShipmentDelegate baseMethod)
        {
            baseMethod(args);
            // [Customize] OrderTypeForZeroBilling - 自動帶入對應的AppointmentDet Lotserail number
            foreach (var shipLine in Base.Transactions.Cache.Cached.RowCast<SOShipLine>())
            {
                var currentSOLine = SelectFrom<SOLine>
                                   .Where<SOLine.orderType.IsEqual<P.AsString>
                                     .And<SOLine.orderNbr.IsEqual<P.AsString>>
                                     .And<SOLine.lineNbr.IsEqual<P.AsInt>>>
                                   .View.Select(Base, shipLine?.OrigOrderType, shipLine?.OrigOrderNbr, shipLine?.OrigLineNbr).TopFirst;

                var currentFSxSOLineRow = currentSOLine.GetExtension<FSxSOLine>();
                if (currentFSxSOLineRow != null)
                {

                    var apptLinesplit = SelectFrom<FSSODet>
                                       .InnerJoin<FSAppointmentDet>.On<FSSODet.sODetID.IsEqual<FSAppointmentDet.sODetID>>
                                       .InnerJoin<FSApptLineSplit>.On<FSAppointmentDet.refNbr.IsEqual<FSApptLineSplit.apptNbr>
                                                                 .And<FSAppointmentDet.lineNbr.IsEqual<FSApptLineSplit.lineNbr>>>
                                       .Where<FSSODet.refNbr.IsEqual<P.AsString>
                                          .And<FSSODet.lineNbr.IsEqual<P.AsInt>>>
                                      .View.Select(Base, currentFSxSOLineRow?.ServiceOrderRefNbr, currentFSxSOLineRow.ServiceOrderLineNbr).RowCast<FSApptLineSplit>().FirstOrDefault();
                    var srvType = SelectFrom<FSSrvOrdType>.Where<FSSrvOrdType.srvOrdType.IsEqual<P.AsString>>.View.Select(Base, apptLinesplit?.SrvOrdType).TopFirst;
                    if (!string.IsNullOrEmpty(srvType.GetExtension<FSSrvOrdTypeExt>()?.UsrOrderTypeForZeroBilling))
                    {
                        var split = Base.splits.Cache.Cached.RowCast<SOShipLineSplit>()
                                   .Where(x => x.InventoryID == shipLine.InventoryID && x.LineNbr == shipLine.LineNbr).FirstOrDefault();
                        if (split != null)
                        { 
                            Base.splits.SetValueExt<SOShipLineSplit.lotSerialNbr>(split, apptLinesplit?.LotSerialNbr);
                            Base.splits.Cache.Update(split);
                        }
                        else
                            InsertSplitLine(shipLine, apptLinesplit?.LocationID, apptLinesplit?.LotSerialNbr);
                    }
                }
            }
            Base.Save.Press();
        }
        #endregion

        public virtual void InsertSplitLine(SOShipLine newline, int? _locationID, string _lotserialNbr)
        {
            IBqlTable newsplit = (SOShipLineSplit)newline;
            PXCache cache = Base.splits.Cache;

            cache.SetValue<SOShipLineSplit.uOM>(newsplit, null);
            cache.SetValue<SOShipLineSplit.splitLineNbr>(newsplit, null);
            cache.SetValue<SOShipLineSplit.locationID>(newsplit, _locationID);
            cache.SetValue<SOShipLineSplit.isUnassigned>(newsplit, newline.IsUnassigned);
            cache.SetValue<SOShipLineSplit.lotSerialNbr>(newsplit, _lotserialNbr);
            cache.SetValue<SOShipLineSplit.qty>(newsplit, newline.ShippedQty);
            cache.SetValue<SOShipLineSplit.baseQty>(newsplit, newline.ShippedQty);
            cache.Insert(newsplit);
        }
    }
}
