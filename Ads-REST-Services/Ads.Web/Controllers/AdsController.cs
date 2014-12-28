namespace Ads.Web.Controllers
{
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Http;
    using Ads.Data;
    using Ads.Models;
    using Ads.Web.Models.Ads;
    using Ads.Web.Properties;

    [AllowAnonymous]
    public class AdsController : BaseApiController
    {
        public AdsController()
            : this(new AdsData())
        {
        }

        public AdsController(IAdsData data)
            : base(data)
        {
        }

        // GET api/Ads
        [HttpGet]
        public IHttpActionResult GetAds([FromUri]GetAdsBindingModel model)
        {
            // Validate the input parameters
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Select all published ads by given category, town
            var ads = this.Data.Ads.All().Include(ad => ad.Owner);
            if (model.CategoryId.HasValue)
            {
                ads = ads.Where(ad => ad.Category.Id == model.CategoryId);
            }
            if (model.TownId.HasValue)
            {
                ads = ads.Where(ad => ad.Town.Id == model.TownId);
            }
            ads = ads.Where(ad => ad.Status == AdvertisementStatus.Published);
            ads = ads.OrderByDescending(ad => ad.Date).ThenBy(ad => ad.Id);

            // Apply paging: find the requested page (by given start page and page size)
            int pageSize = Settings.Default.DefaultPageSize;
            if (model.PageSize.HasValue)
            {
                pageSize = model.PageSize.Value;
            }
            var numItems = ads.Count();
            var numPages = (numItems + pageSize - 1) / pageSize;
            if (model.StartPage.HasValue)
            {
                ads = ads.Skip(pageSize * (model.StartPage.Value - 1));
            }
            ads = ads.Take(pageSize);

            // Select only the columns to be returned 
            var adsToReturn = ads.ToList().Select(ad => new
            {
                id = ad.Id,
                title = ad.Title,
                text = ad.Text,
                date = ad.Date.ToString("o"),
                imageDataUrl = ad.ImageDataURL,
                ownerName = ad.Owner.Name,
                ownerEmail = ad.Owner.Email,
                ownerPhone = ad.Owner.PhoneNumber,
                categoryId = ad.CategoryId,
                townId = ad.TownId
            });

            return this.Ok(
                new
                {
                    numItems,
                    numPages,
                    ads = adsToReturn
                }
            );
        }
    }
}
