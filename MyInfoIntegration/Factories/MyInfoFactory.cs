using MyInfoIntegration.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MyInfoIntegration.Factories
{
    public abstract class MyInfoFactory
    {

        #region Abstract Methods
        public abstract Task<string> RetrieveTokenAsync(string code);
        public abstract Task<string> RetrievePersonData(string token, string scope);
        #endregion

        #region Readonly Properties 
        private static readonly string CallbackUrl = ConfigurationManager.AppSettings.Get("MYINFO_APP_REDIRECT_URL");
        private readonly string ClientSecret = ConfigurationManager.AppSettings.Get("MYINFO_APP_CLIENT_SECRET");
        private readonly string SignPublicKeyPath = ConfigurationManager.AppSettings.Get("SIGN_PUBLIC_KEY_PATH");
        protected static readonly string ClientId = ConfigurationManager.AppSettings.Get("MYINFO_APP_CLIENT_ID");

        private static readonly string RedirectUrl = ConfigurationManager.AppSettings.Get("AUTH_LEVEL") == "L0" ? ConfigurationManager.AppSettings.Get("MYINFO_API_AUTHORISE_L0") : ConfigurationManager.AppSettings.Get("MYINFO_API_AUTHORISE_L2");
        protected readonly string GetTokenUri = ConfigurationManager.AppSettings.Get("AUTH_LEVEL") == "L0" ? ConfigurationManager.AppSettings.Get("MYINFO_TOKEN_URI_L0") : ConfigurationManager.AppSettings.Get("MYINFO_TOKEN_URI_L2");
        protected readonly string GetPersonUri = ConfigurationManager.AppSettings.Get("AUTH_LEVEL") == "L0" ? ConfigurationManager.AppSettings.Get("MY_INFO_PERSON_URL_L0") : ConfigurationManager.AppSettings.Get("MY_INFO_PERSON_URL_L2");
        #endregion

        #region Common Static Methods
        public static Uri RetrieveRedirectUrl(string attributes, string purpose, string state)
        {
            return new Uri(string.Format("{0}?client_id={1}&attributes={2}&purpose={3}&state={4}&redirect_uri={5}",
                new object[] { RedirectUrl,
                    ClientId,
                    attributes,
                    purpose,
                    state,
                    CallbackUrl }));

        }

        #endregion

        #region Internal Shared Methods
        protected SortedDictionary<string, string> ConfigRetrieveTokenParameters(string code)
        {
            SortedDictionary<string, string> parameters = new SortedDictionary<string, string>
            {
                {"grant_type", "authorization_code"},
                {"client_id", ClientId},
                {"code", code},
                {"redirect_uri", CallbackUrl},
                {"client_secret", ClientSecret}
            };
            return parameters;
        }

        protected string GetRetrievePersonParameters(string attributes)
        {
            return string.Format("?client_id={0}&attributes={1}", ClientId, attributes);
        }

        protected string DecodeTokenAndReturnUinfin(string token)
        {
            using (StreamReader sr = new StreamReader(SignPublicKeyPath))
            {
                var key = sr.ReadToEnd();
                var keyBytes = Encoding.UTF8.GetBytes(key);
                var decode = JWTDecodeHelper.Decode(token, keyBytes);
                var decodedToken = JObject.Parse(decode);
                return decodedToken["sub"].ToString();
            }
        }

        internal string SerializePersonInfo(string scope, string personInfo)
        {
            var personJson = JObject.Parse(personInfo);

            var result = new Dictionary<string, string>();
            foreach (var field in scope.Split(' '))
            {
                if (personJson.ContainsKey(field))
                {
                    var subJson = JObject.Parse(personJson[field].ToString());
                    if (subJson.ContainsKey("value"))
                    {
                        result.Add(field, subJson["value"].ToString());
                        continue;
                    }

                    result.Add(field, subJson.ToString());
                }
            }

            return JsonConvert.SerializeObject(result);
        }

        #endregion

        public static MyInfoFactory GetInstance(string portal)
        {
            return Activator.CreateInstance(Type.GetType($"MyInfoIntegration.Factories.{portal}MyInfoFactory") ?? throw new InvalidOperationException("Unable to retrieve MyInfo factory!")) as MyInfoFactory;
        }

        internal HttpClient CreateHttpClient(string authString)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

            if (!string.IsNullOrWhiteSpace(authString))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authString);
            }

            return client;
        }
    }
}