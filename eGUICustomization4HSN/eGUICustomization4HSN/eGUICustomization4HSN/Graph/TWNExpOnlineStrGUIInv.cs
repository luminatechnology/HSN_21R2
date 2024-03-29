﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.SO;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.Descriptor;
using PX.CS.Contracts.Interfaces;
using eGUICustomization4HSN.StringList;

namespace eGUICustomization4HSN.Graph
{
    public class TWNExpOnlineStrGUIInv : PXGraph<TWNExpOnlineStrGUIInv>
    {
        #region Features
        public PXCancel<TWNGUITrans> Cancel;
        public PXProcessing<TWNGUITrans,
                            Where<TWNGUITrans.eGUIExcluded, Equal<False>,
                                  And<TWNGUITrans.gUIFormatcode, Equal<TWNExpGUIInv2BankPro.VATOutCode35>,
                                       And2<Where<TWNGUITrans.eGUIExported, Equal<False>,
                                                  Or<TWNGUITrans.eGUIExported, IsNull>>,
                                           And<TWNGUITrans.isOnlineStore, Equal<True>>>>>> GUITranProc;
        #endregion

        #region Ctor
        public TWNExpOnlineStrGUIInv()
        {
            GUITranProc.SetProcessCaption(ActionsMessages.Upload);
            GUITranProc.SetProcessAllCaption(TWMessages.UploadAll);
            GUITranProc.SetProcessDelegate(Upload);
        }
        #endregion

