using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.SO;
using System.Collections;
using VFCustomizations.Descriptor;
using VFCustomizations.Json_Entity.FTP3;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Objects.IN;
using VFCustomizations.DAC;

namespace VFCustomizations.Graph
{
    public class LUMVFAPI3001Process : PXGraph<LUMVFAPI3001Process>
    {
        public PXSave<SOShipment> Save;
        public PXCancel<SOShipment> Cancel;

        public PXProcessing<SOShipment, Where<SOShipment.status, Equal<SOShipmentStatus.completed>, And<SOShipment.operation, Equal<SOOperation.issue>>>> Transactions;
        public SelectFrom<SOOrder>
               .Where<SOOrder.orderType.IsEqual<SOOrder.orderType.AsOptional>
                   .And<SOOrder.orderNbr.IsEqual<SOOrder.orderNbr.AsOptional>>>
               .View SaleOrderDocument;

        public LUMVFAPI3001Process()
        {
            this.Transactions.AllowUpdate = true;
            this.Transactions.SetProcessDelegate(delegate (List<SOShipment> list)
            {
                GoProcessing(list);
            });
        }

        #region Delegate Data View

        public IEnumerable transactions()
        {
            PXView select = new PXView(this, true, this.Transactions.View.BqlSelect);
            Int32 totalrow = 0;
            Int32 startrow = PXView.StartRow;
            List<object> result = select.Select(PXView.Currents, PXView.Parameters,
                   PXView.Searches, PXView.SortColumns, PXView.Descendings,
                   PXView.Filters, ref startrow, PXView.MaximumRows, ref totalrow);
            PXView.StartRow = 0;
            foreach (SOShipment row in result)
            {
                var attr = this.Transactions.Cache.GetValueExt(row, PX.Objects.CS.Messages.Attribute + "API3001") as PXFieldState;
                if ((bool?)attr.Value != true)
                    yield return row;
            }
        }

        #endregion

        #region Method

        public static void GoProcessing(List<SOShipment> list)
        {
            var baseGraph = CreateInstance<LUMVFAPI3001Process>();
            baseGraph.SendDataToAPI3010(list, baseGraph);
        }

