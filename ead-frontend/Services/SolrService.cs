using System.Configuration;
using CommonServiceLocator;
using SolrNet;

namespace ead_frontend.Services
{
    internal static class SolrService<T> where T : new()
    {
        private static ISolrOperations<T> _solrOperations;

        public static ISolrOperations<T> GetSolrOperations()
        {
            if (_solrOperations == null)
            {
                Startup.Init<T>(ConfigurationManager.AppSettings["SolrURL"]);
                _solrOperations = ServiceLocator.Current.GetInstance<ISolrOperations<T>>();
            }
            return _solrOperations;
        }
    }
}