using System.Web.Script.Serialization;

namespace SocialNetwork.Services.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    //using System.Web.Script.Serialization;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Testing;

    using SocialNetwork.Data;
    using SocialNetwork.Models;
    using SocialNetwork.Services.Models.Posts;
    using SocialNetwork.Services.Models.Users;
    using SocialNetwork.Services.UserSessionUtils;

    [SessionAuthorize]
    [RoutePrefix("api/users")]
    public class UsersController : BaseApiController
    {
        private readonly ApplicationUserManager userManager;

        public UsersController()
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
                return this.Request.GetOwinContext().Authentication;
            }
        }

        // POST api/User/Register
        [HttpPost]
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> RegisterUser(RegisterUserBindingModel model)
        {
            if (this.User.Identity.GetUserId() != null)
            {
                return this.BadRequest("User is already logged in.");
            }

            if (model == null)
            {
                return this.BadRequest("Invalid user data.");
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var emailExists = this.SocialNetworkData.Users.All()
                .Any(x => x.Email == model.Email);
            if (emailExists)
            {
                return this.BadRequest("Email is already taken.");
            }

            var user = new ApplicationUser()
            {
                UserName = model.Username,
                Name = model.Name,
                Email = model.Email,
                Gender = model.Gender
            };

            var identityResult = await this.UserManager.CreateAsync(user, model.Password);

            if (!identityResult.Succeeded)
            {
                return this.GetErrorResult(identityResult);
            }

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
            if (this.User.Identity.GetUserId() != null)
            {
                return this.BadRequest("User is already logged in.");
            }

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
                var username = responseData["userName"];
                var owinContext = this.Request.GetOwinContext();
                var userSessionManager = new UserSessionManager(owinContext);
                userSessionManager.CreateUserSession(username, authToken);

                // Cleanup: delete expired sessions from the database
                userSessionManager.DeleteExpiredSessions();
            }

            return this.ResponseMessage(tokenServiceResponse);
        }

        // POST api/User/Logout
        [HttpPost]
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            // This does not actually perform logout! The OWIN OAuth implementation
            // does not support "revoke OAuth token" (logout) by design.
            this.Authentication.SignOut(DefaultAuthenticationTypes.ExternalBearer);

            // Delete the user's session from the database (revoke its bearer token)
            var owinContext = this.Request.GetOwinContext();
            var userSessionManager = new UserSessionManager(owinContext);
            userSessionManager.InvalidateUserSession();

            return this.Ok(new
            {
                message = "Logout successful."
            });
        }

        [HttpGet]
        [Route("{username}/preview")]
        public IHttpActionResult GetPreview(string username)
        {
            var targetUser = this.SocialNetworkData.Users.All()
                .FirstOrDefault(u => u.UserName == username);
            if (targetUser == null)
            {
                return this.NotFound();
            }

            var loggedUserId = this.User.Identity.GetUserId();
            if (loggedUserId == null)
            {
                return this.BadRequest("Invalid session token.");
            }

            var loggedUser = this.SocialNetworkData.Users.GetById(loggedUserId);

            return this.Ok(UserViewModelPreview.Create(targetUser, loggedUser));
        }

        [HttpGet]
        [Route("search")]
        public IHttpActionResult SearchUserByName([FromUri] string searchTerm)
        {
            var loggedUserId = this.User.Identity.GetUserId();
            if (loggedUserId == null)
            {
                return this.BadRequest("Invalid session token.");
            }

            searchTerm = searchTerm.ToLower();
            var userMatches = this.SocialNetworkData.Users.All()
                .Where(u => u.Name.ToLower().Contains(searchTerm))
                .Take(5)
                .AsEnumerable()
                .Select(UserViewModelMinified.Create);

            return this.Ok(userMatches);
        }

        [HttpGet]
        [Route("{username}")]
        public IHttpActionResult GetUser(string username)
        {
            var targetUser = this.SocialNetworkData.Users.All()
                .FirstOrDefault(u => u.UserName == username);
            if (targetUser == null)
            {
                return this.NotFound();
            }

            var loggedUserId = this.User.Identity.GetUserId();
            if (loggedUserId == null)
            {
                return this.BadRequest("Invalid session token.");
            }

            var loggedUser = this.SocialNetworkData.Users.GetById(loggedUserId);

            return this.Ok(UserViewModel.Create(targetUser, loggedUser));
        }

        [HttpGet]
        [Route("{username}/wall")]
        public IHttpActionResult GetWall(string username, [FromUri]GetWallBindingModel wall)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var wallOwner = this.SocialNetworkData.Users.All()
                .FirstOrDefault(u => u.UserName == username);
            if (wallOwner == null)
            {
                return this.NotFound();
            }

            var loggedUserId = this.User.Identity.GetUserId();
            if (loggedUserId == null)
            {
                return this.BadRequest("Invalid session token.");
            }

            var loggedUser = this.SocialNetworkData.Users.GetById(loggedUserId);

            //if (wallOwner.Id != loggedUserId)
            //{
            //    var isFriendOfWallOwner = wallOwner.Friends
            //       .Any(fr => fr.Id == loggedUserId);
            //    if (!isFriendOfWallOwner)
            //    {
            //        return this.BadRequest("Cannot view non-friend wall.");
            //    }
            //}

            var candidatePosts = wallOwner.WallPosts
                .OrderByDescending(p => p.Date)
                .AsQueryable();

            if (wall.StartPostId.HasValue)
            {
                candidatePosts = candidatePosts
                    .SkipWhile(p => p.Id != wall.StartPostId)
                    .Skip(1)
                    .AsQueryable();
            }

            var pagePosts = candidatePosts
                .Take(wall.PageSize)
                .Select(p => PostViewModel.Create(p, loggedUser));

            return this.Ok(pagePosts);
        }

        [HttpGet]
        [Route("{username}/friends/preview")]
        public IHttpActionResult GetFriendsPreview(string username)
        {
            var user = this.SocialNetworkData.Users.All()
                .FirstOrDefault(u => u.UserName == username);
            if (user == null)
            {
                return this.NotFound();
            }

            var loggedUserId = this.User.Identity.GetUserId();

            bool isFriend = user.Friends
                .Any(fr => fr.Id == loggedUserId);

            if (!isFriend)
            {
                return this.BadRequest("Cannot access non-friend friends.");
            }

            var friends = user.Friends
                .Reverse()
                .Take(6)
                .Select(UserViewModelMinified.Create);

            return this.Ok(new
            {
                totalCount = user.Friends.Count(),
                friends
            });
        }

        [HttpGet]
        [Route("{username}/friends")]
        public IHttpActionResult GetFriends(string username)
        {
            var user = this.SocialNetworkData.Users.All()
                .FirstOrDefault(u => u.UserName == username);
            if (user == null)
            {
                return this.NotFound();
            }

            var loggedUserId = this.User.Identity.GetUserId();

            bool isFriend = user.Friends
                .Any(fr => fr.Id == loggedUserId);

            if (!isFriend)
            {
                return this.BadRequest("Cannot access non-friend friends.");
            }

            var friends = user.Friends
                .OrderBy(fr => fr.Name)
                .Select(UserViewModelMinified.Create);

            return this.Ok(friends);
        }
    }
}