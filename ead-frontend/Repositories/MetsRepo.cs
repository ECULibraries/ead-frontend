using System.Collections.Generic;
using ead_frontend.Models;
using NPoco;

namespace ead_frontend.Repositories
{
    public static class MetsRepo
    {
        private static string _connectionString = "MetsConnString";

        public static IDatabase Connection => new Database(_connectionString);

        public static List<DigitalModel> GetDigitalObjects(string eadid)
        {
            using (var db = Connection)
            {
                return db.Fetch<DigitalModel>($"SELECT objectID, title, localIdentifier FROM object WHERE localIdentifier LIKE '{eadid}%' AND isLive = 'True' ORDER BY title;");
            }
        }
    }
}