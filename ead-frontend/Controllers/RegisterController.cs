using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ead_frontend.Models;
using ead_frontend.Services;
using X.PagedList;

namespace ead_frontend.Controllers
{
    public class RegisterController : EadBaseController
    {
        public ActionResult Index()
        {
            var vm = new RegisterIndexVm();
            vm.Register = new Register();
            vm.HowFound = new List<SelectListItem>();
            var newItem1 = new SelectListItem { Text = "Bibliographic Citation", Value = "Bibliographic Citation" };
            vm.HowFound.Add(newItem1);
            var newItem2 = new SelectListItem { Text = "Google Search", Value = "Google Search" };
            vm.HowFound.Add(newItem2);
            var newItem3 = new SelectListItem { Text = "Library Website", Value = "Library Website" };
            vm.HowFound.Add(newItem3);
            var newItem4 = new SelectListItem { Text = "Professor", Value = "Professor" };
            vm.HowFound.Add(newItem4);
            var newItem5 = new SelectListItem { Text = "Walk-in", Value = "Walk-in" };
            vm.HowFound.Add(newItem5);
            var newItem6 = new SelectListItem { Text = "Word of Mouth", Value = "Word of Mouth" };
            vm.HowFound.Add(newItem6);
            return View(vm);
        }

        [HttpPost]
        public ActionResult Index(RegisterIndexVm reg)
        {
            if (!ModelState.IsValid)
            {
                reg.HowFound = new List<SelectListItem>();
                var newItem1 = new SelectListItem { Text = "Bibliographic Citation", Value = "Bibliographic Citation" };
                reg.HowFound.Add(newItem1);
                var newItem2 = new SelectListItem { Text = "Google Search", Value = "Google Search" };
                reg.HowFound.Add(newItem2);
                var newItem3 = new SelectListItem { Text = "Library Website", Value = "Library Website" };
                reg.HowFound.Add(newItem3);
                var newItem4 = new SelectListItem { Text = "Professor", Value = "Professor" };
                reg.HowFound.Add(newItem4);
                var newItem5 = new SelectListItem { Text = "Walk-in", Value = "Walk-in" };
                reg.HowFound.Add(newItem5);
                var newItem6 = new SelectListItem { Text = "Word of Mouth", Value = "Word of Mouth" };
                reg.HowFound.Add(newItem6);
                return View(reg);
            }

            reg.Register.registered_on = DateTime.Now;
            reg.Register.last_name = reg.Register.last_name.Trim();
            reg.Register.email = reg.Register.email.Trim().ToLower();
            
            EadRepo.RegisterUser(reg.Register);
            return RedirectToAction("Success");
        }

        public ActionResult Success()
        {
            return View();
        }

        public ActionResult Regulations()
        {
            return View();
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult List(string sortOrder, string searchString, string currentFilter, int? page)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                page = 1;
                TempData["Message"] = $"Showing results for <em><b>{searchString}</b></em>. <a href='{Url.Action("List", "Register")}'>Clear Search</a>";
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            if (sortOrder == null) sortOrder = "registered_on DESC";
            ViewBag.CurrentSort = sortOrder;
            ViewBag.FirstSortParm = sortOrder == "first_name" ? "first_name DESC" : "first_name";
            ViewBag.LastSortParm = sortOrder == "last_name" ? "last_name DESC" : "last_name";
            ViewBag.EmailSortParm = sortOrder == "email" ? "email DESC" : "email";
            ViewBag.DateSortParm = sortOrder == "registered_on" ? "registered_on DESC" : "registered_on";
            

            var pageNumber = (page ?? 1) - 1;
            var list = EadRepo.GetRegisterList(pageNumber, 10, sortOrder, searchString, out var totalCount);

            var vm = new StaticPagedList<Register>(list, pageNumber + 1, 10, totalCount);

            return View(vm);
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult View(int id)
        {
            return View(EadRepo.GetRegisteredUser(id));
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult Delete(int id)
        {
            EadRepo.DeleteRegisteredUser(id);
            TempData["Message"] = "Registrant deleted.";
            return RedirectToAction("List");
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult UpdateEmail(Register vm)
        {
            EadRepo.UpdateRegisterEmail(vm.id, vm.email);
            TempData["Message"] = "Email Address updated.";
            return RedirectToAction("View", new { id = vm.id });
        }
    }
}