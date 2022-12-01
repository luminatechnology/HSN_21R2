using PX.Data;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNFinance.Graph
{
    public class LUMRevenueInventoryAccountMaint : PXGraph<LUMRevenueInventoryAccountMaint>
    {
        public PXSave<LUMRevenueInventoryAccounts> Save;
        public PXCancel<LUMRevenueInventoryAccounts> Cancel;
        public SelectFrom<LUMRevenueInventoryAccounts>.View RevenueAcctMapping;
    }
}
