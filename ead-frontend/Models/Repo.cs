using NPoco;

namespace ead_frontend.Models
{
    [TableName("tblRepository")]
    [PrimaryKey("id")]
    public class Repo
    {
        public int id { get; set; }
        public string value { get; set; }
    }
}