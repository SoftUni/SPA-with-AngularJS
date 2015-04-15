namespace Ads.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Script.Serialization;

    using Ads.Web.Attributes;
    using Ads.Web.Properties;
    using Ads.Web.UserSessionUtils;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.Owin.Security;
    using Ads.Data;
    using Ads.Models;
    using Ads.Web.Models.Users;

    using Microsoft.Owin.Testing;

    [SessionAuthorize]
    [RoutePrefix("api/user")]
    public class UserController : BaseApiController
    {
        private ApplicationUserManager userManager;

        public UserController(IAdsData data)
            : base(data)
        {
        }

        public UserController()
            : base(new AdsData())
        {
            this.userManager = new ApplicationUserManager(
                new UserStore<ApplicationUser>(new ApplicationDbContext()));
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return this.userManager;
            }
        }

        private IAuthenticationManager Authentication
        {
            get
            {
                return Request.GetOwinContext().Authentication;
            }
        }

        // POST api/User/Register
        [HttpPost]
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> RegisterUser(RegisterUserBindingModel model)
        {
            if (model == null)
            {
                return this.BadRequest("Invalid user data");
            }

            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Name = model.Name,
                Email = model.Email,
                PhoneNumber = model.Phone,
                TownId = model.TownId
            };

            var identityResult = await this.UserManager.CreateAsync(user, model.Password);

            if (!identityResult.Succeeded)
            {
                return this.GetErrorResult(identityResult);
            }

            // Auto login after registration (successful user registration should return access_token)
            var loginResult = await this.LoginUser(new LoginUserBindingModel()
            {
                Username = model.Username,
                Password = model.Password
            });
            return loginResult;
        }

        // POST api/User/Login
        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        public async Task<IHttpActionResult> LoginUser(LoginUserBindingModel model)
        {
            if (model == null)
            {
                return this.BadRequest("Invalid user data");
            }

            // Invoke the "token" OWIN service to perform the login (POST /api/token)
            // Use Microsoft.Owin.Testing.TestServer to perform in-memory HTTP POST request
            var testServer = TestServer.Create<Startup>();
            var requestParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", model.Username),
                new KeyValuePair<string, string>("password", model.Password)
            };
            var requestParamsFormUrlEncoded = new FormUrlEncodedContent(requestParams);
            var tokenServiceResponse = await testServer.HttpClient.PostAsync(
                Startup.TokenEndpointPath, requestParamsFormUrlEncoded);

            if (tokenServiceResponse.StatusCode == HttpStatusCode.OK)
            {
                // Sucessful login --> create user session in the database
                var responseString = await tokenServiceResponse.Content.ReadAsStringAsync();
                var jsSerializer = new JavaScriptSerializer();
                var responseData =
                    jsSerializer.Deserialize<Dictionary<string, string>>(responseString);
                var authToken = responseData["access_token"];
                var username = responseData["username"];
                var userSessionManager = new UserSessionManager();
                userSessionManager.CreateUserSession(username, authToken);

                // Cleanup: delete expired sessions fromthe database
                userSessionManager.DeleteExpiredSessions();
            }

            return this.ResponseMessage(tokenServiceResponse);
        }

        // POST api/User/Logout
        [HttpPost]
        [SessionAuthorize]
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            // This does not actually perform logout! The OWIN OAuth implementation
            // does not support "revoke OAuth token" (logout) by design.
            this.Authentication.SignOut(DefaultAuthenticationTypes.ExternalBearer);
            
            // Delete the user's session from the database (revoke its bearer token)
            var userSessionManager = new UserSessionManager();
            userSessionManager.InvalidateUserSession();

            return this.Ok(
                new
                {
                    message = "Logout successful."
                }
            );
        }

        // POST api/User/Ads
        [HttpPost]
        [Route("Ads")]
        public IHttpActionResult CreateNewAd(UserCreateAdBindingModel model)
        {
            // Validate the input parameters
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            if (!this.ValidateImageSize(model.ImageDataURL))
            {
                return this.BadRequest(string.Format("The image size should be less than {0}kb!", ImageKilobytesLimit));
            }

            // Validate that the current user exists in the database
            var currentUserId = User.Identity.GetUserId();
            var currentUser = this.Data.Users.All().FirstOrDefault(x => x.Id == currentUserId);
            if (currentUser == null)
            {
                return this.BadRequest("Invalid user token! Please login again!");
            }

            var ad = new Advertisement()
            {
                Title = model.Title,
                Text = model.Text,
                ImageDataURL = model.ImageDataURL,
                CategoryId = model.CategoryId,
                TownId = model.TownId,
                Date = DateTime.Now,
                Status = AdvertisementStatus.WaitingApproval,
                OwnerId = currentUserId
            };

            this.Data.Ads.Add(ad);

            this.Data.SaveChanges();

            return this.Ok(
                new
                {
                    message = "Advertisement created successfully.",
                    adId = ad.Id
                }
            );
        }

        // GET api/User/Ads
        [HttpGet]
        [Route("Ads")]
        public IHttpActionResult GetAds([FromUri]GetUserAdsBindingModel model)
        {
            if (model == null)
            {
                // When no parameters are passed, the model is null, so we create an empty model
                model = new GetUserAdsBindingModel();
            }

            // Validate the input parameters
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Validate the current user exists in the database
            var currentUserId = User.Identity.GetUserId();
            var currentUser = this.Data.Users.All().FirstOrDefault(x => x.Id == currentUserId);
            if (currentUser == null)
            {
                return this.BadRequest("Invalid user token! Please login again!");
            }

            // Select current user's ads by given status
            var ads = this.Data.Ads.All().Include(ad => ad.Category).Include(ad => ad.Town);
            if (model.Status.HasValue)
            {
                ads = ads.Where(ad => ad.Status == model.Status.Value);
            }
            ads = ads.Where(ad => ad.OwnerId == currentUserId);
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

            // Select the columns to be returned 
            var adsToReturn = ads.ToList().Select(ad => new
            {
                id = ad.Id,
                title = ad.Title,
                text = ad.Text,
                date = ad.Date.ToString("o"),
                imageDataUrl = ad.ImageDataURL,
                categoryName = ad.Category == null ? null : ad.Category.Name,
                townName = ad.Town == null ? null : ad.Town.Name,
                status = ad.Status.ToString(),
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

        // PUT api/User/Ads/Deactivate/{id}
        [HttpPut]
        [Route("Ads/Deactivate/{id:int}")]
        public IHttpActionResult DeactivateAd(int id)
        {
            return ChangeAdStatus(id, AdvertisementStatus.Inactive,
                "Advertisement #" + id + " deactivated.");
        }

        // PUT api/User/Ads/PublishAgain/{id}
        [HttpPut]
        [Route("Ads/PublishAgain/{id:int}")]
        public IHttpActionResult PublishAgainAd(int id)
        {
            return ChangeAdStatus(id, AdvertisementStatus.WaitingApproval,
                "Advertisement #" + id + " submitted for approval.");
        }

        private IHttpActionResult ChangeAdStatus(int advertisementId,
            AdvertisementStatus newAdvertisementStatus, string message)
        {
            var ad = this.Data.Ads.All().FirstOrDefault(a => a.Id == advertisementId);

            if (ad == null)
            {
                return this.BadRequest("Advertisement #" + advertisementId + " not found!");
            }

            // Validate the current user ownership over the ad
            var currentUserId = User.Identity.GetUserId();
            if (ad.OwnerId != currentUserId)
            {
                return this.Unauthorized();
            }

            ad.Status = newAdvertisementStatus;

            this.Data.SaveChanges();

            return this.Ok(new { message });
        }

        // GET api/User/Ads/{id}
        [HttpGet]
        [Route("Ads/{id:int}")]
        public IHttpActionResult GetAdById(int id)
        {
            var ad = this.Data.Ads.All()
                .Include(a => a.Category).Include(a => a.Town)
                .FirstOrDefault(d => d.Id == id);
            if (ad == null)
            {
                return this.BadRequest("Advertisement #" + id + " not found!");
            }

            // Validate the current user ownership over the ad
            var currentUserId = User.Identity.GetUserId();
            if (ad.OwnerId != currentUserId)
            {
                return this.Unauthorized();
            }

            return this.Ok(new
            {
                id = ad.Id,
                title = ad.Title,
                text = ad.Text,
                date = ad.Date.ToString("o"),
                imageDataUrl = ad.ImageDataURL,
                categoryId = ad.CategoryId,
                categoryName = ad.Category == null ? null : ad.Category.Name,
                townId = ad.TownId,
                townName = ad.Town == null ? null : ad.Town.Name,
                status = ad.Status.ToString()
            });
        }

        // PUT api/User/Ads/{id}
        [HttpPut]
        [Route("Ads/{id:int}")]
        public IHttpActionResult UpdateAdd(int id, [FromBody]UserUpdateAdBindingModel model)
        {
            // Validate the input parameters
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var ad = this.Data.Ads.All().FirstOrDefault(d => d.Id == id);
            if (ad == null)
            {
                return this.BadRequest("Advertisement #" + id + " not found!");
            }

            if (!this.ValidateImageSize(model.ImageDataURL))
            {
                return this.BadRequest(string.Format("The image size should be less than {0}kb!", ImageKilobytesLimit));
            }

            // Validate the current user ownership over the ad
            var currentUserId = User.Identity.GetUserId();
            if (ad.OwnerId != currentUserId)
            {
                return this.Unauthorized();
            }

            ad.Title = model.Title;
            ad.Text = model.Text;
            if (model.ChangeImage)
            {
                ad.ImageDataURL = model.ImageDataURL;
            }
            ad.CategoryId = model.CategoryId;
            ad.TownId = model.TownId;
            ad.Status = AdvertisementStatus.Inactive;

            this.Data.Ads.SaveChanges();

            return this.Ok(
                new
                {
                    message = "Advertisement #" + id + " edited successfully."
                }
            );
        }

        // DELETE api/User/Ads/{id}
        [HttpDelete]
        [Route("Ads/{id:int}")]
        public IHttpActionResult DeleteAd(int id)
        {
            var ad = this.Data.Ads.All().FirstOrDefault(d => d.Id == id);
            if (ad == null)
            {
                return this.BadRequest("Advertisement #" + id + " not found!");
            }

            // Validate the current user ownership over the add
            var currentUserId = User.Identity.GetUserId();
            if (ad.OwnerId != currentUserId)
            {
                return this.Unauthorized();
            }

            this.Data.Ads.Delete(ad);

            this.Data.Ads.SaveChanges();

            return this.Ok(
               new
               {
                   message = "Advertisement #" + id + " deleted successfully."
               }
           );
        }

        // PUT api/User/ChangePassword
        [HttpPut]
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangeUserPassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            if (User.Identity.GetUserName() == "admin")
            {
                return this.BadRequest("Password change for user 'admin' is not allowed!");
            }

            IdentityResult result = await this.UserManager.ChangePasswordAsync(
                User.Identity.GetUserId(), model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                return this.GetErrorResult(result);
            }

            return this.Ok(
                new
                {
                    message = "Password changed successfully.",
                }
            );
        }

        // GET api/Users/Profile
        [HttpGet]
        [Route("Profile")]
        public IHttpActionResult GetUserProfile()
        {
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Validate the current user exists in the database
            var currentUserId = User.Identity.GetUserId();
            var currentUser = this.Data.Users.All().FirstOrDefault(x => x.Id == currentUserId);
            if (currentUser == null)
            {
                return this.BadRequest("Invalid user token! Please login again!");
            }

            var userToReturn = new
            {
                currentUser.Name,
                currentUser.Email,
                currentUser.PhoneNumber,
                currentUser.TownId,
            };

            return this.Ok(userToReturn);
        }

        // PUT api/Users/Profile
        [HttpPut]
        [Route("Profile")]
        public IHttpActionResult EditUserProfile(EditUserProfileBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Validate the current user exists in the database
            var currentUserId = User.Identity.GetUserId();
            var currentUser = this.Data.Users.All().FirstOrDefault(x => x.Id == currentUserId);
            if (currentUser == null)
            {
                return this.BadRequest("Invalid user token! Please login again!");
            }

            if (User.Identity.GetUserName() == "admin")
            {
                return this.BadRequest("Edit profile for user 'admin' is not allowed!");
            }

            var hasEmailTaken = this.Data.Users.All().Any(x => x.Email == model.Email);
            if (hasEmailTaken)
            {
                return this.BadRequest("Invalid email. The email is already taken!");
            }

            currentUser.Name = model.Name;
            currentUser.Email = model.Email;
            currentUser.PhoneNumber = model.PhoneNumber;
            currentUser.TownId = model.TownId;

            this.Data.SaveChanges();

            return this.Ok(
                new
                {
                    message = "User profile edited successfully.",
                });
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