        public virtual void SendDataToAPI3010(List<SOShipment> selectedList, LUMVFAPI3001Process baseGraph)
        {
            VFApiHelper helper = new VFApiHelper();
            // Get Access Token
            var apiTokenObj = helper.GetAccessToken();
            if (apiTokenObj == null)
                throw new PXException("Can not can Access Token!!");

            foreach (var selectedItem in selectedList)
            {
                var errorMsg = string.Empty;
                try
                {
                    PXProcessing.SetCurrentItem(selectedItem);

                    // Get ShipmentLine
                    var shipmentLine = SelectFrom<SOShipLine>
                                       .Where<SOShipLine.shipmentNbr.IsEqual<P.AsString>
                                         .And<SOShipLine.shipmentType.IsEqual<P.AsString>>>
                                       .View.Select(baseGraph, selectedItem.ShipmentNbr, selectedItem.ShipmentType).RowCast<SOShipLine>();
                    // Get First SO Record
                    var firstSORecord = SaleOrderDocument.Select(shipmentLine.FirstOrDefault()?.OrigOrderType, shipmentLine.FirstOrDefault()?.OrigOrderNbr).TopFirst;
                    // Get Ship to Contact Info
                    var shipContactInfo = SOShipmentContact.PK.Find(baseGraph, selectedItem.ShipContactID);
                    VFFTP3Entity entity = new VFFTP3Entity();
                    entity.DeliveryNo = selectedItem.ShipmentNbr;
                    entity.DeliveryDate = selectedItem.ShipDate?.ToString("dd/MM/yyyy HH:mm");
                    // SalesOrder Attribute SHIPTOCODE
                    var soAttrShipToCode = SaleOrderDocument.Cache.GetValueExt(firstSORecord, PX.Objects.CS.Messages.Attribute + "SHIPTOCODE") as PXFieldState;
                    entity.ShipToCode = (string)soAttrShipToCode.Value;
                    // Shipment Attribute AWBNO
                    var shipmentAttrAWBNO = Transactions.Cache.GetValueExt(selectedItem, PX.Objects.CS.Messages.Attribute + "AWBNO") as PXFieldState;
                    entity.AWBNo = (string)shipmentAttrAWBNO.Value;
                    entity.ExportDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                    // Get SOSHipment
                    var soShipmentInfo = SelectFrom<SOOrderShipment>
                                         .Where<SOOrderShipment.shipmentNbr.IsEqual<P.AsString>
                                           .And<SOOrderShipment.shipmentType.IsEqual<P.AsString>>>
                                         .View.Select(baseGraph, selectedItem.ShipmentNbr, selectedItem.ShipmentType).RowCast<SOOrderShipment>();
                    // Add SalesOrder into Jobitem
                    entity.JobItems = new List<JobItem>();
                    foreach (var soShipment in soShipmentInfo)
                    {
                        var jobSalesOrder = SOOrder.PK.Find(baseGraph, soShipment.OrderType, soShipment.OrderNbr);
                        JobItem jobitem = new JobItem();
                        jobitem.JobNo = jobSalesOrder?.CustomerOrderNbr;
                        jobitem.Items = new List<Item>();
                        // Add Shipline into Jobitem.Items
                        foreach (var shipLine in shipmentLine.Where(x => x.OrigOrderType == jobSalesOrder?.OrderType && x.OrigOrderNbr == jobSalesOrder?.OrderNbr))
                        {
                            // Get ShiplineSplit Info
                            var shiplineSplit = SelectFrom<SOShipLineSplit>
                                                .Where<SOShipLineSplit.shipmentNbr.IsEqual<P.AsString>
                                                  .And<SOShipLineSplit.lineNbr.IsEqual<P.AsInt>>>
                                                .View.SelectSingleBound(baseGraph, null, shipLine.ShipmentNbr, shipLine.LineNbr).TopFirst;
                            // Get Shipline InventoryItem info
                            var inventoryInfo = InventoryItem.PK.Find(baseGraph, shipLine.InventoryID);
                            // Get SoLine Info
                            var soLine = SOLine.PK.Find(baseGraph, shipLine.OrigOrderType, shipLine.OrigOrderNbr, shipLine.OrigLineNbr);
                            jobitem.Items.Add(new Item()
                            {
                                PartNo = inventoryInfo.InventoryCD.Trim(),
                                QTY = (int)(soLine.Qty ?? 0),
                                Serial = shiplineSplit?.LotSerialNbr,
                                PhoneNo = soLine.AlternateID,
                                TLACondition = string.IsNullOrEmpty(shiplineSplit?.LotSerialNbr) ? null :
                                               string.IsNullOrEmpty(shipLine.ReasonCode) ? "New" : shipLine.ReasonCode
                            });
                        }
                        entity.JobItems.Add(jobitem);
                    }
                    // Shipment Attribute PACKINGNO
                    var shipmentAttrPACKINGNO = Transactions.Cache.GetValueExt(selectedItem, PX.Objects.CS.Messages.Attribute + "PACKINGNO") as PXFieldState;
                    entity.PackingNo = (string)shipmentAttrPACKINGNO.Value;
                    // Shipment Attribute ETA
                    var shipmentAttrETA = Transactions.Cache.GetValueExt(selectedItem, PX.Objects.CS.Messages.Attribute + "ETA") as PXFieldState;
                    entity.ETA = shipmentAttrETA.Value;
                    // Sales Order Attribute SHIPVIA
                    var soAttributeSHIPVIA = SaleOrderDocument.Cache.GetValueExt(firstSORecord, PX.Objects.CS.Messages.Attribute + "SHIPVIA") as PXFieldState;
                    entity.ShipVia = soAttributeSHIPVIA.Value;
                    entity.ShipToName = shipContactInfo?.FullName;
                    // Shipment Attribute FORWARDER
                    var shipmentAttrFORWARDER = Transactions.Cache.GetValueExt(selectedItem, PX.Objects.CS.Messages.Attribute + "FORWARDER") as PXFieldState;
                    entity.ForwarderName = (string)shipmentAttrFORWARDER.Value;

                    var ftpResponse = helper.CallFTP3(entity, apiTokenObj);
                    if (ftpResponse == null)
                        throw new Exception("Call API FTP fail");
                    if (ftpResponse.ResponseCode != "0")
                        throw new Exception(ftpResponse.ErrorMessage);
                }
                catch (PXOuterException ex)
                {
                    errorMsg = ex.InnerMessages[0];
                }
                catch (Exception ex)
                {
                    errorMsg = ex.Message;
                }
                finally
                {
                    // Success
                    if (string.IsNullOrEmpty(errorMsg))
                        InsertOrUpdateKvextManual(selectedItem.NoteID.Value, baseGraph);
                    else
                    {
                        PXNoteAttribute.SetNote(baseGraph.Transactions.Cache, selectedItem, errorMsg);
                        PXProcessing.SetError("errorMsg");
                    }
                    baseGraph.Actions.PressSave();
                }
            }

        }

