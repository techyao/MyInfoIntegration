using System;
using System.Net;
using MyInfoIntegration.Services.Contract;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using MyInfoIntegration.Models;
using Newtonsoft.Json.Linq;

namespace MyInfoIntegration.Controllers
{
    public class MyInfoCallbackController : ApiController
    {
        private IMyInfoService myInfoService;
        public MyInfoCallbackController(IMyInfoService _myInfoService)
        {
            myInfoService = _myInfoService;
        }

        [HttpGet, Route("api/myinfo/callback")]
        public async Task<HttpResponseMessage> MyInfoCallbackAsync(string code, string scope)
        {
            try
            {
                var person = await myInfoService.GetPersonData(code, scope);
                var content = Regex.Replace(person, @"\t|\n|\r", string.Empty);
                return Request.CreateResponse(HttpStatusCode.OK,
                    new ResponseData(JObject.Parse(content)));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed,
                    new ResponseData(ex, ex.Message));
            }
        }


        [HttpGet, Route("api/myinfo/redirecturi")]
        public HttpResponseMessage GetRedirectUri(string attributes, string purpose)
        {
            try
            {
                //setup state
                var random = new Random();
                string state = random.Next() + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                return Request.CreateResponse(HttpStatusCode.OK,
                    new ResponseData(myInfoService.GetRedirectUri(attributes, purpose, state)));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed,
                    new ResponseData(ex, ex.Message));
            }
           
        }
    }
}
