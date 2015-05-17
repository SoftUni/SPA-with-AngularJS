namespace SocialNetwork.Tests.IntegrationTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SocialNetwork.Common;
    using SocialNetwork.Data;
    using SocialNetwork.Models;
    using SocialNetwork.Services.Models.Posts;

    [TestClass]
    public class ProfileControllerTests : BaseIntegrationTest
    {
        [TestMethod]
        public void GetOwnUserInfoShouldReturn200OkWhenLogged()
        {
            this.Login(SeededUserUsername, SeededUserPassword);

            var getInfoResponse = this.httpClient.GetAsync("api/me").Result;

            Assert.AreEqual(HttpStatusCode.OK, getInfoResponse.StatusCode);
        }

        [TestMethod]
        public void GetOwnUserInfoShouldReturnUnauthorizedWhenNotLogged()
        {
            var getInfoResponse = this.httpClient.GetAsync("api/me").Result;

            Assert.AreEqual(HttpStatusCode.Unauthorized, getInfoResponse.StatusCode);
        }

        [TestMethod]
        public async Task ApprovingFriendRequestShouldIncrementFriendsOfBothUsersAndMarkRequestApproved()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var responseData = loginResponse.Content.ReadAsStringAsync().Result.ToJson();
            var username = responseData["userName"];

            var friendRequest = this.Data.FriendRequests.All()
                .First(fr => fr.To.UserName == username
                    && fr.Status == FriendRequestStatus.Pending);
            if (friendRequest == null)
            {
                Assert.Fail("User does not have friend requests.");
            }

            int approverFriendsCount = friendRequest.To.Friends.Count;
            int senderFriendsCount = friendRequest.From.Friends.Count;

            var changeRequestResponse = await this.httpClient.PutAsync(
                string.Format(ApiEndpoints.ChangeRequestStatus, friendRequest.Id, "approved"), null);

            this.Data = new SocialNetworkData();

            var approver = this.Data.Users.GetById(friendRequest.To.Id);
            var sender = this.Data.Users.GetById(friendRequest.From.Id);

            Assert.AreEqual(HttpStatusCode.OK, changeRequestResponse.StatusCode);
            Assert.AreEqual(approverFriendsCount + 1, approver.Friends.Count);
            Assert.AreEqual(senderFriendsCount + 1, sender.Friends.Count);
            Assert.AreEqual(FriendRequestStatus.Approved, this.Data.FriendRequests.GetById(friendRequest.Id).Status);
        }

        [TestMethod]
        public async Task RejectingFriendRequestShouldNotChangeFriendsCountOfBothUsersAndMarkRequestRejected()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var responseData = loginResponse.Content.ReadAsStringAsync().Result.ToJson();
            var username = responseData["userName"];

            var friendRequest = this.Data.FriendRequests.All()
                .First(fr => fr.To.UserName == username 
                    && fr.Status == FriendRequestStatus.Pending);
            if (friendRequest == null)
            {
                Assert.Fail("User does not have friend requests.");
            }

            int approverFriendsCount = friendRequest.To.Friends.Count;
            int senderFriendsCount = friendRequest.From.Friends.Count;

            var changeRequestResponse = await this.httpClient.PutAsync(
                string.Format(ApiEndpoints.ChangeRequestStatus, friendRequest.Id, "rejected"), null);

            this.Data = new SocialNetworkData();

            var approver = this.Data.Users.GetById(friendRequest.To.Id);
            var sender = this.Data.Users.GetById(friendRequest.From.Id);

            Assert.AreEqual(HttpStatusCode.OK, changeRequestResponse.StatusCode);
            Assert.AreEqual(approverFriendsCount, approver.Friends.Count);
            Assert.AreEqual(senderFriendsCount, sender.Friends.Count);
            Assert.AreEqual(FriendRequestStatus.Rejected, this.Data.FriendRequests.GetById(friendRequest.Id).Status);
        }

        [TestMethod]
        public void SendingFriendRequestShouldAddPendingRequestToRecipient()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var loggedUsername = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];

            var recipient = this.Data.Users.All()
                .FirstOrDefault(u => u.UserName == "Tanio");
            if (recipient == null)
            {
                Assert.Fail();
            }

            var response = this.httpClient.PostAsync(
                string.Format(ApiEndpoints.SendFriendRequest, recipient.UserName), null).Result;

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var friendRequest = recipient.FriendRequests
                .FirstOrDefault(r => r.From.UserName == loggedUsername);
            Assert.IsNotNull(friendRequest);
            Assert.AreEqual(FriendRequestStatus.Pending, friendRequest.Status);
        }

        [TestMethod]
        public void SendingFriendRequestToSelfShouldReturnBadRequest400()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var username = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];

            var httpResponse = this.httpClient.PostAsync(
                string.Format(ApiEndpoints.SendFriendRequest, username), null).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        }

        [TestMethod]
        public void SendingFriendRequestWhenSuchPendingAlreadyExistsShouldReturnBadRequest400()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var loggedUsername = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];

            var recipient = this.Data.Users.All()
                .First(u => u.FriendRequests
                    .Any(r => r.From.UserName == loggedUsername
                              && r.Status == FriendRequestStatus.Pending));

            var response = this.httpClient.PostAsync(
                string.Format(ApiEndpoints.SendFriendRequest, recipient.UserName), null).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void GetNewsFeedShouldReturnPostsByFriendsOrPostsOnFriendWalls()
        {
            this.Login(SeededUserUsername, SeededUserPassword);

            const int pageSize = 5;
            int? startId = null;

            var getFeedResponse = this.httpClient.GetAsync(string.Format(
                "api/me/feed?pageSize={0}&startPostId={1}",
                pageSize, startId)).Result;

            var responseData = getFeedResponse.Content
                .ReadAsAsync<IEnumerable<PostViewModel>>().Result;

            foreach (var post in responseData)
            {
                Assert.IsNotNull(post.Id);
            }
        }

        [TestMethod]
        public void GetNewsFeedShouldReturnConsecutivePagesWithNonRepeatingPosts()
        {
            this.Login(SeededUserUsername, SeededUserPassword);

            var user = this.Data.Users.All()
                .First(u => u.WallPosts.Count > 10);

            const int pageSize = 1;
            int? startId = null;

            var postIds = new List<int>();

            while (true)
            {
                var getWallResponse = this.httpClient.GetAsync(string.Format(
                    "api/me/feed?pageSize={0}&startPostId={1}",
                    pageSize, startId)).Result;

                var responseData = getWallResponse.Content
                    .ReadAsAsync<IEnumerable<PostViewModel>>().Result;

                if (!responseData.Any())
                {
                    break;
                }

                foreach (var post in responseData)
                {
                    Assert.IsNotNull(post.Id);
                    postIds.Add(post.Id);
                }

                startId = responseData.LastOrDefault() == null ? null : (int?)responseData.Last().Id;
            }

            CollectionAssert.AllItemsAreUnique(postIds);
        }

        [TestMethod]
        [Ignore]
        public void TestProfileEdit()
        {
        }

        [TestMethod]
        [Ignore]
        public void TestPasswordChange()
        {
        }

        [TestMethod]
        [Ignore]
        public void GetOwnInfoShouldReturnMinifiedImage_Name_And_PendingRequests()
        {
        }
    }
}
