using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VFCustomizations.Json_Entity.FTP21;
using VFCustomizations.Json_Entity.FTP22;
using VFCustomizations.Json_Entity.FTP3;
using VFCustomizations.Json_Entity.FTP6;

namespace VFCustomizations.Descriptor
{
    public class VFApiHelper
    {
        /// <summary> Get Verifone Access Token </summary>
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

        /// <summary> Call Verifone API FTP 2101 </summary>
        public VFFTPResponseEntity CallFTP21(VFFTP21Entity entity, VFApiTokenEntity accessEntity)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"bearer {accessEntity.access_token}");
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, @"https://som.inisis.co.th:8888/api/ftp/ftp21");
                    //JsonConvert.SerializeObject(model).Dump();
                    request.Content = new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();
                    var result = JsonConvert.DeserializeObject<VFFTPResponseEntity>(response.Content.ReadAsStringAsync().Result);
                    return result;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary> Call Verifone API FTP 2101 </summary>
        public VFFTPResponseEntity CallFTP22(VFFTP22Entity entity, VFApiTokenEntity accessEntity)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"bearer {accessEntity.access_token}");
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, @"https://som.inisis.co.th:8888/api/ftp/ftp22");
                    //JsonConvert.SerializeObject(model).Dump();
                    request.Content = new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();
                    var result = JsonConvert.DeserializeObject<VFFTPResponseEntity>(response.Content.ReadAsStringAsync().Result);
                    return result;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary> Call Verifone API FTP 3001(FTP3) </summary>
        public VFFTPResponseEntity CallFTP3(VFFTP3Entity entity, VFApiTokenEntity accessEntity)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"bearer {accessEntity.access_token}");
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, @"https://som.inisis.co.th:8888/api/ftp/ftp3");
                    request.Content = new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();
                    var result = JsonConvert.DeserializeObject<VFFTPResponseEntity>(response.Content.ReadAsStringAsync().Result);
                    return result;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary> Call Verifone API FTP 6001(FTP3) </summary>
        public VFFTPResponseEntity CallFTP6(VFFTP6Entity entity, VFApiTokenEntity accessEntity)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"bearer {accessEntity.access_token}");
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, @"https://som.inisis.co.th:8888/api/ftp/ftp6");
                    request.Content = new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();
                    var result = JsonConvert.DeserializeObject<VFFTPResponseEntity>(response.Content.ReadAsStringAsync().Result);
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

    public class VFFTPResponseEntity
    {
        public string ResponseCode { get; set; }
        public string ErrorMessage { get; set; }
    }

}
