using System.Collections.Generic;
using System.Web;
using ead_frontend.Services;
using SolrNet;
using SolrNet.Commands.Parameters;

namespace ead_frontend.Models
{
    public class SearchModel
    {
        public SolrQueryResults<SolrModel> ObjectResults;
        public int QueryTime { get; set; }
        public int TotalResults { get; set; }

        public string Keywords { get; set; }
        public int ResultsPerPage { get; set; }
        public int PageNumber { get; set; }
        
        public List<KeyValuePair<string,string>> Filters { get; set; }

        public SearchModel(string q, string pg, string[] filter_fields, string[] filter_values)
        {
            Keywords = HttpUtility.UrlDecode(q);
            PageNumber = pg == null ? 1 : int.Parse(pg);
            ResultsPerPage = 15;

            Filters = new List<KeyValuePair<string, string>>();
            for (var i = 0; i < filter_fields?.Length; ++i)
            {
                Filters.Add(new KeyValuePair<string, string>(filter_fields[i], filter_values[i]));
            }

            var solrWorker = SolrService<SolrModel>.GetSolrOperations();

            ObjectResults = solrWorker.Query(new SolrQuery(Keywords), GetQueryOptions("search", filter_fields, filter_values));

            QueryTime = ObjectResults.Header.QTime;
            TotalResults = ObjectResults.NumFound;
        }

        private QueryOptions GetQueryOptions(string type, string[] filter_fields, string[] filter_values)
        {
            var extraParams = new Dictionary<string, string>();
            var queryOptions = new QueryOptions();

            switch (type)
            {
                case "search":
                    extraParams.Add("defType", "edismax");
                    //extraParams.Add("qf", "title abstract repo physdesc origination accessions acquisitions bionote scope name subject container");
                    extraParams.Add("qf", "eadid^6 title^5 abstract^4 bionote^3 physdesc accessions acquisitions scope topic persname corpname famname geogname container");
                    extraParams.Add("q.op", "AND");
                    extraParams.Add("facet.mincount", "1");
                    //extraParams.Add("facet.field", "[repo, name, subject]");
                    //extraParams.Add("facet", "on");
                    queryOptions.Rows = ResultsPerPage;
                    queryOptions.StartOrCursor = new StartOrCursor.Start((PageNumber - 1) * ResultsPerPage);
                    queryOptions.ExtraParams = extraParams;

                    queryOptions.Facet = new FacetParameters
                    {
                        Queries = new[]
                        {
                            new SolrFacetFieldQuery("repo"),
                            new SolrFacetFieldQuery("persname"),
                            new SolrFacetFieldQuery("corpname"),
                            new SolrFacetFieldQuery("famname"),
                            new SolrFacetFieldQuery("topic"),
                            new SolrFacetFieldQuery("geogname")
                        }
                    };

                    if (filter_fields != null)
                    {
                        for (var i = 0; i < filter_fields.Length; i++)
                        {
                            queryOptions.AddFilterQueries(new SolrQueryByField(filter_fields[i], filter_values[i]));
                        }
                    }
                    break;
            }

            return queryOptions;
        }
    }
}