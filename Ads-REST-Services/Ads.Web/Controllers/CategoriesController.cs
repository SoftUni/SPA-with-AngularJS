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

        /// <summary>
        ///     GET api/Categories/categoryId
        /// </summary>
        /// <returns>Get category by id</returns>
        public IHttpActionResult GetCategoryById(int id)
        {
            var category = this.Data.Categories
                .All()
                .FirstOrDefault(x => x.Id == id);
            if (category == null)
            {
                return this.BadRequest("Category #" + id + " not found!");
            }

            return this.Ok(category);
        }
    }
}
