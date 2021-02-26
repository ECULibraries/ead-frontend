using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Web.Mvc;
using ead_frontend.Models;
using ead_frontend.Repositories;
using ead_frontend.Services;
using X.PagedList;

namespace ead_frontend.Controllers
{
    public class RequestController : EadBaseController
    {
        public ActionResult Choose()
        {
            return View();
        }

        [Authorize]
        public ActionResult Affiliate()
        {
            if (Request.QueryString["location"] == null)
            {
                return RedirectToAction("Select",
                    new { title = Request.QueryString["title"], identifier = Request.QueryString["identifier"] });
            }
            return RedirectToAction("Submit",
                new { title = Request.QueryString["title"], identifier = Request.QueryString["identifier"], location = Request.QueryString["location"] });
        }

        public ActionResult Select()
        {
            var vm = new GuideModel(Request.QueryString["identifier"], true);
            if(vm.ContainerList.Count == 0 || (vm.ContainerList.Count == 1 && vm.ContainerList[0].UnitTitleDate == "Digitized Material"))
            {
                return RedirectToAction("Submit", "Request", new { title = Request.QueryString["title"], identifier = Request.QueryString["identifier"] });
            }
            return View(vm);
        }

        [HttpPost]
        public ActionResult Process()
        {
            var cuids = new List<string>();
            foreach (string key in Request.Form)
            {
                if (key.StartsWith("chk_"))
                {
                    var noCheck = key.Replace("chk_", "");
                    if (!cuids.Contains(noCheck)) cuids.Add(noCheck);
                }

            }
            return RedirectToAction("Submit", "Request", new { title = Request.Form["title"], identifier = Request.Form["identifier"], cuids = string.Join(",", cuids) });
        }

        public ActionResult Submit()
        {
            var vm = new RequestIndexVm();
            vm.Request = new Request();
            vm.StatusList = EadRepo.GetStatusList().Select(x => new SelectListItem
            {
                Value = x.id.ToString(),
                Text = x.status
            });

            vm.UseList = EadRepo.GetUseList().Select(x => new SelectListItem
            {
                Value = x.id.ToString(),
                Text = x.value
            });

            vm.Request.title = Request.QueryString["title"].Length > 250 ? Request.QueryString["title"].Substring(0, 245) + "..." : Request.QueryString["title"];

            if (Request.QueryString["location"] == null)
            {
                vm.Request.identifier = Request.QueryString["identifier"].TrimEnd('e').ToUpper();
                vm.Request.component_ids = Request.QueryString["cuids"];

                if (vm.Request.component_ids != null)
                {
                    var boxes = new List<string>();
                    foreach (var cuid in vm.Request.component_ids.Split(','))
                    {
                        if (cuid.StartsWith("CD") || cuid.StartsWith("LL") || cuid.StartsWith("UA"))
                        {
                            var boxSplit = cuid.Split('_');
                            ViewBag.Boxes += boxSplit[1] + ", ";
                            if (!boxes.Contains(boxSplit[1])) boxes.Add(boxSplit[1]);

                        }
                        else
                        {
                            var split = cuid.Split('-');
                            foreach (var part in split)
                            {
                                if (part.StartsWith("b"))
                                {
                                    if (!boxes.Contains(part.TrimStart('b'))) boxes.Add(part.TrimStart('b'));
                                }
                            }
                        }
                    }
                    ViewBag.Boxes = string.Join(", ", boxes);
                }
                
            }
            else if (Request.QueryString["location"] == "accession")
            {
                vm.Request.identifier = Request.QueryString["identifier"];
            }
            else
            {
                var catalog = new Catalog();
                var match = catalog.Locations.Find(x => x[0] == Request.QueryString["location"]);
                var location = match == null ? Request.QueryString["location"] : match[1];
                vm.Request.identifier = $"{Request.QueryString["identifier"]} ({location})";
            }

            if (Request.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)User.Identity;
                vm.Request.first_name = identity.Claims.Where(x => x.Type == ClaimTypes.GivenName).Select(y => y.Value).FirstOrDefault();
                vm.Request.last_name = identity.Claims.Where(x => x.Type == ClaimTypes.Surname).Select(y => y.Value).FirstOrDefault();
                vm.Request.email = identity.Claims.Where(x => x.Type == ClaimTypes.Email).Select(y => y.Value).FirstOrDefault().ToLower();
                vm.Request.phone = identity.Claims.Where(x => x.Type == ClaimTypes.OtherPhone).Select(y => y.Value).FirstOrDefault();
            }

            return View(vm);
        }

