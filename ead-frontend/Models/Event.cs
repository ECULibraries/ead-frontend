using System;
using NPoco;

namespace ead_frontend.Models
{
    [TableName("tblEvent")]
    [PrimaryKey("id")]
    public class Event
    {
        public int id { get; set; }
        public int item_id { get; set; }
        public int location_id { get; set; }
        public DateTime date_time { get; set; }
        public string pirate_id { get; set; }
        public string note { get; set; }
    }
}