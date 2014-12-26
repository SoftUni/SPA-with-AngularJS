namespace Ads.Web.Models.Admin
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using global::Ads.Models;

    public class AdminEditAdBindingModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Text { get; set; }

        public bool ChangeImage { get; set; }

        [RegularExpression("^data:image/.*$", ErrorMessage = "The image data should be valid Data URL, e.g. data:image/jpeg;base64,iVBORw0KGgo...")]
        public string ImageDataURL { get; set; }

        public string OwnerUserName { get; set; }

        public int? CategoryId { get; set; }

        public int? TownId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public AdvertisementStatus Status { get; set; }
    }
}
