using X.PagedList;

namespace ead_frontend.Models
{
    public class RequestListVm
    {
        public IPagedList<Request> Requests { get; set; }
    }
}