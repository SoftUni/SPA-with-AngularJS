namespace Ads.Web.Attributes
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;

    using Ads.Data;
    using Ads.Web.UserSessionUtils;

    public class SessionAuthorizeAttribute : AuthorizeAttribute
    {
        protected IAdsData Data { get; private set; }

        public SessionAuthorizeAttribute(IAdsData data)
        {
            this.Data = data;
        }

        public SessionAuthorizeAttribute() 
            : this(new AdsData())
        {
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (SkipAuthorization(actionContext))
            {
                return;
            }

            var userSessionManager = new UserSessionManager();
            if (userSessionManager.ReValidateSession())
            {
                base.OnAuthorization(actionContext);
            }
            else
            {
                actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.Unauthorized, "Session token expried or not valid.");
            }
        }

        private static bool SkipAuthorization(HttpActionContext actionContext)
        {
            return actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any()
                   || actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any();
        }
    }
}