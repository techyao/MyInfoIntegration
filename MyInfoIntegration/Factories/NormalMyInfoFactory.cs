
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MyInfoIntegration.Factories
{
    public class NormalMyInfoFactory : MyInfoFactory
    {
        public override async Task<string> RetrieveTokenAsync(string code)
        {
            var parameters = ConfigRetrieveTokenParameters(code);
            var httpContent = new FormUrlEncodedContent(parameters);

            using (HttpClient client = CreateHttpClient(string.Empty))
            {
                var httpResponse = await client.PostAsync(GetTokenUri, httpContent);
                var responseString = await httpResponse.Content.ReadAsStringAsync();
                return JObject.Parse(responseString)["access_token"].ToString();
            }
        }

        public override async Task<string> RetrievePersonData(string token, string scope)
        {
            var uinfin = DecodeTokenAndReturnUinfin(token);
            var parameters = GetRetrievePersonParameters(scope.Replace(" ", ","));

            using (HttpClient client = CreateHttpClient("Bearer " + token))
            {
                var httpResponse = await client.GetAsync(GetPersonUri + uinfin + parameters);
                var responseString = await httpResponse.Content.ReadAsStringAsync();
                return SerializePersonInfo(scope, responseString);
            }
        }
    }
}