using System.Web.Mvc;

namespace ead_frontend.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index() => View();
        public ActionResult About() => View();
    }
}