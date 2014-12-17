namespace Ads.Web.Models.Users
{
    using System.ComponentModel.DataAnnotations;
    using global::Ads.Models;

    public class GetUserAdsBindingModel
    {
        public GetUserAdsBindingModel()
        {
            this.StartPage = 1;
        }

        public AdvertisementStatus? Status { get; set; }

        [Range(1, 100000, ErrorMessage = "Page number should be in range [1...100000].")]
        public int? StartPage { get; set; }

        [Range(1, 1000, ErrorMessage = "Page size be in range [1...1000].")]
        public int? PageSize { get; set; }
    }
}
