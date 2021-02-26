using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace ead_frontend.Services
{
    public class ApiAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] _allowedroles;

        public ApiAuthorizeAttribute(params string[] users)
        {
            _allowedroles = users;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var authorize = false;

            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var apps = identity.Claims.Where(x => x.Type == "Apps").Select(y => y.Value).FirstOrDefault();

            foreach (var role in _allowedroles)
            {
                if (role == "Administrators" && apps.Contains("[544]")) authorize = true;
                if (role == "RequestAdmins" && apps.Contains("[742]")) authorize = true;
            }

            return authorize;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new HttpUnauthorizedResult();
        }
    }
}