        #region Static Method
        public static void Upload(List<TWNGUITrans> tWNGUITrans)
        {
            const string FixedMsg = "GetRefNbrFromLine";
           
            try
            {
                // Avoid to create empty content file in automation schedule.
                if (tWNGUITrans.Count == 0) { return; }

                TWNExpOnlineStrGUIInv graph   = CreateInstance<TWNExpOnlineStrGUIInv>();
                TWNExpGUIInv2BankPro invGraph = CreateInstance<TWNExpGUIInv2BankPro>();

                string lines = "", fileName = "", verticalBar = TWNExpGUIInv2BankPro.verticalBar;

                TWNGUIPreferences preferences = PXSelect<TWNGUIPreferences>.Select(graph);

                fileName = preferences.OurTaxNbr + "-O-" + DateTime.Today.ToString("yyyyMMdd") + "-" + DateTime.Now.ToString("hhmmss") + ".txt";

                foreach (TWNGUITrans gUITrans in tWNGUITrans)
                {
                    bool isCM = gUITrans.GUIFormatcode == TWGUIFormatCode.vATOutCode33;
                    bool isB2C = string.IsNullOrEmpty(gUITrans.TaxNbr);

                    ARRegister    register = SelectFrom<ARRegister>.Where<ARRegister.refNbr.IsEqual<@P.AsString>>.View.SelectSingleBound(graph, null, gUITrans.OrderNbr);
                    ARRegisterExt regisExt = register.GetExtension<ARRegisterExt>();

                    #region Header
                    // 主檔代號
                    lines += "M" + verticalBar;
                    // 訂單編號
                    lines += (isCM == false ? gUITrans.OrderNbr : register?.OrigRefNbr ?? graph.GetRefNbrByGUINbr(graph, gUITrans.GUINbr)) + verticalBar;
                    // 訂單狀態
                    lines += (gUITrans.GUIStatus == TWNGUIStatus.Voided ? 2 : isCM == false ? 0 : 3) + verticalBar;
                    // 訂單日期
                    lines += gUITrans.TransDate.Value.ToString("yyyy/MM/dd") + verticalBar;
                    // 預計出貨日
                    lines += gUITrans.GUIDate.Value.ToString("yyyy/MM/dd") + verticalBar;
                    // 稅率別 -> 1:應稅 2:零稅率 3:免稅 4:特殊稅率(需帶36 & 37欄位) 
                    lines += (gUITrans.GUIFormatcode.IsIn(TWGUIFormatCode.vATOutCode36, TWGUIFormatCode.vATOutCode37)) ? "4" : TWNExpGUIInv2BankPro.GetTaxType(gUITrans.VATType) + verticalBar;
                    // 訂單金額(未稅)
                    lines += gUITrans.NetAmount + verticalBar;
                    // 訂單稅額
                    lines += gUITrans.TaxAmount + verticalBar;
                    // 訂單金額(含稅)
                    lines += (gUITrans.NetAmount + gUITrans.TaxAmount) + verticalBar;
                    // 賣方統一編號
                    lines += gUITrans.OurTaxNbr + verticalBar;
                    // 賣方廠編
                    lines += verticalBar;
                    // 買方統一編號
                    lines += gUITrans.TaxNbr + verticalBar;
                    // 買受人公司名稱
                    lines += gUITrans.GUITitle + verticalBar;
                    // 會員編號
                    (string Phone, string Email, string Attention) = graph.GetBillingInfo(graph, register.DocType, gUITrans.OrderNbr, gUITrans.CustVend);
                    lines += graph.GetMemberNbr(graph, register?.CustomerID, new string[] { gUITrans.CustVend, Phone }) + verticalBar;
                    // 會員姓名
                    lines += ((isB2C == true ? Attention : gUITrans.GUITitle) ?? string.Empty) + verticalBar;
                    // 會員郵遞區號
                    (string AddressLine, string PostalCode) = graph.GetBillingAddress(graph, register.DocType, gUITrans.OrderNbr, gUITrans.CustVend);
                    lines += (PostalCode ?? string.Empty) + verticalBar;
                    // 會員地址
                    lines += (isB2C == true? AddressLine : string.Empty) + verticalBar;
                    // 會員電話
                    lines += verticalBar;
                    // 會員行動電話
                    lines += Phone + verticalBar;
                    // 會員電子郵件
                    lines += Email + verticalBar;
                    // 紅利點數折扣金額
                    lines += verticalBar;
                    // 索取紙本發票                       
                    lines += "N" + verticalBar;
                    // 發票捐贈註記
                    lines += gUITrans.NPONbr + verticalBar;
                    // 訂單註記
                    // 付款方式
                    // 相關號碼1(出貨單號)
                    lines += new string(char.Parse(verticalBar), 3);
                    // 相關號碼2
                    lines += (isCM == false ? gUITrans.BatchNbr : gUITrans.OrderNbr) + verticalBar;
                    // 相關號碼3
                    // 主檔備註
                    // 商品名稱
                    lines += new string(char.Parse(verticalBar), 3);
                    // 載具類別號碼
                    lines += gUITrans.CarrierType + verticalBar;
                    // 載具顯碼id1(明碼)
                    lines += gUITrans.CarrierID + verticalBar;
                    // 載具隱碼id2(內碼)
                    lines += gUITrans.CarrierID + verticalBar;
                    // 發票號碼
                    lines += gUITrans.GUINbr + verticalBar;
                    // 隨機碼
                    lines += gUITrans.OrderNbr.Substring(gUITrans.OrderNbr.Length - 4, 4) + verticalBar;
                    // 稅率代碼
                    // 稅率
                    lines += 0 + verticalBar + "\r\n";
                    #endregion

                    bool isInclusive = invGraph.AmountInclusiveTax(register.TaxCalcMode, gUITrans.TaxID);
                    bool sumryPrint  = Convert.ToBoolean(graph.GetInvoiceFieldValue((register as ARInvoice), "UsrSummaryPrint") ?? false);
                    int num = 1;
                    string refNbr = string.Empty;
                    decimal? totalUP = 0m, totalEP = 0m, totalTA = 0m;
                    foreach (ARTran tran in invGraph.RetrieveARTran(gUITrans.OrderNbr))
                    {
                        (decimal UnitPrice, decimal ExtPrice) = graph.CalcTaxAmt(isInclusive == true,
                                                                                 !isB2C,
                                                                                 tran.CuryDiscAmt > 0 ? (tran.CuryTranAmt / tran.Qty).Value : tran.CuryUnitPrice.Value,
                                                                                 tran.CuryTranAmt.Value);

                        if (sumryPrint == false)
                        {
                            if (isCM == true && lines.Contains(FixedMsg) == true && !string.IsNullOrEmpty(tran.OrigInvoiceNbr))
                            {
                                lines = lines.Replace(FixedMsg, tran.OrigInvoiceNbr);
                            }

                            // 明細代號
                            lines += "D" + verticalBar;
                            // 序號
                            lines += num++ + verticalBar;
                            // 訂單編號
                            lines += (isCM == false ? tran.RefNbr : tran.OrigInvoiceNbr) + verticalBar;
                            // 商品編號
                            // 商品條碼
                            lines += new string(char.Parse(verticalBar), 2);
                            // 商品名稱
                            lines += Regex.Replace((tran.TranDesc ?? "").Replace("'", @"\'").Trim(), @"[\r\n]+", "") + verticalBar;
                            // 商品規格
                            // 單位
                            lines += new string(char.Parse(verticalBar), 2);
                            // 單價
                            lines += Math.Round(UnitPrice, 0) + verticalBar;
                            // 數量
                            lines += (tran.Qty == 0m ? 1 : tran.Qty) + verticalBar;
                            // 未稅金額
                            lines += ExtPrice + verticalBar;
                            // 含稅金額
                            lines += (isInclusive == false? tran.CuryTranAmt * (decimal)1.05 : tran.CuryTranAmt) + verticalBar;
                            // 健康捐
                            lines += 0 + verticalBar;
                            // 稅率別
                            lines += TWNExpGUIInv2BankPro.GetTaxType(gUITrans.VATType) + verticalBar;
                            // 紅利點數折扣金額
                            // 明細備註
                            lines += new string(char.Parse(verticalBar), 1) + "\r\n";
                        }
                        else
                        {
                            refNbr   = isCM == false ? tran.RefNbr : tran.OrigInvoiceNbr;
                            totalUP += UnitPrice;
                            totalEP += ExtPrice;
                            totalTA += isInclusive == false ? tran.CuryTranAmt * (decimal)1.05 : tran.CuryTranAmt;
                        }
                    }

                    #region GUI Summary 
                    if (sumryPrint == true)
                    {
                        // 明細代號
                        lines += "D" + verticalBar;
                        // 序號
                        lines += num++ + verticalBar;
                        // 訂單編號
                        lines += refNbr + verticalBar;
                        // 商品編號
                        // 商品條碼
                        lines += new string(char.Parse(verticalBar), 2);
                        // 商品名稱
                        lines += (CSAttributeDetail.PK.Find(graph, "GUISUMMARY", (graph.GetInvoiceFieldValue((register as ARInvoice), "UsrGUISummary") ?? string.Empty).ToString())?.Description ?? string.Empty) + verticalBar;
                        // 商品規格
                        // 單位
                        lines += new string(char.Parse(verticalBar), 2);
                        // 單價
                        lines += totalUP + verticalBar;
                        // 數量
                        lines += 1 + verticalBar;
                        // 未稅金額
                        lines += totalEP + verticalBar;
                        // 含稅金額
                        lines += totalTA + verticalBar;
                        // 健康捐
                        lines += 0 + verticalBar;
                        // 稅率別
                        lines += TWNExpGUIInv2BankPro.GetTaxType(gUITrans.VATType) + verticalBar;
                        // 紅利點數折扣金額
                        // 明細備註
                        lines += new string(char.Parse(verticalBar), 1) + "\r\n";
                    }
                    #endregion

                    #region Voided GUI
                    // The following method is only for voided invoice when the invoice doesn't have any lines.
                    if (gUITrans.GUIStatus == TWNGUIStatus.Voided && register.DocType != ARDocType.Prepayment && num == 1)
                    {
                        TWNExpGUIInv2BankPro.CreateVoidedDetailLine(verticalBar, gUITrans.OrderNbr, ref lines);
                    }
                    #endregion
                }

                // Total Records
                lines += tWNGUITrans.Count;

                invGraph.UploadFile2FTP(fileName, lines, true);
                invGraph.UpdateGUITran(tWNGUITrans);
            }
            catch (Exception ex)
            {
                PXProcessing<TWNGUITrans>.SetError(ex);
                throw;
            }
        }
        #endregion

