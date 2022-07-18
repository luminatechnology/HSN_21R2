using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace VFCustomizations.Descriptor
{
    public class VFApiHelper
    {
        /// <summary> 取 Access Token </summary>
        public VFApiTokenEntity GetAccessToken()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://som.inisis.co.th/token");
                    requestMessage.Content = new StringContent($"username=hsnapis&password=QzwV8otChu#ZHqIKpRVee&grant_type=password", Encoding.UTF8);
                    requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    HttpResponseMessage response = client.SendAsync(requestMessage).GetAwaiter().GetResult();
                    var result = JsonConvert.DeserializeObject<VFApiTokenEntity>(response.Content.ReadAsStringAsync().Result);
                    return result;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public VFFTP21ResponseEntity CallFTP21(VFFTP21Entity entity, VFApiTokenEntity accessEntity)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"bearer {accessEntity.access_token}");
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, @"https://som.inisis.co.th/api/ftp/ftp21");
                    //JsonConvert.SerializeObject(model).Dump();
                    request.Content = new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();
                    var result = JsonConvert.DeserializeObject<VFFTP21ResponseEntity>(response.Content.ReadAsStringAsync().Result);
                    return result;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }

    public class VFApiTokenEntity
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string userName { get; set; }
        public string fullName { get; set; }
        public string OrganizationName { get; set; }
        public string position { get; set; }
        public string isSysAdmin { get; set; }
        public string userCode { get; set; }

        public string Issued { get; set; }

        public string Expires { get; set; }
    }

    public class VFFTP21Entity
    {
        public string JobNo { get; set; }
        public string TerminalID { get; set; }
        public string SerialNo { get; set; }
        public string StartDateTime { get; set; }
        public string FinishDateTime { get; set; }
        public string SetupReason { get; set; }
    }

    public class VFFTP21ResponseEntity
    {
        public string ResponseCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
