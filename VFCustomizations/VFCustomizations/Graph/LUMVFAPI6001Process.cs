using Newtonsoft.Json;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFCustomizations.DAC;
using VFCustomizations.Descriptor;
using VFCustomizations.Json_Entity.FTP6;

namespace VFCustomizations.Graph
{
    public class LUMVFAPI6001Process : PXGraph<LUMVFAPI6001Process>
    {
        public PXSave<INRegister> Save;
        public PXCancel<INRegister> Cancel;
        public PXProcessing<INRegister> Transactions;

        public LUMVFAPI6001Process()
        {
            this.Transactions.AllowUpdate = true;
            this.Transactions.SetProcessDelegate(delegate (List<INRegister> list)
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
                   PXView.Filters, ref startrow, 100000, ref totalrow);
            PXView.StartRow = 0;
            foreach (INRegister row in result)
            {
                var attrAPI6001 = this.Transactions.Cache.GetValueExt(row, PX.Objects.CS.Messages.Attribute + "API6001") as PXFieldState;
                var attrAWBNo = this.Transactions.Cache.GetValueExt(row, PX.Objects.CS.Messages.Attribute + "AWBNO") as PXFieldState;
                if ((bool?)attrAPI6001?.Value != true && !string.IsNullOrEmpty((string)attrAWBNo?.Value) && !string.IsNullOrEmpty(row.ExtRefNbr) && row.Status == "R")
                    yield return row;
            }
        }

        #endregion

        #region Method

        public static void GoProcessing(List<INRegister> list)
        {
            var baseGraph = CreateInstance<LUMVFAPI6001Process>();
            baseGraph.SendDataToAPI6001(list, baseGraph);
        }

        public virtual void SendDataToAPI6001(List<INRegister> selectedList, LUMVFAPI6001Process baseGraph)
        {
            VFApiHelper helper = new VFApiHelper();
            // Get Access Token
            var apiTokenObj = helper.GetAccessToken();
            if (apiTokenObj == null)
                throw new PXException("Can not can Access Token!!");

            foreach (var selectedItem in selectedList)
            {
                var errorMsg = string.Empty;
                VFFTP6Entity entity = new VFFTP6Entity();
                try
                {
                    PXProcessing.SetCurrentItem(selectedItem);
                    entity.DeliveryNo = selectedItem?.ExtRefNbr;
                    //entity.DeliveryDate = selectedItem.TranDate?.ToString("dd/MM/yyyy HH:mm");
                    entity.ETDDate = selectedItem.TranDate?.AddTicks(PX.Common.PXTimeZoneInfo.Now.TimeOfDay.Ticks).ToString("dd/MM/yyyy HH:mm");
                    // Get Attribute AWBNO
                    var attrAWBNO = Transactions.Cache.GetValueExt(selectedItem, PX.Objects.CS.Messages.Attribute + "AWBNO") as PXFieldState;
                    entity.AWBNo = (string)attrAWBNO?.Value;
                    // Get Attribute AWBNO
                    var attrFORWARDER = Transactions.Cache.GetValueExt(selectedItem, PX.Objects.CS.Messages.Attribute + "FORWARDER") as PXFieldState;
                    entity.Forwarder = (string)attrFORWARDER?.Value;
                    entity.WarehouseLocation = "RMA";
                    entity.ShipToCode = (string)(Transactions.Cache.GetValueExt(selectedItem, PX.Objects.CS.Messages.Attribute + "SHIPTOCODE") as PXFieldState)?.Value;
                    entity.ShipToName = (string)(Transactions.Cache.GetValueExt(selectedItem, PX.Objects.CS.Messages.Attribute + "SHIPTONAME") as PXFieldState)?.Value;
                    entity.ShipToAddress = (string)(Transactions.Cache.GetValueExt(selectedItem, PX.Objects.CS.Messages.Attribute + "SHIPTOADDR") as PXFieldState)?.Value;
                    entity.ShipToContact = (string)(Transactions.Cache.GetValueExt(selectedItem, PX.Objects.CS.Messages.Attribute + "SHIPTOCONT") as PXFieldState)?.Value;
                    entity.ShipFromCode = (string)(Transactions.Cache.GetValueExt(selectedItem, PX.Objects.CS.Messages.Attribute + "SHIPFRCODE") as PXFieldState)?.Value;
                    entity.ShipFromName = (string)(Transactions.Cache.GetValueExt(selectedItem, PX.Objects.CS.Messages.Attribute + "SHIPFRNAME") as PXFieldState)?.Value;
                    entity.ShipFromAddress = (string)(Transactions.Cache.GetValueExt(selectedItem, PX.Objects.CS.Messages.Attribute + "SHIPFRADDR") as PXFieldState)?.Value;
                    entity.ShipFromContact = (string)(Transactions.Cache.GetValueExt(selectedItem, PX.Objects.CS.Messages.Attribute + "SHIPFRCONT") as PXFieldState)?.Value;
                    entity.JobItems = new List<JobItem>();

                    // Details
                    var inTranDetails = SelectFrom<INTran>
                                        .Where<INTran.refNbr.IsEqual<P.AsString>>
                                        .View.Select(baseGraph, selectedItem.RefNbr).RowCast<INTran>();
                    foreach (var jobGroupItems in inTranDetails.GroupBy(x => x.GetExtension<VFCustomizations.DAC_Extension.INTranExtension>().UsrJobNo))
                    {
                        var _jobItem = new JobItem();
                        _jobItem.JobNo = jobGroupItems.Key;
                        _jobItem.Items = new List<Item>();
                        foreach (var item in jobGroupItems)
                        {
                            var inventoryInfo = InventoryItem.PK.Find(baseGraph, item.InventoryID);
                            var intranSplit = SelectFrom<INTranSplit>
                                              .Where<INTranSplit.refNbr.IsEqual<P.AsString>
                                                .And<INTranSplit.lineNbr.IsEqual<P.AsInt>>>
                                              .View.Select(baseGraph, item?.RefNbr, item?.LineNbr).TopFirst;
                            _jobItem.Items.Add(new Item()
                            {
                                PartNo = inventoryInfo.InventoryCD.Trim(),
                                SerialNo = intranSplit?.LotSerialNbr,
                                PhoneNo = item.GetExtension<VFCustomizations.DAC_Extension.INTranExtension>()?.UsrPhoneNo,
                                QTYSend = (int)(item.GetExtension<VFCustomizations.DAC_Extension.INTranExtension>()?.UsrQtySend ?? 0),
                                QTYReceive = (int)(item.Qty ?? 0),
                                ReceiveDate = item.LastModifiedDateTime?.ToString("dd/MM/yyyy HH:mm"),
                                Owner = item.GetExtension<VFCustomizations.DAC_Extension.INTranExtension>()?.UsrOwner
                            });
                        }
                        entity.JobItems.Add(_jobItem);
                        // Get Attribute PACKINGNO
                        var attrPACKINGNO = Transactions.Cache.GetValueExt(selectedItem, PX.Objects.CS.Messages.Attribute + "PACKINGNO") as PXFieldState;
                        entity.PackingNo = (string)attrPACKINGNO?.Value;
                        entity.ExportDate = PX.Common.PXTimeZoneInfo.Now.ToString("dd/MM/yyyy HH:mm");
                    }

                    var ftpResponse = helper.CallFTP6(entity, apiTokenObj);
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
                    PXNoteAttribute.SetNote(baseGraph.Transactions.Cache, selectedItem, errorMsg + "  " + JsonConvert.SerializeObject(entity));
                    baseGraph.Actions.PressSave();
                    // Success
                    if (string.IsNullOrEmpty(errorMsg))
                        InsertOrUpdateKvextManual(selectedItem.NoteID.Value, baseGraph);
                    else
                        PXProcessing.SetError("errorMsg");
                }
            }

        }

