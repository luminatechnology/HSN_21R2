using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFCustomizations.DAC;
using PX.Objects.CR.Standalone;
using PX.Data.BQL;
using VFCustomizations.DAC_Extension;

namespace VFCustomizations.Graph_Extension
{
    public class SOShipmentEntryExt : PXGraphExtension<SOShipmentEntry>
    {
        public SelectFrom<LUMVFApisetupResult>
               .Where<LUMVFApisetupResult.shipmentNbr.IsEqual<SOShipment.shipmentNbr.FromCurrent>
                   .And<LUMVFApisetupResult.shipmentType.IsEqual<SOShipment.shipmentType.FromCurrent>>>
               .View ApiSetupResult;

        public SelectFrom<LUMVFAPISetupHoldResult>
               .Where<LUMVFAPISetupHoldResult.shipmentNbr.IsEqual<SOShipment.shipmentNbr.FromCurrent>
                   .And<LUMVFAPISetupHoldResult.shipmentType.IsEqual<SOShipment.shipmentType.FromCurrent>>>
               .View ApiHoldResult;

        public SelectFrom<SOOrder>
               .Where<SOOrder.orderNbr.IsEqual<SOOrder.orderNbr.AsOptional>
                 .And<SOOrder.orderType.IsEqual<SOOrder.orderType.AsOptional>>>
               .View soOrderData;

        #region Override Method

        public override void Initialize()
        {
            base.Initialize();
            var organizationID = PXAccess.GetParentOrganization(PXAccess.GetBranchID());
            var locationInfo = SelectFrom<Location>
                     .Where<Location.bAccountID.IsEqual<P.AsInt>>
                     .View.SelectSingleBound(Base, null, organizationID?.BAccountID).TopFirst;
            this.ApiSetupResult.AllowSelect = this.ApiHoldResult.AllowSelect = locationInfo?.CAvalaraExemptionNumber == "VF";
        }

        #endregion

        #region Delegate Methods

        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            var doc = Base.Document.Current;
            var orderShipmentInfo = GetSOOrderShipmentInfo(doc);
            if (doc != null && orderShipmentInfo != null)
            {
                try
                {
                    var orderInfo = SOOrder.PK.Find(Base, orderShipmentInfo?.OrderType, orderShipmentInfo?.OrderNbr);
                    // Sync WFSTAGE User-defined
                    var attributeName = PX.Objects.CS.Messages.Attribute + "WFSTAGE";
                    var attrWFSTAGE = (string)(Base.Document.Cache.GetValueExt(Base.Document.Current, attributeName) as PXFieldState)?.Value;
                    if (!string.IsNullOrEmpty(attrWFSTAGE))
                        InsertOrUpdateSOOrderUserDefined(orderInfo.NoteID, attributeName, attrWFSTAGE);
                    // Sync STAFF User-defined
                    attributeName = PX.Objects.CS.Messages.Attribute + "STAFF";
                    var attrSTAFF = (string)(Base.Document.Cache.GetValueExt(Base.Document.Current, attributeName) as PXFieldState)?.Value;
                    if (!string.IsNullOrEmpty(attrSTAFF))
                        InsertOrUpdateSOOrderUserDefined(orderInfo.NoteID, attributeName, attrSTAFF);
                }
                catch (Exception ex)
                {
                    PXTrace.WriteError(ex.Message);
                }
            }
            baseMethod();
        }

        #endregion

        #region Events

        public virtual void _(Events.RowSelected<SOShipment> e, PXRowSelected baseMethod)
        {
            var isVisiable = SelectFrom<LUMVerifonePreference>.View.Select(Base).TopFirst?.EnableVFCustomizeField ?? false;
            PXUIFieldAttribute.SetVisible<SOShipmentExt.usrDeliverDate>(Base.Document.Cache, null, isVisiable);
            if (e.Row.GetExtension<SOShipmentExt>()?.UsrDeliverDate == null && isVisiable)
                Base.Document.SetValueExt<SOShipmentExt.usrDeliverDate>(e.Row, e.Row.ShipDate);
            baseMethod?.Invoke(e.Cache, e.Args);
        }

