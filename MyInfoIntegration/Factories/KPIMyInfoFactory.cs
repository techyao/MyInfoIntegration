using Jose;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyInfoIntegration.Factories
{
    public class KPIMyInfoFactory : MyInfoFactory
    {
        private readonly string PrivateKeyPath = ConfigurationManager.AppSettings.Get("PRIVATE_KEY_FOR_MYINFO_PATH");
        private readonly string Realm = ConfigurationManager.AppSettings.Get("MYINFO_APP_REALM");
        public override async Task<string> RetrieveTokenAsync(string code)
        {
            var parameters = ConfigRetrieveTokenParameters(code);
            var signature = GenerateSignature(HttpMethod.Post.ToString(), GetTokenUri, parameters);
            var authString = ConfigRequestAuthorization(parameters, signature);
            var requestParameters = ConfigRetrieveTokenParameters(code);
            var httpContent = new FormUrlEncodedContent(requestParameters);

            using (HttpClient client = CreateHttpClient(authString))
            {
                var httpResponse = await client.PostAsync(GetTokenUri, httpContent);
                var responseString = await httpResponse.Content.ReadAsStringAsync();
                return JObject.Parse(responseString)["access_token"].ToString();
            }
        }

        private string ConfigRequestAuthorization(SortedDictionary<string, string> parameters, string signature, string token = "")
        {
            var authString = string.Format("Apex_l2_eg realm=\"{0}\",apex_l2_eg_timestamp=\"{1}\"," +
                                           "apex_l2_eg_nonce=\"{2}\",apex_l2_eg_app_id=\"{3}\"," +
                                           "apex_l2_eg_signature_method=\"SHA256withRSA\",apex_l2_eg_version=\"1.0\"," +
                                           "apex_l2_eg_signature=\"{4}\"",
                                           Realm, parameters["apex_l2_eg_timestamp"],
                                           parameters["apex_l2_eg_nonce"], ClientId, signature);

            if (!string.IsNullOrWhiteSpace(token))
            {
                authString = authString + ",Bearer " + token;
            }

            return authString;
        }

        private string GenerateSignature(string method, string requestUrl, SortedDictionary<string, string> parameters)
        {
            var url = requestUrl.Replace(".api.gov.sg", ".e.api.gov.sg");

            parameters.Add("apex_l2_eg_app_id", ClientId);
            parameters.Add("apex_l2_eg_nonce", "01" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            parameters.Add("apex_l2_eg_signature_method", "SHA256withRSA");
            parameters.Add("apex_l2_eg_timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
            parameters.Add("apex_l2_eg_version", "1.0");

            string baseString = method.ToUpper() + "&" + url + "&";
            baseString += FormatQueryString(parameters);

            var rsa = CreatePrivateRsaCryptoServiceProvider(PrivateKeyPath);
            var signData = rsa.SignData(Encoding.UTF8.GetBytes(baseString), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signData);
        }

        private static string FormatQueryString(SortedDictionary<string, string> parameters)
        {
            string baseString = string.Empty;
            foreach (var pair in parameters)
            {
                baseString += (pair.Key + "=" + pair.Value + (pair.Key != parameters.Last().Key ? "&" : ""));
            }
            return baseString;
        }

        private static RSACryptoServiceProvider CreatePrivateRsaCryptoServiceProvider(string pathToPrivateKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            using (StreamReader sr = new StreamReader(pathToPrivateKey))
            {
                PemReader pr = new PemReader(sr);
                RSAParameters keyParameters = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)pr.ReadObject());

                rsa.ImportParameters(keyParameters);
            }

            return rsa;
        }

        public async override Task<string> RetrievePersonData(string token, string scope)
        {
            var uinfin = DecodeTokenAndReturnUinfin(token);
            var attributes = scope.Replace(" ", ",");
            var requestUrl = GetPersonUri + uinfin + "/";

            var parameters = new SortedDictionary<string, string>
            {
                {"client_id", ClientId},
                { "attributes", attributes}
            };

            var signature = GenerateSignature(HttpMethod.Get.ToString(), requestUrl, parameters);
            var authString = ConfigRequestAuthorization(parameters, signature, token);

            using (HttpClient client = CreateHttpClient(authString))
            {
                HttpResponseMessage httpResponse = await client.GetAsync(requestUrl + GetRetrievePersonParameters(attributes));
                var responseString = await httpResponse.Content.ReadAsStringAsync();

                var rsa = CreatePrivateRsaCryptoServiceProvider(PrivateKeyPath);
                var result = Jose.JWT.Decode(responseString, rsa, JweAlgorithm.RSA_OAEP, JweEncryption.A256GCM);

                return SerializePersonInfo(scope, result);
            }
        }

    }
}