using System.Collections.Generic;
using System.Web.Mvc;

namespace ead_frontend.Models
{
    public class RegisterIndexVm
    {
        public Register Register { get; set; }
        public List<SelectListItem> HowFound { get; set; }
    }
}