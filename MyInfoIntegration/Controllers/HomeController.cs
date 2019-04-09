using MyInfoIntegration.Services.Contract;
using System.Web.Http;
using System.Web.Mvc;

namespace MyInfoIntegration.Controllers
{
    public class HomeController : Controller
    {
        private IMyInfoService myInfoService;
        public HomeController(IMyInfoService _myInfoService)
        {
            myInfoService = _myInfoService;
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Callback()
        {
            if (Request.QueryString.Count == 0 || string.IsNullOrEmpty(Request.QueryString["code"]) || string.IsNullOrEmpty(Request.QueryString["scope"]))
            {
               return RedirectToAction("Index");
            }
            return View();
        }

    }
}