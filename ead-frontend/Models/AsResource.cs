using NPoco;

namespace ead_frontend.Models
{
    [TableName("resource")]
    [PrimaryKey("id")]
    public class AsResource
    {
        public int id { get; set; }
    }

    [TableName("accession")]
    [PrimaryKey("id")]
    public class AsAccession
    {
        public string identifier { get; set; }
        public string title { get; set; }
    }

}