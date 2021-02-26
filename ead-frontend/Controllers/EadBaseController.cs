using System.Web.Mvc;
using ead_frontend.Repositories;

namespace ead_frontend.Controllers
{
    public class EadBaseController : Controller
    {
        public EadRepo EadRepo;

        public EadBaseController()
        {
            EadRepo = new EadRepo();
        }
    }
}