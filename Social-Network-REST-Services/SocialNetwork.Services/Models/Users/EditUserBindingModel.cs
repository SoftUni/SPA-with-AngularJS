namespace SocialNetwork.Services.Models.Users
{
    using System.ComponentModel.DataAnnotations;
    using SocialNetwork.Models;

    public class EditUserBindingModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Gender")]
        public Gender Gender { get; set; }

        [Display(Name = "Profile image")]
        public string ProfileImageData { get; set; }

        [Display(Name = "Cover image")]
        public string CoverImageData { get; set; }
    }
}