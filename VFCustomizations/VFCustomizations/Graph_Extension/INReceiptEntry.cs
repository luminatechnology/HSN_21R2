using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.FS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFCustomizations.DAC;
using VFCustomizations.DAC_Extension;
using PX.Objects.CR;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Objects.PO;

namespace PX.Objects.IN
{
    public class INReceiptEntry_VF_Ext : PXGraphExtension<INReceiptEntry>
    {
        public override void Initialize()
        {
            base.Initialize();
            // 判斷能否看到VF Customize 
            var isVisiable = SelectFrom<LUMVerifonePreference>.View.Select(Base).TopFirst?.EnableVFCustomizeField ?? false;
            lumCreateServiceOrder.SetVisible(isVisiable);
        }

        #region Action
        public PXAction<INRegister> lumCreateServiceOrder;
        [PXButton(CommitChanges = true, Category = "Processing")]
        [PXUIField(DisplayName = "Create Service Order", MapEnableRights = PXCacheRights.Select)]
        public virtual IEnumerable LumCreateServiceOrder(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(Base, () => StartCreateServiceOrder(Base));
            return adapter.Get();
        }

        public PXAction<INRegister> viewServiceOrder;
        [PXButton]
        [PXUIField(DisplayName = "View Service Order", Visible = false)]
        public virtual IEnumerable ViewServiceOrder(PXAdapter adapter)
        {
            var row = Base.transactions.Current;
            if (!string.IsNullOrEmpty(row.GetExtension<INTranExtension>()?.UsrServiceOrderNbr))
            {
                var graph = PXGraph.CreateInstance<ServiceOrderEntry>();
                graph.ServiceOrderRecords.Current = SelectFrom<FSServiceOrder>
                                                    .Where<FSServiceOrder.srvOrdType.IsEqual<P.AsString>
                                                      .And<FSServiceOrder.refNbr.IsEqual<P.AsString>>>
                                                    .View.Select(Base, "RSNR", row.GetExtension<INTranExtension>()?.UsrServiceOrderNbr);
                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
            }
            return adapter.Get();
        }

        #endregion

        #region Event
        public virtual void _(Events.RowSelected<INRegister> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);
            // Setting Create Service Order Action enable
            lumCreateServiceOrder.SetEnabled(e.Row.Status != POReceiptStatus.Released);

            var isVisiable = SelectFrom<LUMVerifonePreference>.View.Select(Base).TopFirst?.EnableVFCustomizeField ?? false;
            PXUIFieldAttribute.SetVisible<INTranExtension.usrJobNo>(Base.transactions.Cache, null, isVisiable);
            PXUIFieldAttribute.SetVisible<INTranExtension.usrPhoneNo>(Base.transactions.Cache, null, isVisiable);
            PXUIFieldAttribute.SetVisible<INTranExtension.usrQtySend>(Base.transactions.Cache, null, isVisiable);
            PXUIFieldAttribute.SetVisible<INTranExtension.usrSymptom>(Base.transactions.Cache, null, isVisiable);
            PXUIFieldAttribute.SetVisible<INTranExtension.usrResolution>(Base.transactions.Cache, null, isVisiable);
            PXUIFieldAttribute.SetVisible<INTranExtension.usrOwner>(Base.transactions.Cache, null, isVisiable);
            PXUIFieldAttribute.SetVisible<INTranExtension.usrForMerchant>(Base.transactions.Cache, null, isVisiable);
            PXUIFieldAttribute.SetVisible<INTranExtension.usrServiceOrderNbr>(Base.transactions.Cache, null, isVisiable);
            PXUIFieldAttribute.SetVisible<INTranExtension.usrCreateServiceOrderErrorMsg>(Base.transactions.Cache, null, isVisiable);

            PXUIFieldAttribute.SetEnabled<INTranExtension.usrServiceOrderNbr>(Base.transactions.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<INTranExtension.usrCreateServiceOrderErrorMsg>(Base.transactions.Cache, null, false);

            if (isVisiable)
                Base.receipt.Cache.AllowUpdate = true;
        }

        public virtual void _(Events.RowSelected<INTran> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);
            var isVisiable = SelectFrom<LUMVerifonePreference>.View.Select(Base).TopFirst?.EnableVFCustomizeField ?? false;

            if (e.Row != null && isVisiable && Base.receipt.Current?.Status == INDocStatus.Released)
            {
                Base.receipt.Cache.AllowUpdate = true;
                e.Cache.AllowUpdate = true;
                Base.transactions.Cache.AllowUpdate = true;
                PXUIFieldAttribute.SetEnabled<INTranExtension.usrForMerchant>(e.Cache, null, true);
                PXUIFieldAttribute.SetEnabled<INTran.lotSerialNbr>(e.Cache, null, false);
            }
        }

        #endregion

        #region Method

