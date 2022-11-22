﻿using PX.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.AR;
using PX.Objects.SO;
using PX.Data.BQL.Fluent;
using HSNFinance;
using PX.Data.BQL;
using PX.Objects.IN;

namespace PX.Objects.FS
{
    public class AppointmentEntry_FinanceExt : PXGraphExtension<AppointmentEntry>
    {
        public delegate void OpenPostingDocumentDelegate2();
        [PXOverride]
        public virtual void openPostingDocument(OpenPostingDocumentDelegate2 baseMethod)
        {
            try
            {
                baseMethod();
            }
            catch (PXRedirectRequiredException ex)
            {
                var invoiceGraph = ex.Graph as SOInvoiceEntry;
                var srvType = Base.AppointmentRecords.Current?.SrvOrdType;
                var srvOrdTypePreference = FSSrvOrdType.PK.Find(invoiceGraph, srvType);
                if (!(srvOrdTypePreference.GetExtension<FSSrvOrdTypeExt_Finance>()?.UsrEnableSelectRevenueAndCostAcct ?? false))
                    throw ex;
                foreach (ARTran item in invoiceGraph.Transactions.Select())
                {
                    var inventoryInfo = InventoryItem.PK.Find(invoiceGraph, item.InventoryID);
                    var mapRevenueData = SelectFrom<LUMRevenueInventoryAccounts>
                                        .Where<LUMRevenueInventoryAccounts.srvOrderType.IsEqual<P.AsString>
                                          .And<LUMRevenueInventoryAccounts.itemClassID.IsEqual<P.AsInt>>>
                                        .View.Select(invoiceGraph, srvType, inventoryInfo?.ItemClassID).TopFirst;
                    if (mapRevenueData != null)
                    {
                        item.AccountID = mapRevenueData.AccountID;
                        item.SubID = mapRevenueData.SubAccountID;
                        item.GetExtension<ARTranExt_Finance>().UsrReasonCode = mapRevenueData.RevenueReasonCode;
                        invoiceGraph.Transactions.Cache.Update(item);
                    }
                }
                throw ex;
            }
        }
    }
}
