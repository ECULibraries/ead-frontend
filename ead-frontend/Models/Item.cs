using NPoco;

namespace ead_frontend.Models
{
    [TableName("tblItem")]
    [PrimaryKey("id")]
    public class Item
    {
        public int id { get; set; }
        public int request_id { get; set; }
        public int? location_id { get; set; }
        public string box { get; set; }
        public string cuids { get; set; }
        public string location_code { get; set; }
        public string reshelve_code { get; set; }

        [Ignore]
        public bool selected { get; set; }

        [ResultColumn]
        public string location { get; set; }
    }
}