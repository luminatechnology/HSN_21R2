using PX.Data;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNCustomizations.Graph_Extension
{
    public class InventoryItemMaint_Extension : PXGraphExtension<InventoryItemMaint>
    {
		//[PXButton(CommitChanges = true)]
		//[PXUIField(DisplayName = "Inventory Summary", MapEnableRights = PXCacheRights.Select)]
		//protected virtual IEnumerable ViewSummary(PXAdapter adapter)
		//{
		//	if (Base.Item.Current != null)
		//	{
		//		InventorySummaryEnq inventorySummaryEnq = PXGraph.CreateInstance<InventorySummaryEnq>();
		//		inventorySummaryEnq.Filter.Current.InventoryID = Base.Item.Current.InventoryID;
		//		inventorySummaryEnq.Filter.Select(Array.Empty<object>());
		//		throw new PXRedirectRequiredException(inventorySummaryEnq, "Inventory Summary")
		//		{
		//			Mode = PXBaseRedirectException.WindowMode.New
		//		};
		//	}
		//	return adapter.Get();
		//}
	}
}
