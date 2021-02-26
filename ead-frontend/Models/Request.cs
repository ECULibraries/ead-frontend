using System;
using System.ComponentModel.DataAnnotations;
using NPoco;

namespace ead_frontend.Models
{
    [TableName("tblRequest")]
    [PrimaryKey("id")]
    public class Request
    {
        public int id { get; set; }

        public int repo_id { get; set; }

        [Required]
        [Display(Name="Email Address")]
        [StringLength(75, ErrorMessage = "Email Address cannot exceed 75 characters.")]
        public string email { get; set; }

        [Required]
        [Display(Name = "Legal First Name")]
        [StringLength(100, ErrorMessage = "Legal First Name cannot exceed 100 characters.")]
        public string first_name { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(100, ErrorMessage = "Last Name cannot exceed 100 characters.")]
        public string last_name { get; set; }

        [Required]
        [Display(Name = "Collection/Book Title")]
        [StringLength(250, ErrorMessage = "Collection/Book Title cannot exceed 250 characters.")]
        public string title { get; set; }

        [Required]
        [Display(Name = "Collection/Book Identifier")]
        [StringLength(75, ErrorMessage = "Collection/Book Identifier cannot exceed 75 characters.")]
        public string identifier { get; set; }

        [Display(Name = "Phone Number")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
        public string phone { get; set; }

        [Required]
        [Display(Name = "Date of Visit")]
        public DateTime? visit_date { get; set; }

        [Display(Name = "Additional Notes")]
        [DataType(DataType.MultilineText)]
        public string folder_item { get; set; }

        [Display(Name = "Date Submitted")]
        public DateTime submitted_on { get; set; }

        [Display(Name = "Status")]
        public int status_id { get; set; }

        [Display(Name = "Type of Use")]
        public int use_id { get; set; }

        public DateTime? completed_on { get; set; }

        public int? reopened { get; set; }

        [Ignore]
        public string component_ids { get; set; }

        [ResultColumn]
        public int? register_id { get; set; }

        [ResultColumn]
        public string status { get; set; }

        [ResultColumn]
        public string use_value { get; set; }

        [ResultColumn]
        public string location { get; set; }

        [ResultColumn]
        public string repo { get; set; }

        [ResultColumn]
        public int component_count { get; set; }
    }
}