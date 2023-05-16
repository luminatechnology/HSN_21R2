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
        public string EOOrderNbr { get; set; }
    }
}