        [HttpPost]
        public ActionResult Submit(Request request)
        {
            request.submitted_on = DateTime.Now;
            request.email = request.email.Trim().ToLower();
            request.last_name = request.last_name.Trim();
            if (request.use_id == 0)
            {
                request.use_id = 1; // un-authenticated users will have use_id of 0
                //request.visit_date = null;
            }

            if (request.identifier.StartsWith("CD")) request.repo_id = 4;
            else if (request.identifier.StartsWith("LL")) request.repo_id = 3;
            else if (request.identifier.StartsWith("UA")) request.repo_id = 2;
            else if (request.identifier.Contains("(")) request.repo_id = 5;
            else if (request.identifier.StartsWith("[")) request.repo_id = 6;
            else request.repo_id = 1;

            EadRepo.InsertRequest(request);

            if (request.repo_id == 3 || request.repo_id == 4)
            {
                if (!Request.IsLocal && request.email.ToLower() != "test@ecu.edu")
                {
                    var userMessage = new MailMessage();
                    userMessage.To.Add(new MailAddress(request.email));
                    userMessage.CC.Add(new MailAddress("hslhistmed@ecu.edu"));
                    userMessage.Bcc.Add(new MailAddress("reecem@ecu.edu"));
                    userMessage.Subject = "We're working on your request. Here's what to expect next...";
                    userMessage.Body += "Thank you for your interest in viewing material in the History Collections Reading Room at Laupus Library. We have received your request and look forward to your arrival.<br/><br/>";
                    userMessage.Body += "Researchers must complete the History Collections Researcher Registration Form once a year and sign the Form at each visit. History Collections staff reserves the right to verify your ID at any time, so please bring a government-issued ID (includes ECU OneCard) to each visit.<br/><br/>";
                    userMessage.Body += "Once you've registered, we will go over the Reading Room Guidelines and pull the requested material for you.<br/><br/>";
                    userMessage.Body += "Laupus History Collections Reading Room will be available by appointment only. Contact us at <a href='mailto:hslhistmed@ecu.edu'>hslhistmed@ecu.edu</a> to set up an appointment.";
                    userMessage.Body += "Our reading room is on the fourth floor of Laupus Library. Visit <a href='https://hsl.ecu.edu/about/directions/'>Laupus Library's Directions and Parking page</a> for more information on how to get to and park at the library.<br/><br/>";
                    userMessage.Body += "Thanks again for giving us the opportunity to serve you. If you have any questions, or require additional assistance, please do not hesitate to contact us at <a href='mailto:hslhistmed@ecu.edu'>hslhistmed@ecu.edu</a>.<br/><br/>";
                    userMessage.IsBodyHtml = true;
                    var client = new SmtpClient();
                    client.Send(userMessage);
                }
                return RedirectToAction("Success", new { library = "laupus"});
            }

            if (!Request.IsLocal && request.email.ToLower() != "test@ecu.edu")
            {
                var userMessage = new MailMessage();
                userMessage.To.Add(new MailAddress(request.email));
                userMessage.CC.Add(new MailAddress("specialcollections@ecu.edu"));
                userMessage.Bcc.Add(new MailAddress("reecem@ecu.edu"));
                userMessage.Subject = "We're working on your request. Here's what to expect next...";
                userMessage.Body += "Thank you for your interest in viewing material in the Reading Room at Joyner Library. We've received your request and look forward to your arrival.<br/><br/>";
                userMessage.Body += "The first time you visit the Research Room, we'll ask to see photo identification such as a driver's license or student ID. You may fill out a registration form onsite or ahead of time at <a href='http://digital.lib.ecu.edu/special/ead/register/'>http://digital.lib.ecu.edu/special/ead/register/</a><br/><br/>";
                userMessage.Body += "Once you've registered, we will go over the Reading Room Regulations and pull the requested material for you. Please note that, due to limited storage space, we cannot pull items prior to your arrival.<br/><br/>";
                //userMessage.Body += "Our hours of operation are generally Monday - Thursday, 9am - 7pm; Friday 9am - 5pm; Saturday and Sunday, 1pm - 5pm. Holidays and university breaks may affect these hours. <a href='https://lib.ecu.edu/hours/joyner/departments/spcl'>Click here</a> for an up-to-date operating schedule or call (252) 328-4285.<br/><br/>";
                userMessage.Body += "Due to the coronavirus, the Research Room for Special Collections is available by appointment only. There will only be two researchers allowed at a time from Monday-Friday 1-5. It will be closed on Saturday. <a href='https://outlook.office365.com/owa/calendar/ECUSpecialCollections@studentsecuedu66932.onmicrosoft.com/bookings/'>Book your appointment here</a>.<br/><br/>";
                userMessage.Body += "Our reading room is on the third floor of Joyner Library. Visit <a href='https://library.ecu.edu/about/directions/'>Joyner Library's Maps and Directions page</a> for more information on how to get to and park at the library.<br/><br/>";
                userMessage.Body += "Thanks again for giving us the opportunity to serve you. If you have any questions, or require additional assistance, please do not hesitate to contact us at <a href='mailto:specialcollections@ecu.edu'>specialcollections@ecu.edu</a> or by calling (252) 328-6601.<br/><br/>";
                userMessage.IsBodyHtml = true;
                var client = new SmtpClient();
                client.Send(userMessage);
            }
            return RedirectToAction("Success", new { library = "joyner" });
        }

