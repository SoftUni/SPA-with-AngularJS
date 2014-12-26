namespace Ads.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;

    using Ads.Data;
    using Ads.Models;

    public class CategoriesController : BaseApiController
    {
        public CategoriesController()
            : this(new AdsData())
        {
        }

        public CategoriesController(IAdsData data)
            : base(data)
        {
        }

        // GET api/Categories
        /// <returns>List of all categories sorted by Id</returns>
        [HttpGet]
        public IEnumerable<Category> GetCategories()
        {
            var categories = this.Data.Categories.All().OrderBy(category => category.Id).ToList();
            return categories;
        }
    }
}
