using PX.Data;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFCustomizations.DAC;

namespace VFCustomizations.Graph
{
    public class LUMVFAPIInterfaceProcess : PXGraph<LUMVFAPIInterfaceProcess>
    {
        public PXSave<LUMVFAPIInterfaceData> Save;
        public PXCancel<LUMVFAPIInterfaceData> Cancel;

        public PXProcessing<LUMVFAPIInterfaceData> VFSourceData;
        public SelectFrom<LUMVFAPIInterfaceData>
               .Where<LUMVFAPIInterfaceData.sequenceNumber.IsEqual<LUMVFAPIInterfaceData.sequenceNumber.FromCurrent>>
               .View JsonViewer;

        public LUMVFAPIInterfaceProcess()
        {
            VFSourceData.Cache.AllowInsert = VFSourceData.Cache.AllowUpdate = VFSourceData.Cache.AllowDelete = true;

            PXUIFieldAttribute.SetEnabled<LUMVFAPIInterfaceData.apiname>(VFSourceData.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<LUMVFAPIInterfaceData.uniqueID>(VFSourceData.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<LUMVFAPIInterfaceData.serviceType>(VFSourceData.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<LUMVFAPIInterfaceData.jsonSource>(VFSourceData.Cache, null, true);
        }

        public PXAction<LUMVFAPIInterfaceData> ViewJson;
        [PXButton]
        [PXUIField(DisplayName = "View Json", MapEnableRights = PXCacheRights.Select)]
        protected void viewJson()
        {
            if (JsonViewer.AskExt(true) != WebDialogResult.OK) return;
        }
    }
}
