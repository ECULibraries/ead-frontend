using NPoco;

namespace ead_frontend.Models
{
    [TableName("tblStatus")]
    [PrimaryKey("id")]
    public class Status
    {
        public int id { get; set; }
        public string status { get; set; }
    }
}