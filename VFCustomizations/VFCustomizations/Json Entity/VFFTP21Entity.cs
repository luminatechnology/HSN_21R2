using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFCustomizations.Json_Entity.FTP21
{
    public class VFFTP21Entity
    {
        public string JobNo { get; set; }
        public string TerminalID { get; set; }
        public string SerialNo { get; set; }
        public string StartDateTime { get; set; }
        public string FinishDateTime { get; set; }
        public string SetupReason { get; set; }
    }

}
