using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PowerBiAuth
{
    public class Authentication
    {
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public string ClientSecret { get; private set; }
        public Authentication(string userName, string password)
        {
            UserName = userName;
            Password = password;            
        }

        public Authentication(string clientSecret)
        {
            ClientSecret = clientSecret;
        }

        public async Task<string> AuthenticationContext(string resourceUrl, string clientId, string tenantId)
        {
            try
            {
                //client_secret
                HttpClient client = new HttpClient();
                string tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/token";
                var body = $"resource={resourceUrl}&client_id={clientId}&grant_type=password&username={UserName}&password={Password}";
                var stringContent = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

                var result = await client.PostAsync(tokenEndpoint, stringContent).ContinueWith((response) =>
                {
                    return response.Result.Content.ReadAsStringAsync().Result;
                });

                JObject jobject = JObject.Parse(result);

                return jobject["access_token"].Value<string>();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<string> AuthenticationClientContext(string resourceUrl, string clientId, string tenantId)
        {
            try
            {                                
                HttpClient client = new HttpClient();
                var clientSecret = ClientSecret;                
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("scope", resourceUrl),
                    new KeyValuePair<string, string>("client_secret",clientSecret)                    
                });
                var result = await client.PostAsync($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token", content).ContinueWith((response) =>
                {

                    return response.Result.Content.ReadAsStringAsync().Result;
                });

                JObject jobject = JObject.Parse(result);

                return jobject["access_token"].Value<string>();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