        public virtual void _(Events.RowSelected<SOShipLine> e, PXRowSelected baseMethod)
        {
            // 判斷是否有啟用VF Preference
            var isVisiable = SelectFrom<LUMVerifonePreference>.View.Select(Base).TopFirst?.EnableVFCustomizeField ?? false;
            PXUIFieldAttribute.SetVisible<SOShipLineExtension.usrNodeName>(Base.Transactions.Cache, null, isVisiable);
            baseMethod?.Invoke(e.Cache, e.Args);
            if (isVisiable)
            {
                // 執行usrNodeName Defaulting 事件
                object newNodeName;
                e.Cache.RaiseFieldDefaulting<SOShipLineExtension.usrNodeName>(e.Row, out newNodeName);
                if (newNodeName != null)
                    e.Row.GetExtension<SOShipLineExtension>().UsrNodeName = (string)newNodeName;
            }
        }

        public virtual void _(Events.FieldUpdated<SOShipLine.origOrderNbr> e, PXFieldUpdated baseMethod)
        {
            baseMethod?.Invoke(e.Cache, e.Args);
            // VF Customization - Show SalesOrder UDF - NodeName
            object newNodeName;
            e.Cache.RaiseFieldDefaulting<SOShipLineExtension.usrNodeName>(e.Row, out newNodeName);
            if (newNodeName != null)
                (e.Row as SOShipLine).GetExtension<SOShipLineExtension>().UsrNodeName = (string)newNodeName;
        }

        public virtual void _(Events.FieldDefaulting<SOShipLineExtension.usrNodeName> e)
        {
            var row = e.Row as SOShipLine;
            if (row != null && !string.IsNullOrEmpty(row.OrigOrderNbr))
            {
                // VF Customization - Show SalesOrder UDF - NodeName
                var so = SOOrder.PK.Find(Base, row?.OrigOrderType, row?.OrigOrderNbr);
                PXCache soCache = Base.Caches<SOOrder>();
                var udfNodeName = (string)(soCache.GetValueExt(so, PX.Objects.CS.Messages.Attribute + "NODENAME") as PXFieldState)?.Value;
                e.NewValue = udfNodeName;
            }
        }

        public virtual void _(Events.RowSelected<SOShipmentPlan> e, PXRowSelected baseMethod)
        {
            baseMethod?.Invoke(e.Cache,e.Args);
            // 判斷是否有啟用VF Preference
            var isVisiable = SelectFrom<LUMVerifonePreference>.View.Select(Base).TopFirst?.EnableVFCustomizeField ?? false;
            PXUIFieldAttribute.SetVisible<SOShipLineExtension.usrNodeName>(Base.soshipmentplan.Cache, null, isVisiable);
            // VF Customization - Show SalesOrder UDF - NodeName
            object newNodeName;
            e.Cache.RaiseFieldDefaulting<SOShipmentPlanExtension.usrNodeName>(e.Row, out newNodeName);
            if (newNodeName != null)
                (e.Row as SOShipmentPlan).GetExtension<SOShipmentPlanExtension>().UsrNodeName = (string)newNodeName;
        }

        public virtual void _(Events.FieldDefaulting<SOShipmentPlanExtension.usrNodeName> e)
        {
            // VF Customization - Show SalesOrder UDF - NodeName
            var row = e.Row as SOShipmentPlan;
            if (row != null && !string.IsNullOrEmpty(row.OrderNbr))
            {
                // 自動帶出對應SO的UDF - NodeName
                var so = SOOrder.PK.Find(Base, row?.OrderType, row?.OrderNbr);
                PXCache soCache = Base.Caches<SOOrder>();
                var udfNodeName = (string)(soCache.GetValueExt(so, PX.Objects.CS.Messages.Attribute + "NODENAME") as PXFieldState)?.Value;
                e.NewValue = udfNodeName;
            }
        }

