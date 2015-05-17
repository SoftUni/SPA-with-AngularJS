namespace SocialNetwork.Services
{
    using System;
    
    using Microsoft.Owin;
    using Microsoft.Owin.Security.OAuth;
    using Owin;
    
    using SocialNetwork.Services.Providers;

    public partial class Startup
    {
        public const string TokenEndpointPath = "/api/token";
        public const string PublicClientId = "self";

        static Startup()
        {
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString(TokenEndpointPath),
                Provider = new ApplicationOAuthProvider(PublicClientId),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(365),
                AllowInsecureHttp = true
            };
        }

        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public void ConfigureAuth(IAppBuilder app)
        {
            // Enable CORS
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            // Enable the application to use bearer tokens to authenticate users
            app.UseOAuthBearerTokens(OAuthOptions);
        }
    }
}
