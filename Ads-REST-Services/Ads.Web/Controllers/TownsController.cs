namespace Ads.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;

    using Newtonsoft.Json;
    using System.Web.Http.Results;

    using Ads.Common;
    using Ads.Data;
    using Ads.Models;

    public class TownsController : BaseApiController
    {
        public TownsController()
            : this(new AdsData())
        {
        }

        public TownsController(IAdsData data)
            : base(data)
        {
        }

        // GET api/Towns
        /// <returns>List of all towns sorted by Id</returns>
        [HttpGet]
        public JsonResult<IEnumerable<Town>> GetTowns()
        {
            var towns = this.Data.Towns.All().OrderBy(town => town.Id).ToEnumerable();
            return Json(towns, new JsonSerializerSettings());
        }

        /// <summary>
        ///     GET api/Towns/townId
        /// </summary>
        /// <returns>Get town by id</returns>
        public IHttpActionResult GetTownById(int id)
        {
            var town = this.Data.Towns
                .All()
                .FirstOrDefault(x => x.Id == id);
            if (town == null)
            {
                return this.BadRequest("Town #" + id + " not found!");
            }

            return this.Ok(town);
        }
    }
}