        #region SETUP
        public virtual void _(Events.RowSelected<LUMVFApisetupResult> e)
        {
            if (e.Row != null)
            {
                if (string.IsNullOrEmpty(e.Row.JobNo))
                {
                    object newJobNo;
                    e.Cache.RaiseFieldDefaulting<LUMVFApisetupResult.jobNo>(e.Row, out newJobNo);
                    e.Row.JobNo = (string)newJobNo;
                }

                if (string.IsNullOrEmpty(e.Row.TerminalID))
                {
                    object newTerminalID;
                    e.Cache.RaiseFieldDefaulting<LUMVFApisetupResult.terminalID>(e.Row, out newTerminalID);
                    e.Row.TerminalID = (string)newTerminalID;
                }
            }
        }

        public virtual void _(Events.FieldDefaulting<LUMVFApisetupResult.lineNbr> e)
        {
            var currentList = this.ApiSetupResult.Select().RowCast<LUMVFApisetupResult>();
            var maxLineNbr = currentList.Count() == 0 ? 0 : currentList.Max(x => x?.LineNbr ?? 0);
            e.NewValue = maxLineNbr + 1;
        }

        public virtual void _(Events.FieldDefaulting<LUMVFApisetupResult.jobNo> e)
        {
            var current = Base.Document.Current;
            if (current != null)
            {
                var orderShipmentInfo = GetSOOrderShipmentInfo(current);
                if (orderShipmentInfo == null)
                    return;
                var orderInfo = SOOrder.PK.Find(Base, orderShipmentInfo?.OrderType, orderShipmentInfo?.OrderNbr);
                e.NewValue = orderInfo?.CustomerOrderNbr;
            }
        }

        public virtual void _(Events.FieldDefaulting<LUMVFApisetupResult.terminalID> e)
        {
            var current = Base.Document.Current;
            if (current != null)
            {
                var orderShipmentInfo = GetSOOrderShipmentInfo(current);
                if (orderShipmentInfo == null)
                    return;
                SOOrder soData = soOrderData.Select(orderShipmentInfo?.OrderNbr, orderShipmentInfo?.OrderType);
                var attr = soOrderData.Cache.GetValueExt(soData, PX.Objects.CS.Messages.Attribute + "TERMINALID") as PXFieldState;
                e.NewValue = attr?.Value;
            }
        }

        #endregion

        #region HOLD
        public virtual void _(Events.RowSelected<LUMVFAPISetupHoldResult> e)
        {
            if (e.Row != null)
            {
                if (string.IsNullOrEmpty(e.Row.JobNo))
                {
                    object newJobNo;
                    e.Cache.RaiseFieldDefaulting<LUMVFAPISetupHoldResult.jobNo>(e.Row, out newJobNo);
                    e.Row.JobNo = (string)newJobNo;
                }

                if (!e.Row.PreviousCommitDate.HasValue)
                {
                    object newPreviousCommitDate;
                    e.Cache.RaiseFieldDefaulting<LUMVFAPISetupHoldResult.previousCommitDate>(e.Row, out newPreviousCommitDate);
                    e.Row.PreviousCommitDate = (DateTime?)newPreviousCommitDate;
                }

                if (!e.Row.HoldDate.HasValue)
                {
                    object newHoldDate;
                    e.Cache.RaiseFieldDefaulting<LUMVFAPISetupHoldResult.holdDate>(e.Row, out newHoldDate);
                    e.Row.HoldDate = (DateTime?)newHoldDate;
                }

                if (string.IsNullOrEmpty(e.Row.IncidentCatalogName))
                {
                    object newIncidentCatalogName;
                    e.Cache.RaiseFieldDefaulting<LUMVFAPISetupHoldResult.incidentCatalogName>(e.Row, out newIncidentCatalogName);
                    e.Row.IncidentCatalogName = (string)newIncidentCatalogName;
                }
            }
        }

        public virtual void _(Events.FieldDefaulting<LUMVFAPISetupHoldResult.lineNbr> e)
        {
            var currentList = this.ApiHoldResult.Select().RowCast<LUMVFAPISetupHoldResult>();
            var maxLineNbr = currentList.Count() == 0 ? 0 : currentList.Max(x => x?.LineNbr ?? 0);
            e.NewValue = maxLineNbr + 1;
        }

