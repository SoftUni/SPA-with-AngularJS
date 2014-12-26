namespace Ads.Web.Models.Admin
{
    using System.ComponentModel.DataAnnotations;

    public class AdminEditCategoryBindingModel
    {
        [Required]
        public string Name { get; set; }
    }
}
