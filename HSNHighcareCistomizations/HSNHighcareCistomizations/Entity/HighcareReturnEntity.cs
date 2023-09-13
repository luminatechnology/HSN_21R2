using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNHighcareCistomizations.Entity
{
    public class HighcareReturnEntity
    {
        public string Status { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public decimal? Amount { get; set; }
        public string ECOrderNumber { get; set; }
        public string OrderNumber { get; set; }
        public List<Details> Details { get; set; }
    }

    public class Details
    {
        public int? InventoryID { get;set;}

        public string InventoryCD { get; set; }

        public decimal? Quantity { get; set; }
    }
}
