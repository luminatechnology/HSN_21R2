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
    public class LUMWarrantyHistoryQuery : PXGraph<LUMWarrantyHistoryQuery>
    {

        public PXFilter<WarrantyHistoryFitler> Filter;

        public SelectFrom<LUMWarrantyHistory>
              .Where<LUMWarrantyHistory.sMEquipmentID.IsEqual<WarrantyHistoryFitler.sMEquipmentID.FromCurrent>>
              .View WarrantyHistory;

    }

    public class WarrantyHistoryFitler : IBqlTable
    {
        #region SMEquipmentID
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "SMEquipment ID", Visible = false)]
        public virtual int? SMEquipmentID { get; set; }
        public abstract class sMEquipmentID : PX.Data.BQL.BqlInt.Field<sMEquipmentID> { }
        #endregion
    }
}
