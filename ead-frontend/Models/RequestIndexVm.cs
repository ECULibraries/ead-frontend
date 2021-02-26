using System.Collections.Generic;
using System.Web.Mvc;

namespace ead_frontend.Models
{
    public class RequestIndexVm
    {
        public Request Request { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }
        public IEnumerable<SelectListItem> UseList { get; set; }
    }
}