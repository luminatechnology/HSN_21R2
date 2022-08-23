using Newtonsoft.Json;
using PX.Data;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFCustomizations.DAC;
using VFCustomizations.Descriptor;
using VFCustomizations.Json_Entity.FTP21;

namespace VFCustomizations.Graph
{
    public class LUMVFAPI2101Process : PXGraph<LUMVFAPI2101Process>
    {
        public PXSave<LUMVFApisetupResult> Save;
        public PXCancel<LUMVFApisetupResult> Cancel;

        public PXProcessing<LUMVFApisetupResult, Where<LUMVFApisetupResult.isProcessed, Equal<False>, Or<LUMVFApisetupResult.isProcessed, IsNull>>> Transactions;

        public LUMVFAPI2101Process()
        {
            this.Transactions.AllowUpdate = true;
            this.Transactions.SetProcessDelegate(delegate (List<LUMVFApisetupResult> list)
            {
                GoProcessing(list);
            });
        }

        #region Method

        public static void GoProcessing(List<LUMVFApisetupResult> list)
        {
            var baseGraph = CreateInstance<LUMVFAPI2101Process>();
            baseGraph.SendDataToAPI2101(list, baseGraph);
        }

        public virtual void SendDataToAPI2101(List<LUMVFApisetupResult> selectedList, LUMVFAPI2101Process baseGraph)
        {
            PXUIFieldAttribute.SetEnabled<LUMVFApisetupResult.isProcessed>(this.Transactions.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<LUMVFApisetupResult.processedDateTime>(this.Transactions.Cache, null, true);
            VFApiHelper helper = new VFApiHelper();
            // Get Access Token
            var apiTokenObj = helper.GetAccessToken();
            if (apiTokenObj == null)
                throw new PXException("Can not can Access Token!!");
            foreach (var item in selectedList)
            {
                var errorMsg = string.Empty;
                VFFTP21Entity entity = new VFFTP21Entity();
                try
                {
                    PXProcessing.SetCurrentItem(item);
                    entity.JobNo = item.JobNo.Trim();
                    entity.TerminalID = item.TerminalID;
                    entity.SerialNo = item.SerialNo;
                    entity.StartDateTime = item.StartDateTime.Value.ToString("dd-MM-yyyy HH:mm");
                    entity.FinishDateTime = item.FinishDateTime.Value.ToString("dd-MM-yyyy HH:mm");
                    entity.SetupReason = item.SetupReason;
                    var ftpResponse = helper.CallFTP21(entity, apiTokenObj);
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
                    item.ProcessedDateTime = DateTime.Now;
                    if (string.IsNullOrEmpty(errorMsg))
                        item.IsProcessed = true;
                    else
                    {
                        PXNoteAttribute.SetNote(baseGraph.Transactions.Cache, item, errorMsg + "  " + JsonConvert.SerializeObject(entity));
                        PXProcessing.SetError("errorMsg");
                    }
                    baseGraph.Transactions.Update(item);
                    baseGraph.Actions.PressSave();
                }
            }

        }

        #endregion

    }
}
