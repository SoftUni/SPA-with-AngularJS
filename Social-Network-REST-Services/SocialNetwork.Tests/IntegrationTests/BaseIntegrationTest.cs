namespace SocialNetwork.Tests.IntegrationTests
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web.Http;
    
    using Microsoft.Owin.Testing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Owin;

    using SocialNetwork.Common;
    using SocialNetwork.Data;
    using SocialNetwork.Data.Contracts;
    using SocialNetwork.Services;

    [TestClass]
    public class BaseIntegrationTest
    {
        protected const string SeededUserUsername = "John";
        protected const string SeededUserPassword = "John";
        protected const string RegisterTestUsername = "pitona";

        protected TestServer httpTestServer;
        protected HttpClient httpClient;

        public BaseIntegrationTest()
            : this(new SocialNetworkData())
        {
        }

        public BaseIntegrationTest(ISocialNetworkData data)
        {
            this.Data = data;
        }

        public ISocialNetworkData Data { get; set; }

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            Database.Delete("SocialNetwork");
        }

        [TestInitialize]
        public void TestInit()
        {
            // Start OWIN testing HTTP server with Web API support
            this.httpTestServer = TestServer.Create(appBuilder =>
            {
                var config = new HttpConfiguration();
                WebApiConfig.Register(config);

                var startup = new Startup();
                startup.ConfigureAuth(appBuilder);

                appBuilder.UseWebApi(config);
            });
            
            this.httpClient = this.httpTestServer.HttpClient;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (this.httpTestServer != null)
            {
                this.httpTestServer.Dispose();
            }
        }

        protected HttpResponseMessage Get(string endpoint)
        {
            var getResponse = this.httpClient.GetAsync(endpoint);

            return getResponse.Result;
        }

        protected HttpResponseMessage Post(string endpoint, HttpContent data)
        {
            var postResponse = this.httpClient.PostAsync(endpoint, data);

            return postResponse.Result;
        }

        protected HttpResponseMessage Put(string endpoint, HttpContent data)
        {
            var putResponse = this.httpClient.PutAsync(endpoint, data);

            return putResponse.Result;
        }

        protected HttpResponseMessage Delete(string endpoint)
        {
            var deleteResponse = this.httpClient.DeleteAsync(endpoint);

            return deleteResponse.Result;
        }

        protected HttpResponseMessage Login(string username, string password)
        {
            var loginData = new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            });

            var loginResponse = this.httpClient.PostAsync(ApiEndpoints.UserLogin, loginData).Result;
            var responseString = loginResponse.Content.ReadAsStringAsync().Result;
            var responseData = responseString.ToJson();

            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", responseData["access_token"]);

            return loginResponse;
        }

        protected void ReloadContext()
        {
            this.Data = new SocialNetworkData();
        }
    }
}
