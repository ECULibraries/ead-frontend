using System.Linq;
using System.Web;
using System.Web.Mvc;
using ead_frontend.Models;
using HttpUtility = SolrNet.Utils.HttpUtility;

namespace ead_frontend.Controllers
{
    public class SearchController : Controller
    {
        [HttpPost]
        public ActionResult Route(string q, string pg, string[] filter_fields, string[] filter_values, string repo)
        {
            if (repo == "All Repositories")
            {
                // By default, older versions of IE (<=8) will submit form data in Latin-1 encoding if possible. 
                // Iincluding a character that can't be expressed in Latin-1, IE is forced to use UTF-8 encoding.
                return RedirectToAction("Index", new { utf8 = "✓", q, pg, filter_fields, filter_values });
            }

            return RedirectToAction("Index", new { utf8 = "✓", q, pg, filter_fields, filter_values });
        }

        [HttpPost]
        public ActionResult RouteMobile(string qm, string repom)
        {
            if (repom == "All Repositories")
            {
                // By default, older versions of IE (<=8) will submit form data in Latin-1 encoding if possible. 
                // Iincluding a character that can't be expressed in Latin-1, IE is forced to use UTF-8 encoding.
                return RedirectToAction("Index", new { utf8 = "✓", q = HttpUtility.UrlEncode(qm) });
            }

            return RedirectToAction("Index", new { utf8 = "✓", q = HttpUtility.UrlEncode(qm), filter_fields = "repo", filter_values = repom });
        }

        public ActionResult Index(string utf8, string q, string pg, string[] filter_fields, string[] filter_values)
        {
            if (q == null)
            {
                return View();
            }

            ViewBag.FilterFields = filter_fields;
            ViewBag.FilterValues = filter_values;
            
            if(filter_fields != null)
            {
                for (int i = 0; i < filter_fields.Length; i++)
                {
                    ViewBag.FilterQueryString += $"&filter_fields={filter_fields[i]}&filter_values={filter_values[i]}";
                }
            }            

            var vm = new SearchModel(q, pg, filter_fields, filter_values);

            foreach (var repoFacet in vm.ObjectResults.FacetFields.ToList())
            {
                foreach (var x in repoFacet.Value.ToList())
                {
                    if (x.Value == vm.TotalResults)
                    {
                        repoFacet.Value.Remove(x);
                    }
                }
            }

            return View(vm);
        }
    }
}