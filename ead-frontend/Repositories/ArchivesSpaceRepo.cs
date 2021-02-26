using System.Collections.Generic;
using ead_frontend.Models;
using NPoco;

namespace ead_frontend.Repositories
{
    public class ArchivesSpaceRepo
    {
        private readonly string _connectionString;

        public ArchivesSpaceRepo()
        {
            _connectionString = "AsConnString";
        }

        public IDatabase Connection => new Database(_connectionString);

        public AsResource GetAsResourceByEadId(string eadid)
        {
            using (var db = Connection)
            {
                return db.FirstOrDefault<AsResource>("SELECT id FROM resource WHERE ead_id = @0;", eadid);
            }
        }

        public string GetAsLocationCodes(string eadid, string cuid)
        {
            using (var db = Connection)
            {
                return db.FirstOrDefault<string>("SELECT title FROM location WHERE id IN (" +
                                          "SELECT location_id FROM top_container_housed_at_rlshp WHERE top_container_id IN (" +
                                          "SELECT top_container_id FROM top_container_link_rlshp WHERE sub_container_id IN (" +
                                          "SELECT id FROM sub_container WHERE instance_id IN (" +
                                          "SELECT id FROM instance WHERE archival_object_id = (" +
                                          $"SELECT id FROM archival_object WHERE component_id = '{cuid}')))));");
            }
        }

        public List<AsAccession> GetUnassignedUaAccessions()
        {
            using (var db = Connection)
            {
                return db.Fetch<AsAccession>("SELECT * FROM accession WHERE repo_id = 2 AND " +
                                             "(SELECT count(id) FROM spawned_rlshp where accession_id = accession.id) = 0 ORDER BY title;");
            }
        }
    }
}