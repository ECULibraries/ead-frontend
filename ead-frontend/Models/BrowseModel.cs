using SolrNet;
using System.Collections.Generic;
using ead_frontend.Services;
using SolrNet.Commands.Parameters;

namespace ead_frontend.Models
{
    public class BrowseModel
    {
        public SolrQueryResults<SolrModel> Results;
        public int QueryTime { get; set; }
        public int TotalResults { get; set; } 

        public BrowseModel(string sort)
        {
            var solrWorker = SolrService<SolrModel>.GetSolrOperations();


            Results = solrWorker.Query(new SolrQuery("*:*"), GetQueryOptions(sort));

            QueryTime = Results.Header.QTime;
            TotalResults = Results.NumFound;
        }

        private QueryOptions GetQueryOptions(string type, string value = "")
        {
            var extraParams = new Dictionary<string, string>();
            var queryOptions = new QueryOptions();

            switch (type)
            {
                case "title":
                    extraParams.Add("sort", "title_sort asc");
                    extraParams.Add("fl", "eadid, title_sort, hasObjects, repo");
                    queryOptions.Rows = 5000;
                    queryOptions.StartOrCursor = new StartOrCursor.Start(0);
                    queryOptions.ExtraParams = extraParams;
                    break;
                case "repo":
                    extraParams.Add("sort", "repo asc");
                    extraParams.Add("fl", "eadid, title_sort, hasObjects, repo");
                    queryOptions.Rows = 5000;
                    queryOptions.StartOrCursor = new StartOrCursor.Start(0);
                    queryOptions.ExtraParams = extraParams;
                    break;
            }
            return queryOptions;
        }
    }
}