using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ead_frontend.Models
{
    public class RequestViewVm
    {
        public Request Request { get; set; }
        public List<Item> InactiveItems { get; set; }
        public List<Item> ActiveItems { get; set; }
        public IEnumerable<SelectListItem> PreUseLocations { get; set; }

        [Required(ErrorMessage = "You must select a use location.")]
        public int SelectedPreUseLocationId { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(250, ErrorMessage = "Optional note cannot exceed 250 characters.")]
        public string PreUseLocationNote { get; set; }

        public IEnumerable<SelectListItem> InUseLocations { get; set; }

        [Required(ErrorMessage = "You must select a use location.")]
        public int SelectedInUseLocationId { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(250, ErrorMessage = "Optional note cannot exceed 250 characters.")]
        public string InUseLocationNote { get; set; }

        public List<Event> Events { get; set; }
        public List<Status> StatusList { get; set; }
        public List<UseType> UseList { get; set; }
        public List<Request> SimilarRequests { get; set; }

    }
}