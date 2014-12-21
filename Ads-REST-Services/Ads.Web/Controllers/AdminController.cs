namespace Ads.Web.Controllers
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Ads.Models;
    using Ads.Web.Models.Admin;
    using Ads.Web.Properties;
    using Microsoft.AspNet.Identity.Owin;
    using Ads.Data;
    using Ads.Common;

    [Authorize(Roles = "Administrator")]
    [RoutePrefix("api/admin")]
    public class AdminController : BaseApiController
    {
        public AdminController(IAdsData data)
            : base(data)
        {
        }

        public AdminController()
            : base(new AdsData())
        {
        }

        private ApplicationUserManager userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return this.userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }

            private set
            {
                this.userManager = value;
            }
        }

        // GET api/Admin/Ads
        [HttpGet]
        [Route("Ads")]
        public IHttpActionResult GetAds([FromUri]AdminGetAdsBindingModel model)
        {
            //if (model == null)
            //{
            //    // Sometimes the model is null, so we create an empty model
            //    model = new AdminGetAdsBindingModel();
            //}

            // Validate the input parameters
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Select ads by given status, category and town
            var ads = this.Data.Ads.All()
                .Include(ad => ad.Category)
                .Include(ad => ad.Town)
                .Include(ad => ad.Owner);
            if (model.Status.HasValue)
            {
                ads = ads.Where(ad => ad.Status == model.Status.Value);
            }
            if (model.CategoryId.HasValue)
            {
                ads = ads.Where(ad => ad.CategoryId == model.CategoryId.Value);
            }
            if (model.TownId.HasValue)
            {
                ads = ads.Where(ad => ad.TownId == model.TownId.Value);
            }
            ads = ads.OrderByDescending(ad => ad.Date).ThenBy(ad => ad.Id);

            // Find the requested page (by given start page and page size)
            int pageSize = Settings.Default.DefaultPageSize;
            if (model.PageSize.HasValue)
            {
                pageSize = model.PageSize.Value;
            }
            var numPages = (ads.Count() + pageSize - 1) / pageSize;
            if (model.StartPage.HasValue)
            {
                ads = ads.Skip(pageSize * (model.StartPage.Value - 1));
            }
            ads = ads.Take(pageSize);

            // Select the columns to be returned 
            var adsToReturn = ads.ToList().Select(ad => new
            {
                id = ad.Id,
                title = ad.Title,
                text = ad.Text,
                date = ad.Date.ToString("o"),
                imageDataUrl = ad.ImageDataURL,
                ownerUsername = ad.Owner.UserName,
                ownerName = ad.Owner.Name,
                ownerEmail = ad.Owner.Email,
                ownerPhone = ad.Owner.PhoneNumber,
                categoryId = ad.CategoryId,
                categoryName = ad.Category.Name,
                townId = ad.TownId,
                townName = ad.Town.Name,
                status = ad.Status.ToString(),
            });

            return this.Ok(
                new
                {
                    numPages,
                    ads = adsToReturn
                }
            );
        }

        // GET api/Admin/Ads/{id}
        [HttpGet]
        [Route("Ads/{id:int}")]
        public IHttpActionResult GetAd(int id)
        {
            var ad = this.Data.Ads.All()
                .Include(a => a.Category)
                .Include(a => a.Town)
                .Include(a => a.Owner)
                .FirstOrDefault(a => a.Id == id);

            if (ad == null)
            {
                return this.BadRequest("Advertisement " + id + " not found!");
            }

            // Select the columns to be returned 
            var adToReturn = new
            {
                id = ad.Id,
                title = ad.Title,
                text = ad.Text,
                date = ad.Date.ToString("o"),
                imageDataUrl = ad.ImageDataURL,
                ownerUsername = ad.Owner.UserName,
                ownerName = ad.Owner.Name,
                ownerEmail = ad.Owner.Email,
                ownerPhone = ad.Owner.PhoneNumber,
                categoryId = ad.CategoryId,
                categoryName = ad.Category.Name,
                townId = ad.TownId,
                townName = ad.Town.Name,
                status = ad.Status.ToString(),
            };

            return this.Ok(adToReturn);
        }

        // PUT api/Admin/Ads/Approve/{id}
        [HttpPut]
        [Route("Ads/Approve/{id:int}")]
        public IHttpActionResult Approve(int id)
        {
            var ad = this.Data.Ads.All().FirstOrDefault(a => a.Id == id);

            if (ad == null)
            {
                return this.BadRequest("Advertisement " + id + " not found!");
            }

            ad.Status = AdvertisementStatus.Published;

            this.Data.SaveChanges();

            return this.Ok(new { message = "Advertisment published." });
        }

        // PUT api/Admin/Ads/Reject/{id}
        [HttpPut]
        [Route("Ads/Reject/{id:int}")]
        public IHttpActionResult Reject(int id)
        {
            var ad = this.Data.Ads.All().FirstOrDefault(a => a.Id == id);

            if (ad == null)
            {
                return this.BadRequest("Advertisement " + id + " not found!");
            }

            ad.Status = AdvertisementStatus.Rejected;

            this.Data.SaveChanges();

            return this.Ok(new { message = "Advertisment rejected." });
        }

        // PUT api/Admin/Ads/{id}
        [HttpPut]
        [Route("Ads/{id:int}")]
        public IHttpActionResult Put(int id, [FromBody]AdminUpdateAdBindingModel model)
        {
            // Validate the input parameters
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Find the advertisement for editing
            var ad = this.Data.Ads.All().FirstOrDefault(d => d.Id == id);
            if (ad == null)
            {
                return this.BadRequest("Advertisement " + id + " not found!");
            }

            // Modify the advertisement properties
            ad.Title = model.Title;
            ad.Text = model.Text;
            if (model.ChangeImage)
            {
                ad.ImageDataURL = model.ImageDataURL;
            }
            if (model.OwnerUserName != null)
            {
                var newOwner = this.Data.Users.All()
                    .FirstOrDefault(u => u.UserName == model.OwnerUserName);
                if (newOwner == null)
                {
                    return this.BadRequest("User not found: username = " + model.OwnerUserName);
                }
                ad.OwnerId = newOwner.Id;
            }
            ad.CategoryId = model.CategoryId;
            ad.TownId = model.TownId;
            ad.Date = model.Date;
            ad.Status = model.Status;

            // Save the changes in the database
            this.Data.Ads.SaveChanges();

            return this.Ok(
                new
                {
                    message = "Advertisement edited successfully"
                }
            );
        }

        // DELETE /api/Admin/Ads/{id}
        [HttpDelete]
        [Route("Ads/{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            var ad = this.Data.Ads.All().FirstOrDefault(d => d.Id == id);
            if (ad == null)
            {
                return this.BadRequest("Advertisement " + id + " not found!");
            }

            this.Data.Ads.Delete(ad);

            this.Data.Ads.SaveChanges();

            return this.Ok(
               new
               {
                   message = "Advertisement deleted successfully."
               }
           );
        }

        // POST api/Admin/SetPassword
        [HttpPost]
        [Route("SetPassword")]
        private async Task<IHttpActionResult> SetUserPassword(AdminSetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var user = await this.Data.Users.All().FirstOrDefaultAsync(u => u.UserName == model.Username);
            if (user == null)
            {
                return this.BadRequest("Not existing user: " + model.Username);
            }

            if (user.UserName == "admin")
            {
                return this.BadRequest("Password change for user 'admin' is not allowed!");
            }

            var removePassResult = await this.UserManager.RemovePasswordAsync(user.Id);
            if (!removePassResult.Succeeded)
            {
                return this.GetErrorResult(removePassResult);
            }

            var addPassResult = await this.UserManager.AddPasswordAsync(
                user.Id, model.NewPassword);
            if (!addPassResult.Succeeded)
            {
                return this.GetErrorResult(addPassResult);
            }

            return this.Ok(
                new
                {
                    message = "Password for user " + user.UserName + " changed successfully.",
                }
            );
        }

        // GET api/Admin/Users
        [HttpGet]
        [Route("Users")]
        public IHttpActionResult GetUsers([FromUri]AdminGetUsersBindingModel model)
        {
            // Validate the input parameters
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Select all users, ordered by the specified column (prefix '-' for descending)
            var users = this.Data.Users.All();
            if (model.OrderByColumn != null)
            {
                if (model.OrderByColumn.StartsWith("-"))
                {
                    users = users.OrderByDescending(model.OrderByColumn.Substring(1));
                }
                else
                {
                    users = users.OrderBy(model.OrderByColumn);
                }
            }

            // Find the requested page (by given start page and page size)
            int pageSize = Settings.Default.DefaultPageSize;
            if (model.PageSize.HasValue)
            {
                pageSize = model.PageSize.Value;
            }
            var numPages = (users.Count() + pageSize - 1) / pageSize;
            if (model.StartPage.HasValue)
            {
                users = users.Skip(pageSize * (model.StartPage.Value - 1));
            }
            users = users.Take(pageSize);

            // Select the columns to be returned 
            var usersToReturn = users.ToList().Select(u => new
            {
                id = u.Id,
                username = u.UserName,
                name = u.Name,
                email = u.Email,
                phoneNumber = u.PhoneNumber
            });

            return this.Ok(
                new
                {
                    numPages,
                    users = usersToReturn
                }
            );
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.UserManager.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