        /// <summary> 手動Insert/Update Attribute </summary>
        public void InsertOrUpdateKvextManual(Guid noteid, LUMVFAPI6001Process baseGraph)
        {
            var kvextInfo = SelectFrom<INRegisterKvExt>
                            .Where<INRegisterKvExt.recordID.IsEqual<P.AsGuid>>
                            .View.Select(baseGraph, noteid).RowCast<INRegisterKvExt>();

            #region Insert or Update Attribute API6001 to 1
            if (kvextInfo.Any(x => x.FieldName == PX.Objects.CS.Messages.Attribute + "API6001"))
            {
                // Update
                PXUpdate<Set<INRegisterKvExt.valueNumeric, Required<INRegisterKvExt.valueNumeric>>,
                    INRegisterKvExt,
                    Where<INRegisterKvExt.recordID, Equal<Required<INRegisterKvExt.recordID>>,
                      And<INRegisterKvExt.fieldName, Equal<Required<INRegisterKvExt.fieldName>>>>>
                    .Update(baseGraph, 1, noteid, PX.Objects.CS.Messages.Attribute + "API6001");
            }
            else
            {
                // Insert
                List<PXDataFieldAssign> assigns = new List<PXDataFieldAssign>();
                assigns.Add(new PXDataFieldAssign<INRegisterKvExt.recordID>(noteid));
                assigns.Add(new PXDataFieldAssign<INRegisterKvExt.valueNumeric>(1));
                assigns.Add(new PXDataFieldAssign<INRegisterKvExt.fieldName>(PX.Objects.CS.Messages.Attribute + "API6001"));
                PXDatabase.Insert<INRegisterKvExt>(assigns.ToArray());
            }
            #endregion

            #region Insert or Update Attribute API6001DT
            if (kvextInfo.Any(x => x.FieldName == PX.Objects.CS.Messages.Attribute + "API6001DT"))
            {
                // Update
                PXUpdate<Set<INRegisterKvExt.valueNumeric, Required<INRegisterKvExt.valueDate>>,
                    INRegisterKvExt,
                    Where<INRegisterKvExt.recordID, Equal<Required<INRegisterKvExt.recordID>>,
                      And<INRegisterKvExt.fieldName, Equal<Required<INRegisterKvExt.fieldName>>>>>
                    .Update(baseGraph, DateTime.Now, noteid, PX.Objects.CS.Messages.Attribute + "API6001");
            }
            else
            {
                // Insert
                List<PXDataFieldAssign> assigns = new List<PXDataFieldAssign>();
                assigns.Add(new PXDataFieldAssign<INRegisterKvExt.recordID>(noteid));
                assigns.Add(new PXDataFieldAssign<INRegisterKvExt.valueDate>(DateTime.Now));
                assigns.Add(new PXDataFieldAssign<INRegisterKvExt.fieldName>(PX.Objects.CS.Messages.Attribute + "API6001DT"));
                PXDatabase.Insert<INRegisterKvExt>(assigns.ToArray());
            }
            #endregion
        }

        #endregion
    }
}
