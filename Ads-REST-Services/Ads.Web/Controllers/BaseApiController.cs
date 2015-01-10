namespace Ads.Web.Controllers
{
    using System.Web.Http;
    using Ads.Data;
    using Microsoft.AspNet.Identity;

    public class BaseApiController : ApiController
    {
        protected const int ImageKilobytesLimit = 50;

        public BaseApiController(IAdsData data)
        {
            this.Data = data;
        }

        protected IAdsData Data { get; private set; }

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

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return this.BadRequest();
                }

                return this.BadRequest(this.ModelState);
            }

            return null;
        }

        protected bool ValidateImageSize(string imageDataURL)
        {
            // Image delete
            if (imageDataURL == null)
            {
                return true;
            }

            // Every 4 bytes from Base64 is equal to 3 bytes
            if ((imageDataURL.Length / 4) * 3 >= ImageKilobytesLimit * 1024)
            {
                return false;
            }

            return true;
        }
    }
}
