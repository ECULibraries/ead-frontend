using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ead_frontend.Models;
using ead_frontend.Services;
using NPoco;

namespace ead_frontend.Repositories
{
    public class EadRepo
    {
        private static string _connectionString = "EadConnString";
        protected static List<Location> Locations;
        protected static List<Status> StatusList;
        protected static List<UseType> UseList;
        protected static List<Repo> RepoList;
        public static IDatabase Connection => new Database(_connectionString);

        public int InsertRequest(Request request)
        {
            using (var db = Connection)
            {
                db.Insert(request);

                var AsRepo = new ArchivesSpaceRepo();
                var random = new Random();

                var items = new List<Item>();

                if (request.component_ids != null)
                {
                    foreach (var cuid in request.component_ids.Split(','))
                    {
                        if (cuid.StartsWith("CD") || cuid.StartsWith("LL") || cuid.StartsWith("UA"))
                        {
                            var boxSplit = cuid.Split('_');
                            var foundBox = items.FirstOrDefault(x => x.box == boxSplit[1]);
                            if (foundBox == null)
                            {
                                var asLoc = AsRepo.GetAsLocationCodes(request.identifier, boxSplit[0]);

                                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                                var code = new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());

                                items.Add(new Item() { request_id = request.id, box = boxSplit[1], cuids = boxSplit[0], location_code = asLoc, reshelve_code = code});
                            }
                            else
                            {
                                foundBox.cuids += "," + cuid;
                            }
                        }
                        else
                        {
                            var split = cuid.Split('-');
                            foreach (var part in split)
                            {
                                if (part.StartsWith("b"))
                                {
                                    var foundBox = items.FirstOrDefault(x => x.box == part.TrimStart('b'));
                                    if (foundBox == null)
                                    {
                                        var asLoc = AsRepo.GetAsLocationCodes(request.identifier, cuid);

                                        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                                        var code = new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());

                                        items.Add(new Item() { request_id = request.id, box = part.TrimStart('b'), cuids = cuid, location_code = asLoc, reshelve_code = code });
                                    }
                                    else
                                    {
                                        foundBox.cuids += "," + cuid;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (request.identifier.Contains("("))
                {
                    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    var code = new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());

                    items.Add(new Item() { request_id = request.id, box = "1", location_code = request.identifier, reshelve_code = code });
                }
                    
                foreach (var item in items)
                {
                    db.Insert(item);
                }
              
            }
            return request.id;
        }

        public List<Request> GetRequestList(int page, int itemsPerPage, string sort, string searchString, string library, out int totalCount)
        {
            var libFilter = string.Empty;
            if (library != null && library == "Joyner")
            {
                libFilter = "AND repo_id IN (1,2,5,6)";
            }
            else if (library != null && library == "Laupus")
            {
                libFilter = "AND repo_id IN (3,4)";
            }
            var requests = new List<Request>();
            using (var db = Connection)
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.Replace("@", "@@");
                    requests = db.SkipTake<Request>(itemsPerPage * page, itemsPerPage,
                        "SELECT id, email, title, identifier, submitted_on, " +
                        "CASE WHEN EXISTS (SELECT 1 FROM tblItem WHERE(location_id IS NOT NULL) AND (request_id = tblRequest.id)) THEN 'In Use' " +
                        $"WHEN visit_date IS NOT NULL AND visit_date < '{DateTime.Today}' THEN 'Past' ELSE 'New' END AS status " +
                        $"FROM tblRequest WHERE completed_on IS NULL {libFilter} AND (id LIKE '%{searchString}%' OR title LIKE '%{searchString}%' OR email LIKE '%{searchString}%' OR identifier LIKE '%{searchString}%') ORDER BY {sort}");
                    totalCount = db.Single<int>($"SELECT COUNT(id) FROM tblRequest WHERE completed_on IS NULL {libFilter} AND (id LIKE '%{searchString}%' OR title LIKE '%{searchString}%' OR email LIKE '%{searchString}%' OR identifier LIKE '%{searchString}%')");
                }
                else
                {
                    requests = db.SkipTake<Request>(itemsPerPage * page, itemsPerPage,
                        "SELECT id, email, title, identifier, submitted_on, " +
                        "CASE WHEN EXISTS (SELECT 1 FROM tblItem WHERE(location_id IS NOT NULL) AND (request_id = tblRequest.id)) THEN 'In Use' " +
                        $"WHEN visit_date IS NOT NULL AND visit_date < '{DateTime.Today}' THEN 'Past' ELSE 'New' END AS status " +
                        $"FROM tblRequest WHERE completed_on IS NULL {libFilter} ORDER BY {sort}");
                    totalCount = db.Single<int>($"SELECT COUNT(id) FROM tblRequest WHERE completed_on IS NULL {libFilter}");
                }
            }
            return requests;
        }
        public List<Request> GetReshelvedRequestList(int page, int itemsPerPage, string sort, string searchString, string library, out int totalCount)
        {
            var libFilter = string.Empty;
            if (library != null && library == "Joyner")
            {
                libFilter = "AND repo_id IN (1,2,5,6)";
            }
            else if (library != null && library == "Laupus")
            {
                libFilter = "AND repo_id IN (3,4)";
            }
            var requests = new List<Request>();
            using (var db = Connection)
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.Replace("@", "@@");
                    requests = db.SkipTake<Request>(itemsPerPage * page, itemsPerPage,
                        "SELECT id, email, title, identifier, completed_on " + 
                        $"FROM tblRequest WHERE completed_on IS NOT NULL {libFilter} AND (id LIKE '%{searchString}%' OR title LIKE '%{searchString}%' OR email LIKE '%{searchString}%' OR identifier LIKE '%{searchString}%') ORDER BY {sort}");
                    totalCount = db.Single<int>($"SELECT COUNT(id) FROM tblRequest WHERE completed_on IS NOT NULL {libFilter} AND (id LIKE '%{searchString}%' OR title LIKE '%{searchString}%' OR email LIKE '%{searchString}%' OR identifier LIKE '%{searchString}%')");
                }
                else
                {
                    requests = db.SkipTake<Request>(itemsPerPage * page, itemsPerPage,
                        "SELECT id, email, title, identifier, completed_on " +
                        $"FROM tblRequest WHERE completed_on IS NOT NULL {libFilter} ORDER BY {sort}");
                    totalCount = db.Single<int>($"SELECT COUNT(id) FROM tblRequest WHERE completed_on IS NOT NULL {libFilter}");
                }
            }
            return requests;
        }

