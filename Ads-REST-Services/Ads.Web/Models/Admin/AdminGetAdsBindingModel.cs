namespace Ads.Web.Models.Admin
{
    using System.ComponentModel.DataAnnotations;
    using global::Ads.Models;

    public class AdminGetAdsBindingModel
    {
        public AdminGetAdsBindingModel()
        {
            this.StartPage = 1;
        }

        public AdvertisementStatus? Status { get; set; }

        public int? CategoryId { get; set; }

        public int? TownId { get; set; }

        /// <summary>
        /// Sorting expression, e.g. 'Title', '-Title' (descending), 'Owner.Name'.
        /// </summary>
        public string SortBy { get; set; }
        
        [Range(1, 100000, ErrorMessage = "Page number should be in range [1...100000].")]
        public int? StartPage { get; set; }

        [Range(1, 1000, ErrorMessage = "Page size be in range [1...1000].")]
        public int? PageSize { get; set; }
    }
}
