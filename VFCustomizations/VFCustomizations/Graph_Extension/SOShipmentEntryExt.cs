﻿using PX.Data;
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

        #region Events

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

                if(!e.Row.HoldDate.HasValue)
                {
                    object newHoldDate;
                    e.Cache.RaiseFieldDefaulting<LUMVFAPISetupHoldResult.holdDate>(e.Row, out newHoldDate);
                    e.Row.HoldDate = (DateTime?)newHoldDate;
                }

                if(string.IsNullOrEmpty(e.Row.IncidentCatalogName))
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
                e.NewValue = attr?.Value;
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
                    .View.SelectSingleBound(Base, null, current.ShipmentNbr, current.ShipmentType)?.TopFirst;
        }

        #endregion

    }
}
