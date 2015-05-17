namespace SocialNetwork.Services.Controllers
{
    using System.Linq;
    using System.Web.Http;

    using Microsoft.AspNet.Identity;

    using SocialNetwork.Data;
    using SocialNetwork.Data.Contracts;
    using SocialNetwork.Models;

    public class BaseApiController : ApiController
    {
        protected const int ProfileImageKilobyteLimit = 128;
        protected const int CoverImageKilobyteLimit = 1024;         

        public BaseApiController()
            : this(new SocialNetworkData())
        {
        }

        public BaseApiController(ISocialNetworkData data)
        {
            this.SocialNetworkData = data;
        }

        protected ISocialNetworkData SocialNetworkData { get; private set; }

        protected IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return this.InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        this.ModelState.AddModelError(string.Empty, error);
                    }
                }

                if (this.ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return this.BadRequest();
                }

                return this.BadRequest(this.ModelState);
            }

            return null;
        }

        protected bool HasAccessToPost(ApplicationUser user, Post post)
        {
            if (post.AuthorId != user.Id && post.WallOwnerId != user.Id)
            {
                bool postAuthorOrWallOwnerIsFriend = user.Friends
                    .Any(fr => fr.Id == post.AuthorId || fr.Id == post.WallOwnerId);

                if (!postAuthorOrWallOwnerIsFriend)
                {
                    return false;
                }
            }

            return true;
        }

        protected bool ValidateImageSize(string imageDataUrl, int kilobyteLimit)
        {
            // Image delete
            if (imageDataUrl == null)
            {
                return true;
            }

            // Every 4 bytes from Base64 is equal to 3 bytes
            if ((imageDataUrl.Length / 4) * 3 >= kilobyteLimit * 1024)
            {
                return false;
            }

            return true;
        }
    }
}