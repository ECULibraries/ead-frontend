using NPoco;

namespace ead_frontend.Models
{
    [TableName("object")]
    [PrimaryKey("id")]
    public class DigitalModel
    {
        public int objectID { get; set; }
        public string title { get; set; }
        public string localIdentifier { get; set; }
    }
}