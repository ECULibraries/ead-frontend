using System.Collections.Generic;

namespace ead_frontend.Models
{
    public class NotShelvedVm
    {
        public List<Request> Requests { get; set; }
        public List<Item> Items { get; set; }
    }
}