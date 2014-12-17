namespace Ads.Web.Models.Ads
{
    using System.ComponentModel.DataAnnotations;

    public class GetAdsBindingModel
    {
        public GetAdsBindingModel()
        {
            this.StartPage = 1;
        }

        public int? CategoryId { get; set; }

        public int? TownId { get; set; }

        [Range(1, 100000, ErrorMessage = "Page number should be in range [1...100000].")]
        public int? StartPage { get; set; }

        [Range(1, 1000, ErrorMessage = "Page size be in range [1...1000].")]
        public int? PageSize { get; set; }
    }
}
