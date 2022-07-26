using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFCustomizations.Json_Entity.FTP22
{
    public class VFFTP22Entity
    {
        public string JobNo { get; set; }
        public string IncidentCatalogName { get; set; }
        public string CommitDate { get; set; }
        public object PreviousCommitDate { get; set; }
        public string HoldReason { get; set; }
        public string HoldDate { get; set; }
        public string HoldStatus { get; set; }
    }
}
