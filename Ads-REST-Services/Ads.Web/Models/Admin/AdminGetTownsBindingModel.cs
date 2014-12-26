namespace Ads.Web.Models.Admin
{
    using System.ComponentModel.DataAnnotations;

    public class AdminGetTownsBindingModel
    {
        public AdminGetTownsBindingModel()
        {
            this.StartPage = 1;
        }
        
        /// <summary>
        /// Sorting expression, e.g. 'Name', 'Id', '-Name' (descending).
        /// </summary>
        public string SortBy { get; set; }

        [Range(1, 100000, ErrorMessage = "Page number should be in range [1...100000].")]
        public int? StartPage { get; set; }

        [Range(1, 1000, ErrorMessage = "Page size be in range [1...1000].")]
        public int? PageSize { get; set; }
    }
}
