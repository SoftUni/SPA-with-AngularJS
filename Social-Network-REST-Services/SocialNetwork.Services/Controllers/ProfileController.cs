using System;

namespace SocialNetwork.Services.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    using SocialNetwork.Common;
    using SocialNetwork.Data;
    using SocialNetwork.Models;
    using SocialNetwork.Services.Models.FriendRequests;
    using SocialNetwork.Services.Models.Posts;
    using SocialNetwork.Services.Models.Users;
    using SocialNetwork.Services.UserSessionUtils;
    using ChangePasswordBindingModel = SocialNetwork.Services.Models.ChangePasswordBindingModel;

    [SessionAuthorize]
    [RoutePrefix("api/me")]
    public class ProfileController : BaseApiController
    {
        private readonly ApplicationUserManager userManager;

        public ProfileController()
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

        [HttpGet]
        [Route]
        public IHttpActionResult GetProfileInfo()
        {
            var userId = this.User.Identity.GetUserId();
            if (userId == null)
            {
                return this.BadRequest("Invalid session token.");
            }

            var user = this.SocialNetworkData.Users.GetById(userId);

            return this.Ok(new
            {
                id = user.Id,
                username = user.UserName,
                name = user.Name,
                email = user.Email,
                profileImageData = user.ProfileImageData,
                gender = user.Gender,
                coverImageData = user.CoverImageData
            });
        }

        [HttpPut]
        [Route]
        public IHttpActionResult EditProfileInfo(EditUserBindingModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Validate the current user exists in the database
            var currentUserId = this.User.Identity.GetUserId();
            var currentUser = this.SocialNetworkData.Users.All()
                .FirstOrDefault(u => u.Id == currentUserId);
            if (currentUser == null)
            {
                return this.BadRequest("Invalid user token.");
            }

            var emailHolder = this.SocialNetworkData.Users.All()
                .FirstOrDefault(u => u.Email == model.Email);
            if (emailHolder != null && emailHolder.Id != currentUserId)
            {
                return this.BadRequest("Email is already taken.");
            }

            if (!this.ValidateImageSize(model.ProfileImageData, ProfileImageKilobyteLimit))
            {
                return this.BadRequest(string.Format("Profile image size should be less than {0}kb.", ProfileImageKilobyteLimit));
            }

            if (!this.ValidateImageSize(model.CoverImageData, CoverImageKilobyteLimit))
            {
                return this.BadRequest(string.Format("Cover image size should be less than {0}kb.", CoverImageKilobyteLimit));
            }

            currentUser.Name = model.Name;
            currentUser.Email = model.Email;
            currentUser.Gender = model.Gender;

            if (model.ProfileImageData != null && model.ProfileImageData.IndexOf(',') == -1)
            {
                model.ProfileImageData = string.Format("{0}{1}", "data:image/jpg;base64,", model.ProfileImageData);
            }

            currentUser.ProfileImageData = model.ProfileImageData;

            try
            {
                string source = model.ProfileImageData;
                if (source != null)
                {
                    string base64 = source.Substring(source.IndexOf(',') + 1);
                    currentUser.ProfileImageDataMinified = string.Format("{0}{1}",
                        "data:image/jpg;base64,", ImageUtility.Resize(base64, 100, 100));
                }
                else
                {
                    currentUser.ProfileImageDataMinified = null;
                }
            }
            catch (FormatException)
            {
                return this.BadRequest("Invalid Base64 string format.");
            }

            if (model.CoverImageData != null && model.CoverImageData.IndexOf(',') == -1)
            {
                model.CoverImageData = string.Format("{0}{1}", "data:image/jpg;base64,", model.CoverImageData);
            }

            currentUser.CoverImageData = model.CoverImageData;

            this.SocialNetworkData.SaveChanges();

            return this.Ok(new
            {
                message = "User profile edited successfully."
            });
        }

        [HttpPut]
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangeUserPassword(ChangePasswordBindingModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            if (this.User.Identity.GetUserName() == "admin")
            {
                return this.BadRequest("Password change for user 'admin' is not allowed!");
            }

            var result = await this.UserManager.ChangePasswordAsync(
                this.User.Identity.GetUserId(), model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                return this.GetErrorResult(result);
            }

            return this.Ok(
                new
                {
                    message = "Password successfully changed.",
                }
            );
        }

        [HttpGet]
        [Route("friends")]
        public IHttpActionResult GetFriends()
        {
            var userId = this.User.Identity.GetUserId();
            if (userId == null)
            {
                return this.BadRequest();
            }

            var user = this.SocialNetworkData.Users.GetById(userId);
            var friends = user.Friends
                .OrderBy(fr => fr.Name)
                .Select(UserViewModelMinified.Create);

            return this.Ok(friends);
        }

        [HttpGet]
        [Route("friends/preview")]
        public IHttpActionResult GetFriendsPreview()
        {
            var userId = this.User.Identity.GetUserId();
            if (userId == null)
            {
                return this.BadRequest();
            }

            var user = this.SocialNetworkData.Users.GetById(userId);
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
        [Route("feed")]
        public IHttpActionResult GetNewsFeed([FromUri]NewsFeedBindingModel feedModel)
        {
            var userId = this.User.Identity.GetUserId();
            if (userId == null)
            {
                return this.BadRequest();
            }

            if (feedModel == null)
            {
                return this.BadRequest("No page size specified.");
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var user = this.SocialNetworkData.Users.GetById(userId);
            var candidatePosts = this.SocialNetworkData.Posts.All()
                .Where(p => p.Author.Friends.Any(fr => fr.Id == userId) || 
                    p.WallOwner.Friends.Any(fr => fr.Id == userId))
                .OrderByDescending(p => p.Date)
                .AsEnumerable();

            if (feedModel.StartPostId.HasValue)
            {
                candidatePosts = candidatePosts
                    .SkipWhile(p => p.Id != feedModel.StartPostId)
                    .Skip(1);
            }

            var pagePosts = candidatePosts
                .Take(feedModel.PageSize)
                .Select(p => PostViewModel.Create(p, user));

            if (!pagePosts.Any())
            {
                return this.Ok(Enumerable.Empty<string>());
            }

            return this.Ok(pagePosts);
        }

        [HttpGet]
        [Route("requests")]
        public IHttpActionResult GetFriendRequests()
        {
            var userId = this.User.Identity.GetUserId();
            if (userId == null)
            {
                return this.BadRequest();
            }

            var user = this.SocialNetworkData.Users.GetById(userId);
            var friendRequests = user.FriendRequests
                .Where(r => r.Status == FriendRequestStatus.Pending)
                .Select(FriendRequestViewModel.Create);

            return this.Ok(friendRequests);
        }

        [HttpPut]
        [Route("requests/{requestId}")]
        public IHttpActionResult ChangeRequestStatus(int requestId, [FromUri] string status)
        {
            var request = this.SocialNetworkData.FriendRequests.GetById(requestId);
            if (request == null)
            {
                return this.NotFound();
            }

            var userId = this.User.Identity.GetUserId();
            if (userId == null)
            {
                return this.BadRequest("Invalid session token.");
            }

            if (request.Status != FriendRequestStatus.Pending)
            {
                return this.BadRequest("Request status is already resolved.");
            }

            var user = this.SocialNetworkData.Users.GetById(userId);
            if (request.ToId != userId)
            {
                return this.BadRequest("Friend request belongs to different user.");
            }

            if (status == "approved")
            {
                request.Status = FriendRequestStatus.Approved;
                user.Friends.Add(request.From);
                request.From.Friends.Add(user);
            }
            else if (status == "rejected")
            {
                request.Status = FriendRequestStatus.Rejected;
            }
            else
            {
                return this.BadRequest("Invalid friend request status.");
            }

            this.SocialNetworkData.SaveChanges();

            return this.Ok(new
            {
                message = string.Format("Friend request successfully {0}.", status)
            });
        }

        [HttpPost]
        [Route("requests/{username}")]
        public IHttpActionResult SendFriendRequest(string username)
        {
            var recipient = this.SocialNetworkData.Users.All()
                .FirstOrDefault(u => u.UserName == username);
            if (recipient == null)
            {
                return this.NotFound();
            }

            var loggedUserId = this.User.Identity.GetUserId();
            if (loggedUserId == null)
            {
                return this.BadRequest("Invalid session token.");
            }

            var loggedUser = this.SocialNetworkData.Users.GetById(loggedUserId);
            if (username == loggedUser.UserName)
            {
                return this.BadRequest("Cannot send request to self.");
            }

            bool isAlreadyFriend = loggedUser.Friends
                .Any(fr => fr.UserName == recipient.UserName);
            if (isAlreadyFriend)
            {
                return this.BadRequest("User is already in friends.");
            }

            bool hasReceivedRequest = loggedUser.FriendRequests
                .Any(r => r.FromId == recipient.Id && r.Status == FriendRequestStatus.Pending);
            bool hasSentRequest = recipient.FriendRequests
                .Any(r => r.FromId == loggedUser.Id && r.Status == FriendRequestStatus.Pending);
            if (hasSentRequest || hasReceivedRequest)
            {
                return this.BadRequest("A pending request already exists.");
            }

            var friendRequest = new FriendRequest()
            {
                From = loggedUser,
                To = recipient
            };

            recipient.FriendRequests.Add(friendRequest);
            this.SocialNetworkData.SaveChanges();

            return this.Ok(new
            {
                message = string.Format(
                    "Friend request successfully sent to {0}.", recipient.Name)
            });
        }
    }
}