using System;
using System.ComponentModel.DataAnnotations;
using NPoco;

namespace ead_frontend.Models
{
    [TableName("tblRegister")]
    [PrimaryKey("id")]
    public class Register
    {
        public int id { get; set; }

        [Required]
        [Display(Name = "Legal First Name")]
        [StringLength(100, ErrorMessage = "Legal First Name cannot exceed 100 characters.")]
        public string first_name { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(100, ErrorMessage = "Last Name cannot exceed 100 characters.")]
        public string last_name { get; set; }

        [Required]
        [Display(Name = "Email Address")]
        [StringLength(75, ErrorMessage = "Email Address cannot exceed 75 characters.")]
        public string email { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$", ErrorMessage = "The PhoneNumber field is not a valid phone number")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
        public string phone { get; set; }

        [Display(Name = "Address")]
        [StringLength(75, ErrorMessage = "Address cannot exceed 75 characters.")]
        public string address { get; set; }

        [Display(Name = "City/Town")]
        [StringLength(40, ErrorMessage = "City/Town cannot exceed 40 characters.")]
        public string city_town { get; set; }

        [Display(Name = "State/Province")]
        [StringLength(40, ErrorMessage = "State/Province cannot exceed 40 characters.")]
        public string state_province { get; set; }

        [Display(Name = "Zip Code")]
        [StringLength(5, ErrorMessage = "Zip Code cannot exceed 5 characters.")]
        public string zip_code { get; set; }

        [Display(Name = "Country")]
        [StringLength(40, ErrorMessage = "Country cannot exceed 40 characters.")]
        public string country { get; set; }

        [Display(Name = "Research Topics")]
        [DataType(DataType.MultilineText)]
        public string research_topics { get; set; }

        [Required]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the regulations to register.")]
        public bool regulation_agreement { get; set; }

        [Display(Name = "Date Registered")]
        public DateTime registered_on { get; set; }

        [Display(Name = "How did you find out about us?")]
        [StringLength(30, ErrorMessage = "How you found us cannot exceed 30 characters.")]
        public string how_found { get; set; }
    }
}