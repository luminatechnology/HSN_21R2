using HSNHighcareCistomizations.DAC;
using HSNHighcareCistomizations.Entity;
using Newtonsoft.Json;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.FS;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HSNHighcareCistomizations.Descriptor
{
    public class HighcareHelper
    {
        public INItemClass GetItemclass(int? inventoryID)
        {
            return SelectFrom<INItemClass>
                   .InnerJoin<InventoryItem>.On<INItemClass.itemClassID.IsEqual<InventoryItem.itemClassID>>
                   .Where<InventoryItem.inventoryID.IsEqual<P.AsInt>>
                   .View.Select(new PXGraph(), inventoryID).RowCast<INItemClass>().FirstOrDefault();
        }

        public FSEquipment GetEquipmentInfo(int? equipmentID)
        {
            return FSEquipment.PK.Find(new PXGraph(), equipmentID);
        }

        /// <summary> Retrieve the corresponding enabled PIN Code based on the equipmen </summary>
        public IEnumerable<LUMCustomerPINCode> GetEquipmentPINCodeList(int? baccountID, int? equipmentID)
        {
            return SelectFrom<LUMCustomerPINCode>
                            .InnerJoin<FSEquipment>.On<LUMCustomerPINCode.bAccountID.IsEqual<FSEquipment.ownerID>>
                            .InnerJoin<LUMEquipmentPINCode>.On<FSEquipment.SMequipmentID.IsEqual<LUMEquipmentPINCode.sMEquipmentID>
                                                          .And<LUMCustomerPINCode.pin.IsEqual<LUMEquipmentPINCode.pincode>>>
                            .Where<LUMCustomerPINCode.bAccountID.IsEqual<P.AsInt>
                              .And<FSEquipment.SMequipmentID.IsEqual<P.AsInt>>
                              .And<P.AsDateTime.IsBetween<LUMCustomerPINCode.startDate, LUMCustomerPINCode.endDate>>>
                  .OrderBy<Asc<LUMCustomerPINCode.startDate>>
                  .View.Select(new PXGraph(), baccountID, equipmentID, DateTime.Now).RowCast<LUMCustomerPINCode>();
        }

        public (bool success, string errorMessage) CallReturnAPI(HighcareReturnEntity entity)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var preference = PXDatabase.Select<LUMHighcarePreference>().FirstOrDefault();
                    client.DefaultRequestHeaders.Add("Authorization", $"bearer {preference?.SecretKey}");
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, preference?.ReturnUrl);
                    request.Content = new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        return (true, string.Empty);
                    else
                        throw new Exception("Call return api failed");
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            }
        }
    }

    public class HighcareClassAttr : PX.Data.BQL.BqlString.Constant<HighcareClassAttr>
    {
        public HighcareClassAttr() : base("HIGHCARE") { }
    }

    [Serializable]
    public class ServiceScopeFilter : IBqlTable
    {
        #region CPriceClassID
        [PXString(10, IsUnicode = true, InputMask = "")]
        [PXSelector(typeof(PX.Objects.AR.ARPriceClass.priceClassID))]
        [PXUIField(DisplayName = "Price Class ID")]
        public virtual string CPriceClassID { get; set; }
        public abstract class cPriceClassID : PX.Data.BQL.BqlString.Field<cPriceClassID> { }
        #endregion
    }

    [Serializable]
    public class HighcareReturnFilter : IBqlTable
    {

        public const string NewReturn = "New";
        public const string ReleaseReturn = "Release";

        [PXDBString(50, IsUnicode = true, InputMask = "")]
        [PXDefault(HighcareReturnFilter.NewReturn)]
        [PXUIField(DisplayName = "Process type")]
        [PXStringList(new string[] { HighcareReturnFilter.NewReturn, HighcareReturnFilter.ReleaseReturn }, new string[] { "Process New Return", "Process Released Return" })]
        public virtual string ProcessType { get; set; }
        public abstract class processType : PX.Data.BQL.BqlString.Field<processType> { }
    }
}
