using NPoco;

namespace ead_frontend.Models
{
    [TableName("tblUse")]
    [PrimaryKey("id")]
    public class UseType
    {
        public int id { get; set; }
        public string value { get; set; }
    }
}