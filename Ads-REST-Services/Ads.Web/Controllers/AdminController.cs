namespace Ads.Web.Controllers
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Ads.Models;
    using Ads.Web.Models.Admin;
    using Ads.Web.Properties;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
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
            this.userManager = new ApplicationUserManager(
                new UserStore<ApplicationUser>(new ApplicationDbContext()));
        }

        private ApplicationUserManager userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return this.userManager;
            }
        }

        // GET api/Admin/Ads
        [HttpGet]
        [Route("Ads")]
        public IHttpActionResult GetAds([FromUri]AdminGetAdsBindingModel model)
        {
            if (model == null)
            {
                // When no parameters are passed, the model is null, so we create an empty model
                model = new AdminGetAdsBindingModel();
            }

            // Validate the input parameters
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Select ads by given status, category and town (apply filtering)
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

            // Apply sorting by the specified column / expression (prefix '-' for descending)
            if (model.SortBy != null)
            {
                try
                {
                    // Apply custom sorting order by the specified column / expression
                    if (model.SortBy.StartsWith("-"))
                    {
                        ads = ads.OrderByDescending(model.SortBy.Substring(1)).ThenBy(ad => ad.Id);
                    }
                    else
                    {
                        ads = ads.OrderBy(model.SortBy).ThenBy(ad => ad.Id);
                    }
                }
                catch (Exception)
                {
                    return this.BadRequest("Invalid sorting expression: " + model.SortBy);
                }
            }
            else
            {
                // Apply the default sorting order: by date descending
                ads = ads.OrderByDescending(ad => ad.Date).ThenBy(ad => ad.Id);
            }

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
                ownerUsername = ad.Owner.UserName,
                ownerName = ad.Owner.Name,
                ownerEmail = ad.Owner.Email,
                ownerPhone = ad.Owner.PhoneNumber,
                categoryId = ad.CategoryId,
                categoryName = (ad.Category != null) ? ad.Category.Name : null,
                townId = ad.TownId,
                townName = (ad.Town != null) ? ad.Town.Name : null,
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

        // PUT api/Admin/Ads/Approve/{id}
        [HttpPut]
        [Route("Ads/Approve/{id:int}")]
        public IHttpActionResult ApproveAd(int id)
        {
            var ad = this.Data.Ads.All().FirstOrDefault(a => a.Id == id);

            if (ad == null)
            {
                return this.BadRequest("Advertisement #" + id + " not found!");
            }

            ad.Status = AdvertisementStatus.Published;

            this.Data.SaveChanges();

            return this.Ok(new { message = "Advertisment #" + id + " approved (published)." });
        }

        // PUT api/Admin/Ads/Reject/{id}
        [HttpPut]
        [Route("Ads/Reject/{id:int}")]
        public IHttpActionResult RejectAd(int id)
        {
            var ad = this.Data.Ads.All().FirstOrDefault(a => a.Id == id);

            if (ad == null)
            {
                return this.BadRequest("Advertisement #" + id + " not found!");
            }

            ad.Status = AdvertisementStatus.Rejected;

            this.Data.SaveChanges();

            return this.Ok(new { message = "Advertisment #" + id + " rejected." });
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
                return this.BadRequest("Advertisement #" + id + " not found!");
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
                categoryName = (ad.Category != null) ? ad.Category.Name : null,
                townId = ad.TownId,
                townName = (ad.Town != null) ? ad.Town.Name : null,
                status = ad.Status.ToString(),
            };

            return this.Ok(adToReturn);
        }

        // PUT api/Admin/Ads/{id}
        [HttpPut]
        [Route("Ads/{id:int}")]
        public IHttpActionResult EditAd(int id, [FromBody]AdminEditAdBindingModel model)
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
                return this.BadRequest("Advertisement #" + id + " not found!");
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
                    message = "Advertisement #" + id + " edited successfully."
                }
            );
        }

        // DELETE /api/Admin/Ads/{id}
        [HttpDelete]
        [Route("Ads/{id:int}")]
        public IHttpActionResult DeleteAd(int id)
        {
            var ad = this.Data.Ads.All().FirstOrDefault(d => d.Id == id);
            if (ad == null)
            {
                return this.BadRequest("Advertisement #" + id + " not found!");
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

        // GET api/Admin/Users
        [HttpGet]
        [Route("Users")]
        public IHttpActionResult GetUsers([FromUri]AdminGetUsersBindingModel model)
        {
            if (model == null)
            {
                // When no parameters are passed, the model is null, so we create an empty model
                model = new AdminGetUsersBindingModel();
            }

            // Validate the input parameters
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Select all users along with their roles
            var users = this.Data.Users.All().Include(u => u.Roles).Include(u => u.Town);

            // Apply sorting by the specified column / expression (prefix '-' for descending)
            if (model.SortBy != null)
            {
                try
                {
                    // Apply custom sorting order by the specified column / expression
                    if (model.SortBy.StartsWith("-"))
                    {
                        users = users.OrderByDescending(model.SortBy.Substring(1)).ThenBy(u => u.Id);
                    }
                    else
                    {
                        users = users.OrderBy(model.SortBy).ThenBy(u => u.Id);
                    }
                }
                catch (Exception)
                {
                    return this.BadRequest("Invalid sorting expression: " + model.SortBy);
                }
            }
            else
            {
                // Apply the default sorting order: by username
                users = users.OrderBy(u => u.UserName).ThenBy(u => u.Id);
            }

            // Apply paging: find the requested page (by given start page and page size)
            int pageSize = Settings.Default.DefaultPageSize;
            if (model.PageSize.HasValue)
            {
                pageSize = model.PageSize.Value;
            }
            var numItems = users.Count();
            var numPages = (numItems + pageSize - 1) / pageSize;
            if (model.StartPage.HasValue)
            {
                users = users.Skip(pageSize * (model.StartPage.Value - 1));
            }
            users = users.Take(pageSize);

            // Select the admin role ID
            var adminRoleId = this.Data.UserRoles.All().First(r => r.Name == "Administrator").Id;

            // Select the columns to be returned 
            var usersToReturn = users.ToList().Select(u => new
            {
                id = u.Id,
                username = u.UserName,
                name = u.Name,
                email = u.Email,
                phoneNumber = u.PhoneNumber,
                townId = u.TownId,
                townName = u.TownId != null ? u.Town.Name : null,
                isAdmin = u.Roles.Any(r => r.RoleId == adminRoleId)
            });

            return this.Ok(
                new
                {
                    numItems,
                    numPages,
                    users = usersToReturn
                }
            );
        }

        // PUT api/Admin/User/{username}
        [HttpPut]
        [Route("User/{username}")]
        public IHttpActionResult EditUserProfile(string username, 
            [FromBody]AdminEditUserBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Find the user in the database
            var user = this.Data.Users.All().FirstOrDefault(x => x.UserName == username);
            if (user == null)
            {
                return this.BadRequest("User not found: username = " + username);
            }

            if (user.UserName == "admin")
            {
                return this.BadRequest("Edit profile for user 'admin' is not allowed!");
            }

            user.Name = model.Name;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.TownId = model.TownId;

            if (model.IsAdmin.HasValue)
            {
                if (model.IsAdmin.Value)
                {
                    // Make the user administrator
                    this.UserManager.AddToRole(user.Id, "Administrator");
                }
                else
                {
                    // Make the user non-administrator
                    this.UserManager.RemoveFromRole(user.Id, "Administrator");
                }
            }

            this.Data.SaveChanges();

            return this.Ok(
                new
                {
                    message = "User " + user.UserName + " edited successfully.",
                }
            );
        }

        // PUT api/Admin/SetPassword
        [HttpPut]
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetUserPassword(AdminSetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var user = await this.Data.Users.All().FirstOrDefaultAsync(u => u.UserName == model.Username);
            if (user == null)
            {
                return this.BadRequest("User not found: " + model.Username);
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

        // DELETE /api/Admin/User/{username}
        /// <summary>
        /// Deletes user by username. All user ads are also deleted.
        /// </summary>
        [HttpDelete]
        [Route("User/{username}")]
        public IHttpActionResult DeleteUser(string username)
        {
            var user = this.UserManager.FindByName(username);
            if (user == null)
            {
                return this.BadRequest("User not found: " + username);
            }

            if (user.UserName == "admin")
            {
                return this.BadRequest("Deleting user 'admin' is not allowed!");
            }

            var currentUserId = User.Identity.GetUserId();
            if (user.Id == currentUserId)
            {
                return this.BadRequest("User cannot delete himself: " + username);
            }

            this.UserManager.Delete(user);

            return this.Ok(
               new
               {
                   message = "User " + username + " deleted successfully."
               }
           );
        }

        // GET api/Admin/Categories
        [HttpGet]
        [Route("Categories")]
        public IHttpActionResult GetCategories([FromUri]AdminGetCategoriesBindingModel model)
        {
            if (model == null)
            {
                // When no parameters are passed, the model is null, so we create an empty model
                model = new AdminGetCategoriesBindingModel();
            }

            // Validate the input parameters
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Select all categories
            var categories = this.Data.Categories.All();

            // Apply sorting by the specified column / expression (prefix '-' for descending)
            if (model.SortBy != null)
            {
                try
                {
                    // Apply custom sorting order by the specified column / expression
                    if (model.SortBy.StartsWith("-"))
                    {
                        categories = categories.OrderByDescending(
                            model.SortBy.Substring(1)).ThenBy(c => c.Id);
                    }
                    else
                    {
                        categories = categories.OrderBy(model.SortBy).ThenBy(c => c.Id);
                    }
                }
                catch (Exception)
                {
                    return this.BadRequest("Invalid sorting expression: " + model.SortBy);
                }
            }
            else
            {
                // Apply the default sorting order: by name
                categories = categories.OrderBy(c => c.Name).ThenBy(c => c.Id);
            }

            // Apply paging: find the requested page (by given start page and page size)
            int pageSize = Settings.Default.DefaultPageSize;
            if (model.PageSize.HasValue)
            {
                pageSize = model.PageSize.Value;
            }
            var numItems = categories.Count();
            var numPages = (numItems + pageSize - 1) / pageSize;
            if (model.StartPage.HasValue)
            {
                categories = categories.Skip(pageSize * (model.StartPage.Value - 1));
            }
            categories = categories.Take(pageSize);

            // Select the columns to be returned 
            var categoriesToReturn = categories.ToList().Select(c => new
            {
                id = c.Id,
                username = c.Name
            });

            return this.Ok(
                new
                {
                    numItems,
                    numPages,
                    categories = categoriesToReturn
                }
            );
        }

        // POST api/Admin/Categories
        [HttpPost]
        [Route("Categories")]
        public IHttpActionResult CreateNewCategory([FromBody]AdminCategoryBindingModel model)
        {
            // Validate the input parameters
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Create new category and assign its properties form the model
            var category = new Category
            {
                Name = model.Name
            };

            // Save the changes in the database
            this.Data.Categories.Add(category);
            this.Data.Categories.SaveChanges();

            return this.Ok(
                new
                {
                    message = "Category #" + category.Id + " created."
                }
            );
        }

        // PUT api/Admin/Categories/{id}
        [HttpPut]
        [Route("Categories/{id:int}")]
        public IHttpActionResult EditCategory(int id, [FromBody]AdminCategoryBindingModel model)
        {
            // Validate the input parameters
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Find the category for editing
            var category = this.Data.Categories.All().FirstOrDefault(d => d.Id == id);
            if (category == null)
            {
                return this.BadRequest("Category #" + id + " not found!");
            }

            // Modify the category properties
            category.Name = model.Name;

            // Save the changes in the database
            this.Data.Categories.SaveChanges();

            return this.Ok(
                new
                {
                    message = "Category #" + id + " edited successfully."
                }
            );
        }

        // DELETE /api/Admin/Categories/{id}
        [HttpDelete]
        [Route("Categories/{id:int}")]
        public IHttpActionResult DeleteCategory(int id)
        {
            var category = this.Data.Categories.All().FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return this.BadRequest("Category #" + id + " not found!");
            }

            this.Data.Categories.Delete(category);

            this.Data.Categories.SaveChanges();

            return this.Ok(
               new
               {
                   message = "Category #" + id + " deleted successfully."
               }
           );
        }

        // GET api/Admin/Towns
        [HttpGet]
        [Route("Towns")]
        public IHttpActionResult GetTowns([FromUri]AdminGetTownsBindingModel model)
        {
            if (model == null)
            {
                // When no parameters are passed, the model is null, so we create an empty model
                model = new AdminGetTownsBindingModel();
            }

            // Validate the input parameters
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Select all towns
            var towns = this.Data.Towns.All();

            // Apply sorting by the specified column / expression (prefix '-' for descending)
            if (model.SortBy != null)
            {
                try
                {
                    // Apply custom sorting order by the specified column / expression
                    if (model.SortBy.StartsWith("-"))
                    {
                        towns = towns.OrderByDescending(
                            model.SortBy.Substring(1)).ThenBy(c => c.Id);
                    }
                    else
                    {
                        towns = towns.OrderBy(model.SortBy).ThenBy(c => c.Id);
                    }
                }
                catch (Exception)
                {
                    return this.BadRequest("Invalid sorting expression: " + model.SortBy);
                }
            }
            else
            {
                // Apply the default sorting order: by name
                towns = towns.OrderBy(c => c.Name).ThenBy(c => c.Id);
            }

            // Apply paging: find the requested page (by given start page and page size)
            int pageSize = Settings.Default.DefaultPageSize;
            if (model.PageSize.HasValue)
            {
                pageSize = model.PageSize.Value;
            }
            var numItems = towns.Count();
            var numPages = (numItems + pageSize - 1) / pageSize;
            if (model.StartPage.HasValue)
            {
                towns = towns.Skip(pageSize * (model.StartPage.Value - 1));
            }
            towns = towns.Take(pageSize);

            // Select the columns to be returned 
            var townsToReturn = towns.ToList().Select(c => new
            {
                id = c.Id,
                username = c.Name
            });

            return this.Ok(
                new
                {
                    numItems,
                    numPages,
                    towns = townsToReturn
                }
            );
        }

        // POST api/Admin/Towns
        [HttpPost]
        [Route("Towns")]
        public IHttpActionResult CreateNewTown([FromBody]AdminTownBindingModel model)
        {
            // Validate the input parameters
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Create new town and assign its properties form the model
            var town = new Town
            {
                Name = model.Name
            };

            // Save the changes in the database
            this.Data.Towns.Add(town);
            this.Data.Towns.SaveChanges();

            return this.Ok(
                new
                {
                    message = "Town #" + town.Id + " created."
                }
            );
        }

        // PUT api/Admin/Towns/{id}
        [HttpPut]
        [Route("Towns/{id:int}")]
        public IHttpActionResult EditTown(int id, [FromBody]AdminTownBindingModel model)
        {
            // Validate the input parameters
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Find the town for editing
            var town = this.Data.Towns.All().FirstOrDefault(d => d.Id == id);
            if (town == null)
            {
                return this.BadRequest("Town #" + id + " not found!");
            }

            // Modify the town properties
            town.Name = model.Name;

            // Save the changes in the database
            this.Data.Towns.SaveChanges();

            return this.Ok(
                new
                {
                    message = "Town #" + id + " edited successfully."
                }
            );
        }

        // DELETE /api/Admin/Towns/{id}
        [HttpDelete]
        [Route("Towns/{id:int}")]
        public IHttpActionResult DeleteTown(int id)
        {
            var town = this.Data.Towns.All().FirstOrDefault(c => c.Id == id);
            if (town == null)
            {
                return this.BadRequest("Town #" + id + " not found!");
            }

            this.Data.Towns.Delete(town);
            this.Data.Towns.SaveChanges();

            return this.Ok(
               new
               {
                   message = "Town #" + id + " deleted successfully."
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
