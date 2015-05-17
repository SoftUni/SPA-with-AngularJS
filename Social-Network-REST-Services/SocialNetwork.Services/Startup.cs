using Microsoft.Owin;

[assembly: OwinStartup(typeof(SocialNetwork.Services.Startup))]

namespace SocialNetwork.Services
{
    using System.Threading.Tasks;
    using System.Web.Cors;
    using System.Web.Http;

    using Microsoft.Owin;
    using Microsoft.Owin.Cors;

    using Owin;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(new CorsOptions()
            {
                PolicyProvider = new CorsPolicyProvider()
                {
                    PolicyResolver = request =>
                    {
                        if (request.Path.StartsWithSegments(new PathString(TokenEndpointPath)))
                        {
                            return Task.FromResult(new CorsPolicy { AllowAnyOrigin = true });
                        }

                        return Task.FromResult<CorsPolicy>(null);
                    }
                }
            });

            this.ConfigureAuth(app);

            GlobalConfiguration.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
        }
    }
}
