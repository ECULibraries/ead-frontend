using System.Web.Mvc;
using ead_frontend.Models;

namespace ead_frontend.Controllers
{
    public class BrowseController : Controller
    {
        public ActionResult Index(string sortOrder, string letter)
        {
            ViewBag.Letter = letter;
            if (sortOrder == null) sortOrder = "title";
            var vm = new BrowseModel(sortOrder);
            return View(vm);
        }
    }
}