        /// <summary> 準備執行建立Service Order </summary>
        public static void StartCreateServiceOrder(INReceiptEntry baseGraph)
        {
            // 只執行Lot SerialNbr. is not null && service order is null
            var receiptDoc = baseGraph.receipt.Current;
            var receiptLine = baseGraph.transactions.Select().RowCast<INTran>().Where(x => !string.IsNullOrEmpty(x?.LotSerialNbr) && string.IsNullOrEmpty(x.GetExtension<INTranExtension>()?.UsrServiceOrderNbr)).ToList();
            var defaultServiceItem = SelectFrom<LUMVerifonePreference>.View.Select(baseGraph).TopFirst?.DefaultServiceInventoryID;
            var skipItemClassID = SelectFrom<INItemClass>.Where<INItemClass.itemClassCD.IsEqual<P.AsString>>
                                  .View.Select(baseGraph, "SIM CARD").TopFirst?.ItemClassID;
            foreach (var item in receiptLine)
            {
                try
                {
                    // Skip ItemClass = SIM CARD
                    var inventoryInfo = InventoryItem.PK.Find(baseGraph, item?.InventoryID);
                    if (inventoryInfo?.ItemClassID == skipItemClassID)
                        continue;
                    var srvGraph = PXGraph.CreateInstance<ServiceOrderEntry>();
                    // 处理每个记录
                    ProcessingServiceOrder(srvGraph, item, receiptDoc, defaultServiceItem);
                }
                catch (PXOuterException ex)
                {
                    for (int i = 0; i < ex.InnerFields.Length; i++)
                        item.GetExtension<INTranExtension>().UsrCreateServiceOrderErrorMsg += $"{ex.InnerFields[i]} - {ex.InnerMessages[i]} \r\n";
                }
                catch (Exception ex)
                {
                    item.GetExtension<INTranExtension>().UsrCreateServiceOrderErrorMsg = ex.Message;
                }
                finally
                {
                    baseGraph.transactions.Update(item);
                }

            }
            baseGraph.Save.Press();
        }

        /// <summary> 執行建立Service Order</summary>
        public static void ProcessingServiceOrder(ServiceOrderEntry srvGraph, INTran item, INRegister receiptDoc, int? defaultServiceItem)
        {
            #region Header
            var itemExt = item.GetExtension<INTranExtension>();
            var doc = srvGraph.ServiceOrderRecords.Cache.CreateInstance() as FSServiceOrder;
            doc.SrvOrdType = "RSNR";
            doc.CustomerID = SelectFrom<BAccount2>.
                             Where<BAccount2.acctCD.IsEqual<P.AsString>>.
                             View.Select(srvGraph, itemExt?.UsrOwner).TopFirst?.BAccountID;
            doc.BranchLocationID = SelectFrom<FSBranchLocation>.
                                   Where<FSBranchLocation.branchLocationCD.IsEqual<P.AsString>>.
                                   View.Select(srvGraph, "HSNT").TopFirst?.BranchLocationID;
            doc.CustPORefNbr = itemExt?.UsrJobNo;
            doc.CustWorkOrderRefNbr = item?.LotSerialNbr;
            doc.DocDesc = $"{itemExt?.UsrOwner} / {item?.TranDesc} / SN:{item?.LotSerialNbr} /";
            doc = srvGraph.ServiceOrderRecords.Insert(doc);
            #endregion

            #region Attribute
            var attributeList = srvGraph.Answers.Select().RowCast<CSAnswers>().ToList();
            foreach (var attr in attributeList)
            {
                switch (attr.AttributeID?.ToUpper())
                {
                    case "BRAND":
                        attr.Value = "Verifone";
                        break;
                    case "CSTPREPAYM":
                        attr.Value = itemExt?.UsrResolution;
                        break;
                    case "RESYMPTOM":
                        attr.Value = itemExt?.UsrSymptom;
                        break;
                    case "WARRANTY":
                        attr.Value = "IN";
                        break;
                    case "RECEIPTNBR":
                        attr.Value = receiptDoc?.RefNbr;
                        break;
                }
                srvGraph.Answers.Update(attr);
            }
            #endregion

            #region Details(Line)
            if (defaultServiceItem.HasValue)
            {
                var line = srvGraph.ServiceOrderDetails.Cache.CreateInstance() as FSSODet;
                line.InventoryID = defaultServiceItem;
                line.SMEquipmentID = SelectFrom<FSEquipment>.
                                     Where<FSEquipment.serialNumber.IsEqual<P.AsString>>.
                                     View.Select(srvGraph, item.LotSerialNbr).TopFirst?.SMEquipmentID;
                srvGraph.ServiceOrderDetails.Insert(line);
            }
            #endregion

            srvGraph.Save.Press();
            itemExt.UsrServiceOrderNbr = doc.RefNbr;
            itemExt.UsrCreateServiceOrderErrorMsg = null;
        }

        #endregion
    }
}