        public List<Request> GetSimilarIdentifierRequests(int id, string identifier)
        {
            using (var db = Connection)
            {
                return db.Fetch<Request>("SELECT id, email, title FROM tblRequest WHERE identifier = @0 AND id != @1 AND completed_on IS NULL", identifier, id);
            }
        }

        public Request GetRequestById(int id)
        {
            using (var db = Connection)
            {
                return db.SingleOrDefault<Request>("SELECT * FROM tblRequest WHERE id = @0", id);
            }
        }

        public void AddBoxes(int id, string boxes)
        {
            using (var db = Connection)
            {
                var existing = db.Fetch<string>("SELECT DISTINCT box FROM tblItem WHERE request_id = @0", id);
                var split = boxes.Split(',');
                var random = new Random();
                foreach (var box in split)
                {
                    var currentBox = box.Trim();
                    if(!existing.Contains(currentBox))
                    {
                        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                        var code = new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());

                        var itm = new Item() { box = currentBox, request_id = id, reshelve_code = code };
                        db.Save(itm);
                    }                    
                }
            }
        }

        //public List<Item> GetDistinctBoxesById(int id)
        //{
        //    using (var db = Connection)
        //    {
        //        return db.Fetch<Item>("SELECT box FROM tblItem WHERE request_id = @0 GROUP BY box", id);
        //    }
        //}

        public RequestViewVm GetRequestViewWithEvents(int id)
        {
            var vm = new RequestViewVm();
            using (var db = Connection)
            {
                var results = db.FetchMultiple<Request, Item, Event>("SELECT *, " +
                    "(SELECT TOP(1) id FROM tblRegister WHERE email = tblRequest.email) AS register_id " +
                    "FROM tblRequest WHERE id = @0; SELECT * FROM tblItem WHERE request_id = @0; " +
                    "SELECT * FROM tblEvent WHERE item_id IN (SELECT id FROM tblItem WHERE request_id = @0) ORDER BY date_time DESC", id);
                vm.Request = results.Item1[0];
                vm.InactiveItems = results.Item2.FindAll(x => x.location_id == null);
                vm.ActiveItems = results.Item2.FindAll(x => x.location_id != null);
                vm.Events = results.Item3;
            }
            return vm;
        }

        public List<Item> GetItemsIn(string ids)
        {
            using (var db = Connection)
            {
                return db.Fetch<Item>($"SELECT * FROM tblItem WHERE id IN ({ids})");
            }
        }
        
        public List<Location> GetLocations()
        {
            if (CacheService.Get("UseLocations", out Locations)) return Locations;

            using (var db = Connection)
            {
                Locations = db.Fetch<Location>("SELECT * FROM tblLocation");
            }
            CacheService.Add(Locations, "UseLocations", DateTime.Now.AddHours(24));

            return Locations;
        }

        public List<Status> GetStatusList()
        {
            if (CacheService.Get("StatusList", out StatusList)) return StatusList;

            using (var db = Connection)
            {
                StatusList = db.Fetch<Status>("SELECT * FROM tblStatus");
            }
            CacheService.Add(StatusList, "StatusList", DateTime.Now.AddHours(24));

            return StatusList;
        }

        public List<UseType> GetUseList()
        {
            if (CacheService.Get("UseList", out UseList)) return UseList;

            using (var db = Connection)
            {
                UseList = db.Fetch<UseType>("SELECT * FROM tblUse");
            }
            CacheService.Add(UseList, "UseList", DateTime.Now.AddHours(24));

            return UseList;
        }

        public void UpdateItemLocation(Item item)
        {
            using (var db = Connection)
            {
                db.Update("tblItem", "id", new { location_id = item.location_id }, item.id);
            }
        }

        public int GetUnReshelvedCount(int id)
        {
            using (var db = Connection)
            {
                return db.Fetch<int>("SELECT COUNT(id) FROM tblItem WHERE request_id = @0 AND ((location_id != 103 AND location_id != 12) OR location_id IS NULL)", id)[0];
            }
        }

        public void CompleteRequest(int id)
        {
            using (var db = Connection)
            {
                db.Update("tblRequest", "id", new { completed_on = DateTime.Now }, id);
            }
        }

        public void ReOpenRequest(int id)
        {
            using (var db = Connection)
            {
                var curVal = db.SingleOrDefault<int?>("SELECT reopened FROM tblRequest WHERE id = @0", id);
                var newVal = curVal == null ? 1 : curVal + 1;
                db.Update<Request>("SET completed_on=NULL, reopened=@0 WHERE id=@1", newVal, id);
            }
        }

        public void CreateEvent(Event evt)
        {
            using (var db = Connection)
            {
                db.Save(evt);
            }
        }

        public List<StatusChart> GetStatusChart(string repo, string start, string end)
        {
            var idFilter = repo == "All Repositories" ? "" : $" AND repo_id = {GetRepoList().Find(x => x.value == repo).id}";
            var dateFilter = string.Empty;

            if (start != string.Empty)
            {
                dateFilter = $" AND submitted_on >= '{start}' AND submitted_on <= '{end}'";
            }

            using (var db = Connection)
            {
                return db.Fetch<StatusChart>($"SELECT tblStatus.status, (SELECT COUNT(id) FROM tblRequest WHERE status_id = tblStatus.id{idFilter}{dateFilter}) AS request_count, (SELECT SUM(reopened) FROM tblRequest WHERE status_id = tblStatus.id{idFilter}{dateFilter}) AS reopened_count FROM tblStatus;");
            }
        }

        public List<StatusChart> GetUseChart(string repo, string start, string end)
        {
            var idFilter = repo == "All Repositories" ? "" : $" AND repo_id = {GetRepoList().Find(x => x.value == repo).id}";
            var dateFilter = string.Empty;
            
            if (start != string.Empty)
            {
                dateFilter = $" AND submitted_on >= '{start}' AND submitted_on <= '{end}'";
            }

            using (var db = Connection)
            {
                return db.Fetch<StatusChart>($"SELECT tblUse.value AS status, (SELECT COUNT(id) FROM tblRequest WHERE use_id = tblUse.id{idFilter}{dateFilter}) AS request_count, (SELECT SUM(reopened) FROM tblRequest WHERE use_id = tblUse.id{idFilter}{dateFilter}) AS reopened_count FROM tblUse;");
            }
        }

        public List<YearChart> GetYearChart(string repo, int startyear, int startmonth, int endyear, int endmonth)
        {
            var repoId = repo == "All Repositories" ? "0" : repo == "Joyner" ? "IN (1,2,5)" : $"= {GetRepoList().Find(x => x.value == repo).id}";

            var startDate = new DateTime(startyear, startmonth, 1);
            var endDate = new DateTime(endyear, endmonth, 1);
            var sql = string.Empty;
            var count = 1;
            while (startDate <= endDate)
            {
                if (repoId == "0")
                {
                    sql +=
                        $"SELECT {count} AS filter, '{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(startDate.Month)}' AS month, COUNT(id) AS submitted_count, " +
                        $"(SELECT COUNT(id) FROM tblRequest WHERE completed_on IS NOT NULL AND YEAR(completed_on) = {startDate.Year} AND MONTH(completed_on) = {startDate.Month}) AS completed_count  " +
                        $"FROM tblRequest WHERE YEAR(submitted_on) = {startDate.Year} AND MONTH(submitted_on) = {startDate.Month} UNION ";
                }
                else
                {
                    sql +=
                        $"SELECT {count} AS filter, '{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(startDate.Month)}' AS month, COUNT(id) AS submitted_count, " +
                        $"(SELECT COUNT(id) FROM tblRequest WHERE completed_on IS NOT NULL AND YEAR(completed_on) = {startDate.Year} AND MONTH(completed_on) = {startDate.Month}) AS completed_count  " +
                        $"FROM tblRequest WHERE YEAR(submitted_on) = {startDate.Year} AND MONTH(submitted_on) = {startDate.Month} AND repo_id {repoId} UNION ";
                }
                startDate = startDate.AddMonths(1);
                count++;
            }

            sql = sql.Substring(0, sql.Length - 6) + "ORDER BY filter";
            
            using (var db = Connection)
            {
                return db.Fetch<YearChart>(sql);
            }
        }

        public List<YearChart> GetItemYearChart(string repo, int startyear, int startmonth, int endyear, int endmonth)
        {
            var repoId = repo == "All Repositories" ? "0" : repo == "Joyner" ? "IN (1,2,5)" : $"= {GetRepoList().Find(x => x.value == repo).id}";

            var startDate = new DateTime(startyear, startmonth, 1);
            var endDate = new DateTime(endyear, endmonth, 1);
            var sql = string.Empty;
            var count = 1;
            while (startDate <= endDate)
            {
                if (repoId == "0")
                {
                    sql +=
                        $"SELECT {count} AS filter, '{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(startDate.Month)}' AS month, COUNT(tblItem.id) AS submitted_count, " +
                        $"(SELECT COUNT(tblItem.id) FROM tblRequest JOIN tblItem ON tblItem.request_id = tblRequest.id WHERE completed_on IS NOT NULL AND YEAR(completed_on) = {startDate.Year} AND MONTH(completed_on) = {startDate.Month}) AS completed_count " +
                        $"FROM tblRequest JOIN tblItem ON tblItem.request_id = tblRequest.id WHERE YEAR(submitted_on) = {startDate.Year} AND MONTH(submitted_on) = {startDate.Month} UNION ";
                }
                else
                {
                    sql +=
                        $"SELECT {count} AS filter, '{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(startDate.Month)}' AS month, COUNT(tblItem.id) AS submitted_count, " +
                        $"(SELECT COUNT(tblItem.id) FROM tblRequest JOIN tblItem ON tblItem.request_id = tblRequest.id WHERE completed_on IS NOT NULL AND YEAR(completed_on) = {startDate.Year} AND MONTH(completed_on) = {startDate.Month} AND repo_id {repoId}) AS completed_count " +
                        $"FROM tblRequest JOIN tblItem ON tblItem.request_id = tblRequest.id WHERE YEAR(submitted_on) = {startDate.Year} AND MONTH(submitted_on) = {startDate.Month} AND repo_id {repoId} UNION ";
                }
                startDate = startDate.AddMonths(1);
                count++;
            }

            sql = sql.Substring(0, sql.Length - 6) + "ORDER BY filter";

            using (var db = Connection)
            {
                return db.Fetch<YearChart>(sql);
            }
        }

        public List<StatusChart> GetRepoChart()
        {
            using (var db = Connection)
            {
                return db.Fetch<StatusChart>("SELECT value AS status, (SELECT COUNT(id) FROM tblRequest WHERE (repo_id = tblRepository.id)) AS request_count, (SELECT SUM(reopened) FROM tblRequest WHERE (repo_id = tblRepository.id)) AS reopened_count FROM tblRepository");
            }
        }

        public List<StatusChart> GetUseLocationChart(string repo)
        {
            using (var db = Connection)
            {
                if (repo == "All Repositories")
                {
                    return db.Fetch<StatusChart>("SELECT (SELECT location FROM tblLocation WHERE id = location_id) AS status, COUNT(*) AS request_count " +
                                                 "FROM tblItem WHERE location_id != 103 AND location_id != 12 GROUP BY location_id");
                }
                return db.Fetch<StatusChart>("SELECT (SELECT location FROM tblLocation WHERE id = location_id) AS status, COUNT(*) AS request_count FROM tblItem " +
                                             "JOIN tblRequest ON tblRequest.id = tblItem.request_id WHERE location_id != 103 AND location_id != 12 " +
                                             $"AND repo_id = {GetRepoList().Find(x => x.value == repo).id} GROUP BY location_id");
            }
        }

        public List<StatusChart> GetHowFoundChart()
        {
            using (var db = Connection)
            {
                return db.Fetch<StatusChart>("SELECT DISTINCT how_found AS status, COUNT(*) AS request_count " +
                                             "FROM tblRegister GROUP BY how_found");

            }
        }

        public List<CollectionRequest> GetCollectionRequests(int page, int itemsPerPage, string sort, string repo, string start, string end, out int totalCount)
        {
            var idFilter = repo == "All Repositories" ? "" : $"WHERE repo_id = {GetRepoList().Find(x => x.value == repo).id}";

            var dateFilter = string.Empty;

            if (start != null)
            {
                dateFilter = idFilter.StartsWith("WHERE") ? $"AND submitted_on >= '{start}' AND submitted_on <= '{end}'" : $"WHERE submitted_on >= '{start}' AND submitted_on <= '{end}'";
            }


            var results = new List<CollectionRequest>();
            using (var db = Connection)
            {
                results = db.SkipTake<CollectionRequest>(itemsPerPage * page, itemsPerPage, 
                    $"SELECT title, identifier, COUNT(*) AS count, SUM(reopened) AS reopened FROM tblRequest {idFilter} {dateFilter} GROUP BY title, identifier ORDER BY {sort}");
                totalCount = db.Fetch<int>($"SELECT COUNT(id) FROM tblRequest {idFilter} {dateFilter} GROUP BY title, identifier").Count;
            }
            return results;
        }

        public NotShelvedVm GetNotShelvedRequests(string sort, string repo)
        {
            var idFilter = repo == "All Repositories" ? "" : $"AND repo_id = {GetRepoList().Find(x => x.value == repo).id}";

            var results = new NotShelvedVm();

            using (var db = Connection)
            {
                results.Requests = db.Fetch<Request>(
                    "SELECT id, title, identifier, (SELECT value FROM tblRepository WHERE id = repo_id) AS location " +
                    $"FROM tblRequest WHERE id IN (SELECT DISTINCT request_id FROM tblItem WHERE (location_id <> 103 AND location_id IS NOT NULL)) {idFilter} ORDER BY {sort}");
                results.Items = db.Fetch<Item>(
                        "SELECT request_id, (SELECT location FROM tblLocation WHERE id = location_id) AS location, box FROM tblItem WHERE location_id <> 103 AND location_id IS NOT NULL");
            }
            return results;
        }

        public List<HighUseChart> GetHighUseChart()
        {
            using (var db = Connection)
            {
                return db.Fetch<HighUseChart>("SELECT tblRequest.identifier, box, COUNT(*) AS count FROM tblItem " +
                                              "JOIN tblRequest ON tblRequest.id = tblItem.request_id GROUP BY identifier,box HAVING COUNT(*) >= 3 ORDER BY count DESC");
            }
        }

        public List<Request> GetExcelExportData()
        {
            using (var db = Connection)
            {
                var sql =
                    "SELECT id, (SELECT value from tblRepository WHERE id = repo_id) AS repo, email, first_name, last_name, " +
                    "(SELECT status FROM tblStatus WHERE id = status_id) AS status, (SELECT value from tblUse WHERE id = use_id) AS use_value, " +
                    "title, identifier, folder_item, submitted_on, phone, visit_date, completed_on, (SELECT COUNT(*) FROM tblItem WHERE request_id = tblRequest.id) AS component_count FROM tblRequest";
                return db.Fetch<Request>(sql);
            }
        }

        public List<Repo> GetRepoList()
        {
            if (CacheService.Get("RepoList", out RepoList)) return RepoList;

            using (var db = Connection)
            {
                RepoList = db.Fetch<Repo>("SELECT * FROM tblRepository");
            }
            CacheService.Add(RepoList, "UseList", DateTime.Now.AddHours(24));

            return RepoList;
        }

        public void RegisterUser(Register reg)
        {
            using (var db = Connection)
            {
                var found = db.SingleOrDefault<Register>("SELECT id FROM tblRegister WHERE email=@0", reg.email);
                if(found ==  null) db.Save(reg);
            }
        }

        public List<Register> GetRegisterList(int page, int itemsPerPage, string sort, string searchString, out int totalCount)
        {
            var users = new List<Register>();
            using (var db = Connection)
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    users = db.SkipTake<Register>(itemsPerPage * page, itemsPerPage,
                        "SELECT id, first_name, last_name, email, registered_on " +
                        $"FROM tblRegister WHERE (first_name LIKE '%{searchString}%' OR last_name LIKE '%{searchString}%' OR email LIKE '%{searchString}%') ORDER BY {sort}");
                    totalCount = db.Single<int>($"SELECT COUNT(id) FROM tblRequest WHERE (first_name LIKE '%{searchString}%' OR last_name LIKE '%{searchString}%' OR email LIKE '%{searchString}%')");
                }
                else
                {
                    users = db.SkipTake<Register>(itemsPerPage * page, itemsPerPage,
                        "SELECT id, first_name, last_name, email, registered_on " + $"FROM tblRegister ORDER BY {sort}");
                    totalCount = db.Single<int>("SELECT COUNT(id) FROM tblRegister");
                }
            }
            return users;
        }

        public Register GetRegisteredUser(int id)
        {
            using (var db = Connection)
            {
                return db.SingleById<Register>(id);
            }
        }

        public void DeleteRegisteredUser(int id)
        {
            using (var db = Connection)
            {
                db.Delete<Register>("WHERE id = @0", id);
            }
        }

        public void DeleteRequest(int id)
        {
            using (var db = Connection)
            {
                var items = db.Fetch<int>("SELECT id FROM tblItem WHERE request_id = @0", id);
                if (items.Count > 0)
                {
                    db.Delete<Event>("WHERE item_id IN (" + string.Join(",", items) + ")");
                    db.Delete<Item>("WHERE request_id = @0", id);
                }
                db.Delete<Request>("WHERE id = @0", id);
            }
        }

        public void UpdateVisitDate(int rid, DateTime date)
        {
            using (var db = Connection)
            {
                db.Update("tblRequest", "id", new { visit_date = date.Date }, rid);
            }
        }

        public void UpdateEmail(int rid, string email)
        {
            using (var db = Connection)
            {
                db.Update("tblRequest", "id", new { email = email }, rid);
            }
        }

        public void UpdateRegisterEmail(int rid, string email)
        {
            using (var db = Connection)
            {
                db.Update("tblRegister", "id", new { email = email }, rid);
            }
        }
    }
}