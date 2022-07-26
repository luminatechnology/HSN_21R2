using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFCustomizations.Json_Entity.FTP3
{
    public class Item
    {
        public string PartNo { get; set; }
        public string SerialNo { get; set; }
        public object PhoneNo { get; set; }
        public int QTY { get; set; }
        public string TLACondition { get; set; }
    }

    public class JobItem
    {
        public string JobNo { get; set; }
        public List<Item> Items { get; set; }
    }

    public class VFFTP3Entity
    {
        public string DeliveryNo { get; set; }
        public string DeliveryDate { get; set; }
        public string ShipToCode { get; set; }
        public string ShipToName { get; set; }
        public string AWBNo { get; set; }
        public string ForwarderName { get; set; }
        public object ShipVia { get; set; }
        public string PackingNo { get; set; }
        public object ETA { get; set; }
        public string ExportDate { get; set; }
        public List<JobItem> JobItems { get; set; }
    }


}
