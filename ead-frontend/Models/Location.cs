using NPoco;

namespace ead_frontend.Models
{
    [TableName("tblLocation")]
    [PrimaryKey("id")]
    public class Location
    {
        public int id { get; set; }
        public string location { get; set; }
        public bool pre_use { get; set; }
    }
}