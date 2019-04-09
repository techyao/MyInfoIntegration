using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MyInfoIntegration.Services.Contract
{
    public interface IMyInfoService
    {
        Uri GetRedirectUri(string attributes, string purpose, string state);
        Task<string> GetPersonData(string code, string scope);
    }
}