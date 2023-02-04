using PX.Data;
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
                if (ex.Graph is SOInvoiceEntry)
                {
                    var invoiceGraph = ex.Graph as SOInvoiceEntry;
                    var srvType = Base.AppointmentRecords.Current?.SrvOrdType;
                    var srvOrdTypePreference = FSSrvOrdType.PK.Find(invoiceGraph, srvType);
                    if (!(srvOrdTypePreference.GetExtension<FSSrvOrdTypeExt_Finance>()?.UsrEnableSelectRevenueAndCostAcct ?? false))
                        throw ex;
                    foreach (ARTran item in invoiceGraph.Transactions.Select())
                    {
                        var mapRevenueData = SelectFrom<LUMRevenueInventoryAccounts>
                                            .Where<LUMRevenueInventoryAccounts.srvOrderType.IsEqual<P.AsString>>
                                            .View.Select(invoiceGraph, srvType).TopFirst;
                        if (mapRevenueData != null)
                        {
                            item.AccountID = mapRevenueData.AccountID;
                            item.SubID = mapRevenueData.SubAccountID;
                            item.ReasonCode = mapRevenueData.RevenueReasonCode;
                            // 使用標準欄位
                            //item.GetExtension<ARTranExt_Finance>().UsrReasonCode = mapRevenueData.RevenueReasonCode;
                            invoiceGraph.Transactions.Cache.Update(item);
                        }
                    }
                }
                throw ex;
            }
        }
    }
}
