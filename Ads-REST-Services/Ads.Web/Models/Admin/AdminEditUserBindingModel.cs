namespace Ads.Web.Models.Admin
{
    using System.ComponentModel.DataAnnotations;

    public class AdminEditUserBindingModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        public int? TownId { get; set; }

        public bool? IsAdmin { get; set; }
    }
}