        #region Methods
        private (string addressLine, string postalCode) GetBillingAddress(PXGraph graph, string docType, string refNbr, string customer)
        {
            IAddressBase address = SelectFrom<ARAddress>.InnerJoin<ARInvoice>.On<ARInvoice.billAddressID.IsEqual<ARAddress.addressID>>
                                                        .Where<ARInvoice.refNbr.IsEqual<@P.AsString>>.View.SelectSingleBound(graph, null, refNbr).TopFirst;

            if (address == null)
            {
                address = SelectFrom<ARAddress>.InnerJoin<ARInvoice>.On<ARInvoice.billAddressID.IsEqual<ARAddress.addressID>>
                                                .InnerJoin<ARAdjust>.On<ARAdjust.adjdDocType.IsEqual<ARInvoice.docType>
                                                                        .And<ARAdjust.adjdRefNbr.IsEqual<ARInvoice.refNbr>>>
                                                .Where<ARAdjust.adjgDocType.IsEqual<@P.AsString>
                                                        .And<ARAdjust.adjgRefNbr.IsEqual<@P.AsString>>>.View
                                                .SelectSingleBound(graph, null, docType, refNbr).TopFirst;
            }

            if (address == null)
            {
                address = SelectFrom<SOAddress>.InnerJoin<SOOrder>.On<SOOrder.billAddressID.IsEqual<SOAddress.addressID>>
                                                .InnerJoin<SOAdjust>.On<SOAdjust.adjdOrderType.IsEqual<SOOrder.orderType>
                                                                        .And<SOAdjust.adjdOrderNbr.IsEqual<SOOrder.orderNbr>>>
                                                .Where<SOAdjust.adjgDocType.IsEqual<@P.AsString>
                                                        .And<SOAdjust.adjgRefNbr.IsEqual<@P.AsString>>>.View
                                                .SelectSingleBound(graph, null, docType, refNbr).TopFirst;
        }

            if (address == null)
            {
                address = SelectFrom<Address>.InnerJoin<Customer>.On<Customer.defBillAddressID.IsEqual<Address.addressID>>
                                                 .Where<Customer.acctCD.IsEqual<@P.AsString>>.View
                                                 .SelectSingleBound(graph, null, customer).TopFirst;
            }

            return (address.AddressLine1, address.PostalCode);
        }

