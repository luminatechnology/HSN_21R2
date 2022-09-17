using Newtonsoft.Json;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFCustomizations.DAC;
using VFCustomizations.Descriptor;
using VFCustomizations.Json_Entity.FTP22;

namespace VFCustomizations.Graph
{
    public class LUMVFAPI2102Process : PXGraph<LUMVFAPI2102Process>
    {
        public PXSave<LUMVFAPISetupHoldResult> Save;
        public PXCancel<LUMVFAPISetupHoldResult> Cancel;

        public PXProcessing<LUMVFAPISetupHoldResult, Where<LUMVFAPISetupHoldResult.processed, Equal<False>, Or<LUMVFAPISetupHoldResult.processed, IsNull>>> Transactions;

        public LUMVFAPI2102Process()
        {
            this.Transactions.AllowUpdate = true;
            this.Transactions.SetProcessDelegate(delegate (List<LUMVFAPISetupHoldResult> list)
            {
                GoProcessing(list);
            });
        }

        #region Method

        public static void GoProcessing(List<LUMVFAPISetupHoldResult> list)
        {
            var baseGraph = CreateInstance<LUMVFAPI2102Process>();
            baseGraph.SendDataToAPI2102(list, baseGraph);
        }

        public virtual void SendDataToAPI2102(List<LUMVFAPISetupHoldResult> selectedList, LUMVFAPI2102Process baseGraph)
        {
            PXUIFieldAttribute.SetEnabled<LUMVFAPISetupHoldResult.previousCommitDate>(this.Transactions.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<LUMVFAPISetupHoldResult.processedDateTime>(this.Transactions.Cache, null, true);
            VFApiHelper helper = new VFApiHelper();
            // Get Access Token
            var apiTokenObj = helper.GetAccessToken();
            if (apiTokenObj == null)
                throw new PXException("Can not can Access Token!!");
            foreach (var item in selectedList)
            {
                var errorMsg = string.Empty;
                VFFTP22Entity entity = new VFFTP22Entity();
                try
                {
                    PXProcessing.SetCurrentItem(item);
                    entity.JobNo = item.JobNo.Trim();
                    entity.IncidentCatalogName = item.IncidentCatalogName;
                    entity.CommitDate = item.CommitDate?.ToString("dd/MM/yyyy HH:mm");
                    entity.PreviousCommitDate = item.PreviousCommitDate?.ToString("dd/MM/yyyy HH:mm");
                    entity.HoldReason = item.HoldReason;
                    entity.HoldDate = item.HoldDate?.ToString("dd/MM/yyyy HH:mm");
                    entity.HoldStatus = item.HoldSatus;
                    var ftpResponse = helper.CallFTP22(entity, apiTokenObj);
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
                        item.Processed = true;
                    else
                        PXProcessing.SetError("errorMsg");
                    PXNoteAttribute.SetNote(baseGraph.Transactions.Cache, item, errorMsg + "  " + JsonConvert.SerializeObject(entity));
                    baseGraph.Transactions.Update(item);
                    baseGraph.Actions.PressSave();
                }
            }

        }

        #endregion
    }
}