        public ActionResult Success()
        {
            ViewBag.Library = Request.QueryString["library"];
            return View();
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult Index(string sortOrder, string searchString, string currentFilter, int? page, string library)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                if (searchString.StartsWith("^"))
                {
                    searchString = searchString.TrimStart('^');
                    page = page ?? 1;
                }
                else page = 1;

                TempData["Message"] = $"Showing results for <em><b>{searchString}</b></em>. <a href='{Url.Action("Index", "Request")}'>Clear Search</a>";
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            if (sortOrder == null) sortOrder = "submitted_on DESC";
            ViewBag.CurrentSort = sortOrder;
            ViewBag.Library = library;
            ViewBag.RidSortParm = sortOrder == "id" ? "id DESC" : "id";
            ViewBag.StatusSortParm = sortOrder == "status" ? "status DESC" : "status";
            ViewBag.EmailSortParm = sortOrder == "email" ? "email DESC" : "email";
            ViewBag.TitleSortParm = sortOrder == "title" ? "title DESC" : "title";
            ViewBag.DateSortParm = sortOrder == "submitted_on" ? "submitted_on DESC" : "submitted_on";
            ViewBag.IdSortParm = sortOrder == "identifier" ? "identifier DESC" : "identifier";

            var pageNumber = (page ?? 1) - 1;
            var requests = EadRepo.GetRequestList(pageNumber, 25, sortOrder, searchString, library, out var totalCount);

            var vm = new RequestListVm();
            vm.Requests = new StaticPagedList<Request>(requests, pageNumber + 1, 25, totalCount);

            ViewBag.StartCount = totalCount == 0 ? 0 : (25 * pageNumber + 1);
            ViewBag.EndCount = (25 * pageNumber + 25) > totalCount ? totalCount : (25 * pageNumber + 25);
            ViewBag.TotalCount = totalCount;

            return View(vm);
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult Reshelved(string sortOrder, string searchString, string currentFilter, int? page, string library)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                page = 1;
                TempData["Message"] = $"Showing results for <em><b>{searchString}</b></em>. <a href='{Url.Action("Reshelved", "Request")}'>Clear Search</a>";
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            if (sortOrder == null) sortOrder = "completed_on DESC";
            ViewBag.CurrentSort = sortOrder;
            ViewBag.Library = library;
            ViewBag.RidSortParm = sortOrder == "id" ? "id DESC" : "id";
            ViewBag.EmailSortParm = sortOrder == "email" ? "email DESC" : "email";
            ViewBag.TitleSortParm = sortOrder == "title" ? "title DESC" : "title";
            ViewBag.DateSortParm = sortOrder == "completed_on" ? "completed_on DESC" : "completed_on";
            ViewBag.IdSortParm = sortOrder == "identifier" ? "identifier DESC" : "identifier";

            var pageNumber = (page ?? 1) - 1;
            var requests = EadRepo.GetReshelvedRequestList(pageNumber, 25, sortOrder, searchString, library, out var totalCount);

            var vm = new RequestListVm();
            vm.Requests = new StaticPagedList<Request>(requests, pageNumber + 1, 25, totalCount);

            ViewBag.StartCount = totalCount == 0 ? 0 : (25 * pageNumber + 1);
            ViewBag.EndCount = (25 * pageNumber + 25) > totalCount ? totalCount : (25 * pageNumber + 25);
            ViewBag.TotalCount = totalCount;

            return View(vm);
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult View(int id)
        {
            var vm = EadRepo.GetRequestViewWithEvents(id);

            vm.PreUseLocations = EadRepo.GetLocations().FindAll(x => x.pre_use).Select(x => new SelectListItem
            {
                Value = x.id.ToString(),
                Text = x.location
            });
            vm.InUseLocations = EadRepo.GetLocations().FindAll(x => x.id != 103).Select(x => new SelectListItem
            {
                Value = x.id.ToString(),
                Text = x.location
            });

            vm.StatusList = EadRepo.GetStatusList();
            vm.UseList = EadRepo.GetUseList();

            vm.SimilarRequests = EadRepo.GetSimilarIdentifierRequests(vm.Request.id, vm.Request.identifier);
            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult SaveInactive(RequestViewVm vm)
        {
            if (vm.InactiveItems.All(x => x.selected != true))
            {
                TempData["Message"] = "No components selected.";
                return RedirectToAction("View", "Request", new { id = vm.Request.id });
            }

            var selectedItems = vm.InactiveItems.FindAll(x => x.selected);
            foreach (var item in selectedItems)
            {
                var evt = new Event();
                evt.item_id = item.id;
                evt.location_id = vm.SelectedPreUseLocationId;
                evt.date_time = DateTime.Now;
                evt.pirate_id = User.Identity.Name.ToLower();
                evt.note = vm.PreUseLocationNote;
                EadRepo.CreateEvent(evt);

                item.location_id = vm.SelectedPreUseLocationId;
                EadRepo.UpdateItemLocation(item);
            }

            if (EadRepo.GetUnReshelvedCount(vm.Request.id) == 0)
            {
                EadRepo.CompleteRequest(vm.Request.id);
                TempData["Message"] = "All components are cleared and the request is completed.";
                return RedirectToAction("Reshelved", "Request");
            }

            if (vm.SelectedPreUseLocationId == 12) // not used
            {
                TempData["Message"] = "Components updated.";
                return RedirectToAction("View", "Request", new { id = vm.Request.id });
            }
            return RedirectToAction("Print", "Request", new { rid = vm.Request.id, itemIds = string.Join(",", selectedItems.Select(x => x.id)) });
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult SaveActive(RequestViewVm vm)
        {
            if (vm.ActiveItems.All(x => x.selected != true))
            {
                TempData["Message"] = "No components selected.";
                return RedirectToAction("View", "Request", new { id = vm.Request.id });
            }

            var selectedItems = vm.ActiveItems.FindAll(x => x.selected);
            foreach (var item in selectedItems)
            {
                var evt = new Event();
                evt.item_id = item.id;
                evt.location_id = vm.SelectedInUseLocationId;
                evt.date_time = DateTime.Now;
                evt.pirate_id = User.Identity.Name.ToLower();
                evt.note = vm.InUseLocationNote;
                EadRepo.CreateEvent(evt);

                item.location_id = vm.SelectedInUseLocationId;
                EadRepo.UpdateItemLocation(item);
            }

            if (EadRepo.GetUnReshelvedCount(vm.Request.id) == 0)
            {
                EadRepo.CompleteRequest(vm.Request.id);
                TempData["Message"] = "All components are cleared and the request is completed.";
                return RedirectToAction("Reshelved", "Request");
            }

            TempData["Message"] = "Components Updated.";
            return RedirectToAction("View", "Request", new { id = vm.Request.id });
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult AddBox(int rid, string boxes)
        {
            EadRepo.AddBoxes(rid, boxes.Trim().TrimEnd(','));
            return RedirectToAction("View", "Request", new { id = rid });
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult Print(int rid, string itemIds)
        {
            var vm = new RequestPrintVm();
            vm.Request = EadRepo.GetRequestById(rid);
            vm.Items = EadRepo.GetItemsIn(itemIds);
            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult Complete(int request_id, int item_id, string item_code, string user_code)
        {
            if (item_code != user_code)
            {
                TempData["Message"] = "Reshelve code is not correct";
                return RedirectToAction("View", "Request", new { id = request_id });
            }

            var evt = new Event();
            evt.item_id = item_id;
            evt.location_id = 103;
            evt.date_time = DateTime.Now;
            evt.pirate_id = User.Identity.Name.ToLower();
            EadRepo.CreateEvent(evt);

            var itm = new Item {id = item_id, location_id = 103};
            EadRepo.UpdateItemLocation(itm);

            if (EadRepo.GetUnReshelvedCount(request_id) == 0)
            {
                EadRepo.CompleteRequest(request_id);
                TempData["Message"] = "All components are cleared and the request is completed.";
                return RedirectToAction("Reshelved", "Request");
            }

            return RedirectToAction("View", "Request", new { id = request_id });
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult Reopen(int id)
        {
            EadRepo.ReOpenRequest(id);
            TempData["Message"] = "Request Re-opened.";
            return RedirectToAction("View", new { id } );
        }
        
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult Delete(int id)
        {
            EadRepo.DeleteRequest(id);
            TempData["Message"] = "Request Deleted.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult UpdateVisitDate(RequestViewVm vm)
        {
            EadRepo.UpdateVisitDate(vm.Request.id, vm.Request.visit_date.Value);
            TempData["Message"] = "Date of Visit updated.";
            return RedirectToAction("View", new { id = vm.Request.id });
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult UpdateEmail(RequestViewVm vm)
        {
            EadRepo.UpdateEmail(vm.Request.id, vm.Request.email);
            TempData["Message"] = "Email Address updated.";
            return RedirectToAction("View", new { id = vm.Request.id });
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult Accession()
        {
            var AsRepo = new ArchivesSpaceRepo();
            var vm = AsRepo.GetUnassignedUaAccessions();
            vm.ForEach(x => x.identifier = x.identifier.Replace(",null", ""));
            return View(vm);
        }

    }
}