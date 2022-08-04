using HSNCustomizations.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNCustomizations.Graph
{
    //[Phase II - Staff Selection for Customer Locations]
    public class LUMCustomerStaffMaint : PXGraph<LUMCustomerStaffMaint>
    {
        public PXSave<LUMCustomerStaffMapping> Save;
        public PXCancel<LUMCustomerStaffMapping> Cancel;

        [PXImport(typeof(LUMCustomerStaffMapping))]
        public SelectFrom<LUMCustomerStaffMapping>.View MappingList;

        #region Events

        public virtual void _(Events.FieldUpdated<LUMCustomerStaffMapping.customerID> e)
        {
            object newLocationID;
            e.Cache.RaiseFieldDefaulting<LUMCustomerStaffMapping.locationID>(e.Row, out newLocationID);
        }

        #endregion
    }
}
