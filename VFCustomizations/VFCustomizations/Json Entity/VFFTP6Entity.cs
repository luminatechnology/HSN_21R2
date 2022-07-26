using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFCustomizations.Json_Entity.FTP6
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Item
    {
        public string PartNo { get; set; }
        public string SerialNo { get; set; }
        public string PhoneNo { get; set; }
        public int QTYSend { get; set; }
        public int QTYReceive { get; set; }
        public string ReceiveDate { get; set; }
        public int? QTY { get; set; }
    }

    public class JobItem
    {
        public string JobNo { get; set; }
        public List<Item> Items { get; set; }
    }

    public class VFFTP6Entity
    {
        public string DeliveryNo { get; set; }
        public string DeliveryDate { get; set; }
        public string AWBNo { get; set; }
        public string Forwarder { get; set; }
        public object BankShortName { get; set; }
        public string ExportDate { get; set; }
        public string PackingNo { get; set; }
        public string ETDDate { get;set;}
        public string WarehouseLocation { get;set;}
        public string ShipToCode { get;set;}
        public string ShipToName { get;set;}
        public string ShipToAddress { get;set;}
        public string ShipToContact { get;set;}
        public string ShipFromCode { get;set;}
        public string ShipFromName { get;set;}
        public string ShipFromAddress { get;set;}
        public string ShipFromContact { get;set;}
        public List<JobItem> JobItems { get; set; }
    }


}
