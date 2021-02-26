using System.Collections.Generic;

namespace ead_frontend.Models
{
    public class RequestPrintVm
    {
        public Request Request { get; set; }
        public List<Item> Items { get; set; }
    }
}