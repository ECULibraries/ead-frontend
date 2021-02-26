using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ead_frontend.Models;
using ead_frontend.Services;
using X.PagedList;

namespace ead_frontend.Controllers
{
    public class ReportController : EadBaseController
    {
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult Index() => View();


        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult Status()
        {
            ViewBag.Collection = "All Repositories";
            return View();
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult Status(string CollectionList, string start, string end)
        {
            ViewBag.Collection = CollectionList;
            
            if (start != null && end != string.Empty)
            {
                var startDate = DateTime.Parse(start);
                var endDate = DateTime.Parse(end);

                if (startDate < endDate)
                {
                    ViewBag.Start = startDate.ToShortDateString();
                    ViewBag.End = endDate.ToShortDateString();
                }
            }
            return View();
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public JsonResult GetStatusChart(string repo, string start, string end)
        {
            var data = EadRepo.GetStatusChart(repo, start, end);

            List<object> iData = new List<object>();
            //Creating sample data  
            DataTable dt = new DataTable();
            dt.Columns.Add("Status", System.Type.GetType("System.String"));
            dt.Columns.Add("RequestCount", System.Type.GetType("System.Int32"));

            foreach (var request in data)
            {
                DataRow dr = dt.NewRow();
                dr["Status"] = request.status;
                dr["RequestCount"] = request.request_count + request.reopened_count;
                dt.Rows.Add(dr);
            }

            //Looping and extracting each DataColumn to List<Object>  
            foreach (DataColumn dc in dt.Columns)
            {
                List<object> x = new List<object>();
                x = (from DataRow drr in dt.Rows select drr[dc.ColumnName]).ToList();
                iData.Add(x);
            }
            //Source data returned as JSON  
            return Json(iData, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult ActiveLocations()
        {
            ViewBag.Collection = "All Repositories";
            return View();
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult ActiveLocations(string CollectionList)
        {
            ViewBag.Collection = CollectionList;
            return View();
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public JsonResult GetActiveLocationsChart(string repo)
        {
            var data = EadRepo.GetUseLocationChart(repo);

            List<object> iData = new List<object>();
            //Creating sample data  
            DataTable dt = new DataTable();
            dt.Columns.Add("Location", System.Type.GetType("System.String"));
            dt.Columns.Add("Count", System.Type.GetType("System.Int32"));

            foreach (var request in data)
            {
                DataRow dr = dt.NewRow();
                dr["Location"] = request.status;
                dr["Count"] = request.request_count;
                dt.Rows.Add(dr);
            }

            //Looping and extracting each DataColumn to List<Object>  
            foreach (DataColumn dc in dt.Columns)
            {
                List<object> x = new List<object>();
                x = (from DataRow drr in dt.Rows select drr[dc.ColumnName]).ToList();
                iData.Add(x);
            }
            //Source data returned as JSON  
            return Json(iData, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult HowFound()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public JsonResult GetHowFoundChart()
        {
            var data = EadRepo.GetHowFoundChart();

            List<object> iData = new List<object>();
            //Creating sample data  
            DataTable dt = new DataTable();
            dt.Columns.Add("Location", System.Type.GetType("System.String"));
            dt.Columns.Add("Count", System.Type.GetType("System.Int32"));

            foreach (var request in data)
            {
                DataRow dr = dt.NewRow();
                dr["Location"] = request.status;
                dr["Count"] = request.request_count;
                dt.Rows.Add(dr);
            }

            //Looping and extracting each DataColumn to List<Object>  
            foreach (DataColumn dc in dt.Columns)
            {
                List<object> x = new List<object>();
                x = (from DataRow drr in dt.Rows select drr[dc.ColumnName]).ToList();
                iData.Add(x);
            }
            //Source data returned as JSON  
            return Json(iData, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult UseType()
        {
            ViewBag.Collection = "All Repositories";
            return View();
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult UseType(string CollectionList, string start, string end)
        {
            ViewBag.Collection = CollectionList;

            if (start != null && end != string.Empty)
            {
                var startDate = DateTime.Parse(start);
                var endDate = DateTime.Parse(end);

                if (startDate < endDate)
                {
                    ViewBag.Start = startDate.ToShortDateString();
                    ViewBag.End = endDate.ToShortDateString();
                }
            }
            return View();
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public JsonResult GetUseTypeChart(string repo, string start, string end)
        {
            var data = EadRepo.GetUseChart(repo, start, end);

            List<object> iData = new List<object>();
            //Creating sample data  
            DataTable dt = new DataTable();
            dt.Columns.Add("Status", System.Type.GetType("System.String"));
            dt.Columns.Add("RequestCount", System.Type.GetType("System.Int32"));

            foreach (var request in data)
            {
                DataRow dr = dt.NewRow();
                dr["Status"] = request.status;
                dr["RequestCount"] = request.request_count + request.reopened_count;
                dt.Rows.Add(dr);
            }

            //Looping and extracting each DataColumn to List<Object>  
            foreach (DataColumn dc in dt.Columns)
            {
                List<object> x = new List<object>();
                x = (from DataRow drr in dt.Rows select drr[dc.ColumnName]).ToList();
                iData.Add(x);
            }
            //Source data returned as JSON  
            return Json(iData, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult Year()
        {
            ViewBag.Collection = "All Repositories";
            ViewBag.StartYear = DateTime.Now.Year.ToString();
            ViewBag.StartMonth = 1;
            ViewBag.EndYear = DateTime.Now.Year.ToString();
            ViewBag.EndMonth = 12;
            return View();
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult Year(string CollectionList, string year, string year2)
        {
            ViewBag.Collection = CollectionList;
            var startSplit = year.Split('/');
            var endSplit = year2.Split('/');
            ViewBag.StartYear = startSplit[1];
            ViewBag.StartMonth = startSplit[0];
            ViewBag.EndYear = endSplit[1];
            ViewBag.EndMonth = endSplit[0];
            return View();
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public JsonResult GetYearChart(string repo, int startyear, int startmonth, int endyear, int endmonth)
        {
            var data = EadRepo.GetYearChart(repo, startyear, startmonth, endyear, endmonth);

            List<object> iData = new List<object>();
            //Creating sample data  
            DataTable dt = new DataTable();
            dt.Columns.Add("Month", System.Type.GetType("System.String"));
            dt.Columns.Add("SubmittedCount", System.Type.GetType("System.Int32"));
            dt.Columns.Add("CompletedCount", System.Type.GetType("System.Int32"));

            foreach (var request in data)
            {
                DataRow dr = dt.NewRow();
                dr["Month"] = request.month;
                dr["SubmittedCount"] = request.submitted_count;
                dr["CompletedCount"] = request.completed_count;
                dt.Rows.Add(dr);
            }

            //Looping and extracting each DataColumn to List<Object>  
            foreach (DataColumn dc in dt.Columns)
            {
                List<object> x = new List<object>();
                x = (from DataRow drr in dt.Rows select drr[dc.ColumnName]).ToList();
                iData.Add(x);
            }
            //Source data returned as JSON  
            return Json(iData, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult ItemCounts()
        {
            ViewBag.Collection = "All Repositories";
            ViewBag.StartYear = DateTime.Now.Year.ToString();
            ViewBag.StartMonth = 1;
            ViewBag.EndYear = DateTime.Now.Year.ToString();
            ViewBag.EndMonth = 12;
            return View();
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult ItemCounts(string CollectionList, string year, string year2)
        {
            ViewBag.Collection = CollectionList;
            var startSplit = year.Split('/');
            var endSplit = year2.Split('/');
            ViewBag.StartYear = startSplit[1];
            ViewBag.StartMonth = startSplit[0];
            ViewBag.EndYear = endSplit[1];
            ViewBag.EndMonth = endSplit[0];
            return View();
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public JsonResult GetItemCountsChart(string repo, int startyear, int startmonth, int endyear, int endmonth)
        {
            var data = EadRepo.GetItemYearChart(repo, startyear, startmonth, endyear, endmonth);

            List<object> iData = new List<object>();
            //Creating sample data  
            DataTable dt = new DataTable();
            dt.Columns.Add("Month", System.Type.GetType("System.String"));
            dt.Columns.Add("SubmittedCount", System.Type.GetType("System.Int32"));
            dt.Columns.Add("CompletedCount", System.Type.GetType("System.Int32"));

            foreach (var request in data)
            {
                DataRow dr = dt.NewRow();
                dr["Month"] = request.month;
                dr["SubmittedCount"] = request.submitted_count;
                dr["CompletedCount"] = request.completed_count;
                dt.Rows.Add(dr);
            }

            //Looping and extracting each DataColumn to List<Object>  
            foreach (DataColumn dc in dt.Columns)
            {
                List<object> x = new List<object>();
                x = (from DataRow drr in dt.Rows select drr[dc.ColumnName]).ToList();
                iData.Add(x);
            }
            //Source data returned as JSON  
            return Json(iData, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult RepoReport()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public JsonResult GetRepoChart()
        {
            var data = EadRepo.GetRepoChart();

            List<object> iData = new List<object>();
            //Creating sample data  
            DataTable dt = new DataTable();
            dt.Columns.Add("Repository", System.Type.GetType("System.String"));
            dt.Columns.Add("RequestCount", System.Type.GetType("System.Int32"));

            foreach (var request in data)
            {
                DataRow dr = dt.NewRow();
                dr["Repository"] = request.status;
                dr["RequestCount"] = request.request_count + request.reopened_count;
                dt.Rows.Add(dr);
            }

            //Looping and extracting each DataColumn to List<Object>  
            foreach (DataColumn dc in dt.Columns)
            {
                List<object> x = new List<object>();
                x = (from DataRow drr in dt.Rows select drr[dc.ColumnName]).ToList();
                iData.Add(x);
            }
            //Source data returned as JSON  
            return Json(iData, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult CollectionRequests(string sortOrder, int? page, string repo, string start, string end)
        {
            var repoStr = repo ?? "All Repositories";
            ViewBag.Collection = repoStr;
            ViewBag.Start = start;
            ViewBag.End = end;

            if (sortOrder == null) sortOrder = "title";
            ViewBag.CurrentSort = sortOrder;
            ViewBag.IdSortParm = sortOrder == "identifier" ? "identifier DESC" : "identifier";
            ViewBag.TitleSortParm = sortOrder == "title" ? "title DESC" : "title";
            ViewBag.CountSortParm = sortOrder == "count" ? "count DESC" : "count";

            var pageNumber = (page ?? 1) - 1;
            var requests = EadRepo.GetCollectionRequests(pageNumber, 10, sortOrder, repoStr, start, end, out var totalCount);

            var vm = new StaticPagedList<CollectionRequest>(requests, pageNumber + 1, 10, totalCount);
            
            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult CollectionRequests(string CollectionList, string start, string end)
        {
            return RedirectToAction("CollectionRequests", new {repo = CollectionList, start, end});
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult NotShelved(string sortOrder, int? page, string repo)
        {
            var repoStr = repo ?? "All Repositories";
            ViewBag.Collection = repoStr;

            if (sortOrder == null) sortOrder = "title";
            ViewBag.CurrentSort = sortOrder;
            ViewBag.IdSortParm = sortOrder == "identifier" ? "identifier DESC" : "identifier";
            ViewBag.TitleSortParm = sortOrder == "title" ? "title DESC" : "title";
            ViewBag.RepoSortParm = sortOrder == "location" ? "location DESC" : "location";

            var requests = EadRepo.GetNotShelvedRequests(sortOrder, repoStr);

            return View(requests);
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult NotShelved(string CollectionList)
        {
            //ViewBag.Collection = CollectionList;
            return RedirectToAction("NotShelved", new { repo = CollectionList });
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult HighUse()
        {
            return View(EadRepo.GetHighUseChart());
        }

        [Authorize]
        [ApiAuthorize("RequestAdmins")]
        public ActionResult Export()
        {
            var data = EadRepo.GetExcelExportData();
            var csv = new StringBuilder();
            csv.AppendLine("id,repo,email,first_name,last_name,status,use,title,identifier,folder_item,submitted_on,phone,visit_date,completed_on,reopened,component_count");

            foreach (var r in data)
            {
                csv.AppendLine($"{r.id},{r.repo},{r.email},{r.first_name},{r.last_name},{r.status},{r.use_value},\"{r.title.Replace("\"", "\"\"")}\",=\"{r.identifier.Replace("\"", "").Replace(",", "-")}\",\"{r.folder_item}\",{r.submitted_on},{r.phone},{r.visit_date},{r.completed_on},{r.reopened},{r.component_count}");
            }
            
            System.IO.File.WriteAllText(Server.MapPath("~/Download/requestData.csv"), csv.ToString(), Encoding.UTF8);
            return Redirect("~/Download/requestData.csv");
        }
    }
}