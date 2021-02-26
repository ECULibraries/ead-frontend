using System.Configuration;
using System.Web.Mvc;

namespace ead_frontend.Controllers
{
    public class LoginController : Controller
    {
        /// <summary>
        /// Redirects to central authentication.
        /// </summary>
        public ActionResult Index() => Redirect(ConfigurationManager.AppSettings["LoginUrl"] + Request.QueryString["ReturnUrl"]);
    }
}