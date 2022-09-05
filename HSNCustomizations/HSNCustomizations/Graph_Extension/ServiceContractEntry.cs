using PX.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS
{
    public class ServiceContractEntry_Extension : PXGraphExtension<ServiceContractEntry>
    {
        //public IEnumerable contractPeriodRecords()
        //{
        //    PXView select = new PXView(Base, false, Base.ContractPeriodDetRecords.View.BqlSelect);
        //    Int32 totalrow = 0;
        //    Int32 startrow = PXView.StartRow;
        //    List<object> result = select.Select(PXView.Currents, PXView.Parameters,
        //           PXView.Searches, PXView.SortColumns, PXView.Descendings,
        //           PXView.Filters, ref startrow, 1, ref totalrow);
        //    PXView.StartRow = startrow + 1;
        //    //foreach (PXResult<FSContractPeriod> row in result)
        //    //{
        //    //    //Do any dynamic calculations
        //    //}
        //    return result;
        //}
    }
}