        /// <summary> 手動Insert/Update Attribute </summary>
        public void InsertOrUpdateKvextManual(Guid noteid, LUMVFAPI3001Process baseGraph)
        {
            var kvextInfo = SelectFrom<SOShipmentKvExt>
                            .Where<SOShipmentKvExt.recordID.IsEqual<P.AsGuid>>
                            .View.Select(baseGraph, noteid).RowCast<SOShipmentKvExt>();

            #region Insert or Update Attribute API3001 to 1
            if (kvextInfo.Any(x => x.FieldName == PX.Objects.CS.Messages.Attribute + "API3001"))
            {
                // Update
                PXUpdate<Set<SOShipmentKvExt.valueNumeric, Required<SOShipmentKvExt.valueNumeric>>,
                    SOShipmentKvExt,
                    Where<SOShipmentKvExt.recordID, Equal<Required<SOShipmentKvExt.recordID>>,
                      And<SOShipmentKvExt.fieldName, Equal<Required<SOShipmentKvExt.fieldName>>>>>
                    .Update(baseGraph, 1, noteid, PX.Objects.CS.Messages.Attribute + "API3001");
            }
            else
            {
                // Insert
                List<PXDataFieldAssign> assigns = new List<PXDataFieldAssign>();
                assigns.Add(new PXDataFieldAssign<SOShipmentKvExt.recordID>(noteid));
                assigns.Add(new PXDataFieldAssign<SOShipmentKvExt.valueNumeric>(1));
                assigns.Add(new PXDataFieldAssign<SOShipmentKvExt.fieldName>(PX.Objects.CS.Messages.Attribute + "API3001"));
                PXDatabase.Insert<SOShipmentKvExt>(assigns.ToArray());
            }
            #endregion

            #region Insert or Update Attribute API3001DT
            if (kvextInfo.Any(x => x.FieldName == PX.Objects.CS.Messages.Attribute + "API3001DT"))
            {
                // Update
                PXUpdate<Set<SOShipmentKvExt.valueNumeric, Required<SOShipmentKvExt.valueDate>>,
                    SOShipmentKvExt,
                    Where<SOShipmentKvExt.recordID, Equal<Required<SOShipmentKvExt.recordID>>,
                      And<SOShipmentKvExt.fieldName, Equal<Required<SOShipmentKvExt.fieldName>>>>>
                    .Update(baseGraph, DateTime.Now, noteid, PX.Objects.CS.Messages.Attribute + "API3001");
            }
            else
            {
                // Insert
                List<PXDataFieldAssign> assigns = new List<PXDataFieldAssign>();
                assigns.Add(new PXDataFieldAssign<SOShipmentKvExt.recordID>(noteid));
                assigns.Add(new PXDataFieldAssign<SOShipmentKvExt.valueDate>(DateTime.Now));
                assigns.Add(new PXDataFieldAssign<SOShipmentKvExt.fieldName>(PX.Objects.CS.Messages.Attribute + "API3001DT"));
                PXDatabase.Insert<SOShipmentKvExt>(assigns.ToArray());
            }
            #endregion
        }

        #endregion
    }
}