        public virtual void _(Events.FieldDefaulting<LUMVFAPISetupHoldResult.jobNo> e)
        {
            var current = Base.Document.Current;
            if (current != null)
            {
                var orderShipmentInfo = GetSOOrderShipmentInfo(current);
                if (orderShipmentInfo == null)
                    return;
                var orderInfo = SOOrder.PK.Find(Base, orderShipmentInfo?.OrderType, orderShipmentInfo?.OrderNbr);
                e.NewValue = orderInfo?.CustomerOrderNbr;
            }
        }

        public virtual void _(Events.FieldDefaulting<LUMVFAPISetupHoldResult.previousCommitDate> e)
        {
            var current = Base.Document.Current;
            if (current != null)
            {
                var orderShipmentInfo = GetSOOrderShipmentInfo(current);
                if (orderShipmentInfo == null)
                    return;
                SOOrder soData = soOrderData.Select(orderShipmentInfo?.OrderNbr, orderShipmentInfo?.OrderType);
                var attr = soOrderData.Cache.GetValueExt(soData, PX.Objects.CS.Messages.Attribute + "COMMITDATE") as PXFieldState;
                DateTime newValue;
                if (DateTime.TryParse(attr?.Value?.ToString(), out newValue))
                    e.NewValue = newValue;
                else
                {
                    e.NewValue = null;
                    PXTrace.WriteError($"Can not transfer Attribute value(COMMITDATE) to datetime({attr?.Value})");
                }
            }
        }

        public virtual void _(Events.FieldDefaulting<LUMVFAPISetupHoldResult.holdDate> e)
        {
            e.NewValue = DateTime.Now;
        }

        public virtual void _(Events.FieldDefaulting<LUMVFAPISetupHoldResult.incidentCatalogName> e)
        {
            var current = Base.Document.Current;
            if (current != null)
            {
                var orderShipmentInfo = GetSOOrderShipmentInfo(current);
                if (orderShipmentInfo == null)
                    return;
                SOOrder soData = soOrderData.Select(orderShipmentInfo?.OrderNbr, orderShipmentInfo?.OrderType);
                var attr = soOrderData.Cache.GetValueExt(soData, PX.Objects.CS.Messages.Attribute + "SRVCATALOG") as PXFieldState;
                e.NewValue = attr?.Value;
            }
        }

        #endregion

        #endregion

        #region Method

        public SOOrderShipment GetSOOrderShipmentInfo(SOShipment current)
        {
            return SelectFrom<SOOrderShipment>
                    .Where<SOOrderShipment.shipmentNbr.IsEqual<P.AsString>
                      .And<SOOrderShipment.shipmentType.IsEqual<P.AsString>>>
                    .View.SelectSingleBound(Base, null, current?.ShipmentNbr, current?.ShipmentType)?.TopFirst;
        }

        /// <summary> Insert or Update SOOrder User-defined </summary>
        public void InsertOrUpdateSOOrderUserDefined(Guid? noteid, string fieldName, string attributeValue)
        {
            var kvextInfo = SelectFrom<SOOrderKvExt>
                           .Where<SOOrderKvExt.recordID.IsEqual<P.AsGuid>>
                           .View.Select(Base, noteid).RowCast<SOOrderKvExt>();

            if (kvextInfo.Any(x => x.FieldName == fieldName))
            {
                // Update
                PXUpdate<Set<SOOrderKvExt.valueString, Required<SOOrderKvExt.valueString>>,
                    SOOrderKvExt,
                    Where<SOOrderKvExt.recordID, Equal<Required<SOOrderKvExt.recordID>>,
                      And<SOOrderKvExt.fieldName, Equal<Required<SOOrderKvExt.fieldName>>>>>
                    .Update(new PXGraph(), attributeValue, noteid, fieldName);
            }
            else
            {
                // Insert
                List<PXDataFieldAssign> assigns = new List<PXDataFieldAssign>();
                assigns.Add(new PXDataFieldAssign<SOOrderKvExt.recordID>(noteid));
                assigns.Add(new PXDataFieldAssign<SOOrderKvExt.valueString>(attributeValue));
                assigns.Add(new PXDataFieldAssign<SOOrderKvExt.fieldName>(fieldName));
                PXDatabase.Insert<SOOrderKvExt>(assigns.ToArray());
            }
        }

        #endregion

    }
}
