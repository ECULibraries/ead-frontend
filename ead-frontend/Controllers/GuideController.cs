using System.Web.Mvc;
using ead_frontend.Models;

namespace ead_frontend.Controllers
{
    public class GuideController : Controller
    {
        /// <summary>
        /// Displays EAD XML.
        /// </summary>
        /// <param name="eadid">Guide Identifier (EADID)</param>
        /// <returns></returns>
        public ActionResult Index(string eadid) => View(new GuideModel(eadid, false));

        // matches "guide/{eadid}/request" in RouteConfig.cs
        public ActionResult Pass(string eadid)
        {
            var guide = new GuideModel(eadid);
            return RedirectToAction("Choose", "Request", new {title = guide.Title, identifier = eadid });
        }
    }
}