        private (string phone, string email, string attention) GetBillingInfo(PXGraph graph, string docType, string refNbr, string customer)
        {
            IContact contact = SelectFrom<ARContact>.InnerJoin<ARInvoice>.On<ARInvoice.billContactID.IsEqual<ARContact.contactID>>
                                                    .Where<ARInvoice.refNbr.IsEqual<@P.AsString>>.View.SelectSingleBound(graph, null, refNbr).TopFirst;

            if (contact == null)
            {
                contact = SelectFrom<ARContact>.InnerJoin<ARInvoice>.On<ARInvoice.billContactID.IsEqual<ARContact.contactID>>
                                               .InnerJoin<ARAdjust>.On<ARAdjust.adjdDocType.IsEqual<ARInvoice.docType>
                                                                       .And<ARAdjust.adjdRefNbr.IsEqual<ARInvoice.refNbr>>>
                                               .Where<ARAdjust.adjgDocType.IsEqual<@P.AsString>
                                                      .And<ARAdjust.adjgRefNbr.IsEqual<@P.AsString>>>.View
                                               .SelectSingleBound(graph, null, docType, refNbr).TopFirst;
            }

            if (contact == null)
            {
                contact = SelectFrom<SOContact>.InnerJoin<SOOrder>.On<SOOrder.billContactID.IsEqual<SOContact.contactID>>
                                               .InnerJoin<SOAdjust>.On<SOAdjust.adjdOrderType.IsEqual<SOOrder.orderType>
                                                                       .And<SOAdjust.adjdOrderNbr.IsEqual<SOOrder.orderNbr>>>
                                               .Where<SOAdjust.adjgDocType.IsEqual<@P.AsString>
                                                       .And<SOAdjust.adjgRefNbr.IsEqual<@P.AsString>>>.View
                                               .SelectSingleBound(graph, null, docType, refNbr).TopFirst;
            }

            if (contact == null)
            {
                contact = SelectFrom<Contact>.InnerJoin<Customer>.On<Customer.defBillContactID.IsEqual<Contact.contactID>>
                                             .Where<Customer.acctCD.IsEqual<@P.AsString>>.View.SelectSingleBound(graph, null, customer).TopFirst;
            }

            return (contact.Phone1, contact.Email, contact.Attention);
        }

        /// <summary>
        /// According to David's email [[SCM][廷漢] ��單檔案上傳結果訊息通知] comment.
        /// </summary>
        private string GetMemberNbr(PXGraph graph, int? customerID, string[] strings)
        {
            bool iseGUICust = Convert.ToBoolean(Convert.ToInt32(CSAnswers.PK.Find(graph, Customer.PK.Find(graph, customerID)?.NoteID, "EGUICUSMEM")?.Value ?? "0") );

            // [0] -> TWNGUITrans.CustVend, [1] -> billing phone
            return iseGUICust == true ? strings[0] + strings[1] : strings[0];
        }

        private string GetRefNbrByGUINbr(PXGraph graph, string gUINbr)
        {
            return SelectFrom<TWNGUITrans>.Where<TWNGUITrans.gUINbr.IsEqual<@P.AsString>
                                                 .And<TWNGUITrans.gUIFormatcode.IsEqual<@P.AsString>>>.View
                                          .SelectSingleBound(graph, null, gUINbr, TWGUIFormatCode.vATOutCode35).TopFirst?.OrderNbr;
        }

        public virtual (decimal UnitPrice, decimal ExtPrice) CalcTaxAmt(bool isGross, bool hasTaxNbr, decimal unitPrice, decimal extPrice)
        {
            // B2C
            if (hasTaxNbr == false)
            {
                if (isGross == false)
                {
                    return (decimal.Multiply(unitPrice, (decimal)1.05), decimal.Multiply(extPrice, (decimal)1.05));
                }
            }
            // B2B
            else
            {
                if (isGross == true)
                {
                    return (decimal.Divide(unitPrice, (decimal)1.05), decimal.Divide(extPrice, (decimal)1.05));
                }
            }

            return (unitPrice, extPrice);
        }

        public virtual object GetInvoiceFieldValue(ARInvoice invoice, string fieldName)
        {
            ARInvoiceEntry graph = CreateInstance<ARInvoiceEntry>();

            graph.Document.Current = invoice;

            return graph.Document.Cache.GetValue(invoice, fieldName);
        }
        #endregion
    }
}