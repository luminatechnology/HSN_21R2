using PX.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PX.Objects.FS
{
    public class ServiceContractEntry_Extension : PXGraphExtension<ServiceContractEntry>
    {
        // [Phase II] - Service Contract Performance issue
        public IEnumerable contractPeriodDetRecords()
        {
            var maxRow = PXView.MaximumRows;
            // 只有Export to excel才需要Select all data
            if (HttpContext.Current != null)                
                maxRow = HttpContext.Current.Request?.Params["__CALLBACKPARAM"]?.Split('|')[0] == "ExportExcel" ? PXView.MaximumRows : 15;
            PXView select = new PXView(Base, false, Base.ContractPeriodDetRecords.View.BqlSelect);
            Int32 totalrow = 0;
            Int32 startrow = PXView.StartRow;
            List<object> result = select.Select(PXView.Currents, PXView.Parameters,
                   PXView.Searches, PXView.SortColumns, PXView.Descendings,
                   PXView.Filters, ref startrow, maxRow, ref totalrow);
            PXView.StartRow = 0;
            return result;
        }
    }
}
