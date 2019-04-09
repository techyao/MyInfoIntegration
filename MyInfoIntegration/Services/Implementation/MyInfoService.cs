using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MyInfoIntegration.Factories;
using MyInfoIntegration.Services.Contract;

namespace MyInfoIntegration.Services.Implementation
{
    public class MyInfoService : IMyInfoService
    {
        public Uri GetRedirectUri(string attributes, string purpose, string state)
        {
            return MyInfoFactory.RetrieveRedirectUrl(attributes, purpose, state);
        }

        public async Task<string> GetPersonData(string code, string scope)
        {
            var portal = ConfigurationManager.AppSettings.Get("AUTH_LEVEL") == "L0" ? "Normal" : "KPI";
            var my_info_factory = MyInfoFactory.GetInstance(portal);
            var token = await my_info_factory.RetrieveTokenAsync(code);
            return await my_info_factory.RetrievePersonData(token, scope);
        }
    }
}