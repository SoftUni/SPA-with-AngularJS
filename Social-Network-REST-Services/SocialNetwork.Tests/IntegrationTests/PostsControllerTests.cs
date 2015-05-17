namespace SocialNetwork.Tests.IntegrationTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SocialNetwork.Common;
    using SocialNetwork.Data;
    using SocialNetwork.Services.Models.Likes;
    using SocialNetwork.Services.Models.Posts;

    [TestClass]
    public class PostsControllerTests : BaseIntegrationTest
    {
        [TestMethod]
        public void GetPostShouldReturnInfoAboutPost()
        {
            this.Login(SeededUserUsername, SeededUserPassword);

            var post = this.Data.Posts.All().First();

            var getResponse = this.httpClient.GetAsync(
                string.Format("api/posts/{0}", post.Id)).Result;

            var responseData = getResponse.Content.ReadAsAsync<PostViewModel>().Result;

            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
            Assert.AreEqual(post.Id, responseData.Id);
        }

        [TestMethod]
        public void EditingOwnPostShouldModifyPostAndReturnData()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var username = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];

            var ownPost = this.Data.Posts.All()
                .First(p => p.Author.UserName == username);
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("postContent", "new content")
            });

            var putResponse = this.httpClient.PutAsync(
                string.Format("api/posts/{0}", ownPost.Id), formData).Result;
            var responseData = putResponse.Content.ReadAsStringAsync().Result.ToJson<string, object>();

            this.ReloadContext();

            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);
            Assert.AreEqual(responseData["content"],
                this.Data.Posts.GetById(ownPost.Id).Content);
        }

        [TestMethod]
        public void EditForeignPostShoulReturnBadRequest()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var username = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];
            var foreignPost = this.Data.Posts.All()
                .First(p => p.Author.Name != username);
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("postContent", "new content")
            });

            var putResponse = this.httpClient.PutAsync(
                string.Format("api/posts/{0}", foreignPost.Id), formData).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, putResponse.StatusCode);
        }

        [TestMethod]
        public void DeletePostShouldRemovePostFromDatabase()
        {
            this.Login(SeededUserUsername, SeededUserPassword);

            var post = this.Data.Posts.All().First();

            var deleteResponse = this.httpClient.DeleteAsync(
                string.Format("api/posts/{0}", post.Id)).Result;

            Assert.AreEqual(HttpStatusCode.OK, deleteResponse.StatusCode);

            this.Data = new SocialNetworkData();
            var samePost = this.Data.Posts.GetById(post.Id);

            Assert.IsNull(samePost);
        }

        [TestMethod]
        [Ignore]
        public void DeletingForeignPostOnOwnWallShouldReturn200Ok()
        {
            
        }

        [TestMethod]
        [Ignore]
        public void DeletingOwnPostOnForeignWallShouldReturn200Ok()
        {

        }

        [TestMethod]
        [Ignore]
        public void DeletingForeignPostOnForeignWallShouldReturnBadRequest()
        {

        }

        [TestMethod]
        public void GetDetailedLikesShouldReturnDataAboutAllLikes()
        {
            this.Login(SeededUserUsername, SeededUserPassword);

            var likedPost = this.Data.Posts.All()
                .First(p => p.Likes.Count > 0);

            var getLikesResponse = this.httpClient.GetAsync(
                string.Format("api/posts/{0}/likes", likedPost.Id)).Result;

            Assert.AreEqual(HttpStatusCode.OK, getLikesResponse.StatusCode);

            var responseData = getLikesResponse.Content
                .ReadAsAsync<ICollection<PostLikeViewModel>>().Result;

            foreach (var postLike in responseData)
            {
                Assert.IsNotNull(postLike.PostId);
                Assert.IsNotNull(postLike.User.Id);
                Assert.IsNotNull(postLike.User.Username);
                Assert.IsNotNull(postLike.User.Gender);
            }
        }

        [TestMethod]
        public void LikingUnlikedPostOnFriendWallShouldReturn200Ok()
        {
            this.Login(SeededUserUsername, SeededUserPassword);

            var unlikedPostOnFriendWall = this.Data.Posts.All()
                .FirstOrDefault(p => p.Content == "Friend wall");

            var likeResponse = this.httpClient.PostAsync(string.Format("api/posts/{0}/likes", unlikedPostOnFriendWall.Id), null).Result;

            Assert.AreEqual(HttpStatusCode.OK, likeResponse.StatusCode);
        }

        [TestMethod]
        public void LikingUnlikedPostByFriendOnNonFriendWallShouldReturn200Ok()
        {
            this.Login(SeededUserUsername, SeededUserPassword);

            var unlikedPostOnFriendWall = this.Data.Posts.All()
                .FirstOrDefault(p => p.Content == "Other wall");

            var likeResponse = this.httpClient.PostAsync(
                string.Format("api/posts/{0}/likes", unlikedPostOnFriendWall.Id), null).Result;

            Assert.AreEqual(HttpStatusCode.OK, likeResponse.StatusCode);
        }

        [TestMethod]
        public void LikingUnlikedPostByNonFriendOnNonFriendWallShouldReturnBadRequest()
        {
            this.Login(SeededUserUsername, SeededUserPassword);

            var unlikedPostOnFriendWall = this.Data.Posts.All()
                .FirstOrDefault(p => p.Content == "Restricted wall");

            var likeResponse = this.httpClient.PostAsync(string.Format("api/posts/{0}/likes", unlikedPostOnFriendWall.Id), null).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, likeResponse.StatusCode);
        }

        [TestMethod]
        public void LikingUnlikedPostShouldIncrementLikes()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var username = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];

            var unlikedOwnPost = this.Data.Users.All()
                .First(u => u.UserName == username)
                .WallPosts.First(p => p.Likes.Count == 0);

            int postLikesCount = unlikedOwnPost.Likes.Count;

            var likeResponse = this.httpClient.PostAsync(string.Format("api/posts/{0}/likes", unlikedOwnPost.Id), null).Result;

            Assert.AreEqual(HttpStatusCode.OK, likeResponse.StatusCode);

            this.Data = new SocialNetworkData();
            Assert.AreEqual(postLikesCount + 1, this.Data.Posts.GetById(unlikedOwnPost.Id).Likes.Count);
        }

        [TestMethod]
        public void UnlikingLikedPostShouldDecrementLikes()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var username = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];

            var unlikedOwnPost = this.Data.Users.All()
                .First(u => u.UserName == username)
                .WallPosts.First(p => p.Likes
                    .Any(l => l.User.UserName == username));

            int postLikesCount = unlikedOwnPost.Likes.Count;

            var unlikeResponse = this.httpClient.DeleteAsync(string.Format("api/posts/{0}/likes", unlikedOwnPost.Id)).Result;

            Assert.AreEqual(HttpStatusCode.OK, unlikeResponse.StatusCode);

            this.Data = new SocialNetworkData();
            Assert.AreEqual(postLikesCount - 1, this.Data.Posts.GetById(unlikedOwnPost.Id).Likes.Count);
        }

        [TestMethod]
        public void PostingOnOwnWallShouldAddPost()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var username = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];

            var user = this.Data.Users.All()
                .First(u => u.UserName == username);
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("postContent", "Heeey brother.."),
                new KeyValuePair<string, string>("username", user.UserName)
            });

            int postsCount = user.WallPosts.Count;

            var postResponse = this.httpClient.PostAsync(
                string.Format("api/posts"), formData).Result;

            this.ReloadContext();

            Assert.AreEqual(HttpStatusCode.OK, postResponse.StatusCode);
            Assert.AreEqual(postsCount + 1, this.Data.Users.GetById(user.Id).WallPosts.Count);
        }

        [TestMethod]
        public void PostingOnFriendWallShouldAddPostToWall()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var username = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];

            var friend = this.Data.Users.All()
                .First(u => u.Friends
                    .Any(fr => fr.UserName == username));

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("postContent", "Heeey brother.."),
                new KeyValuePair<string, string>("username", friend.UserName)
            });

            int wallPostsCounts = friend.WallPosts.Count;

            var postResponse = this.httpClient.PostAsync(
                string.Format("api/posts"), formData).Result;

            this.Data = new SocialNetworkData();

            Assert.AreEqual(HttpStatusCode.OK, postResponse.StatusCode);
            Assert.AreEqual(wallPostsCounts + 1, this.Data.Users.GetById(friend.Id).WallPosts.Count);
        }

        [TestMethod]
        public void PostingOnNonFriendWallShouldReturnBadRequest()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var username = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];

            var nonFriend = this.Data.Users.All()
                .First(u => u.UserName != username && u.Friends
                    .All(fr => fr.UserName != username));

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("content", "Heeey brother.."),
                new KeyValuePair<string, string>("userId", nonFriend.Id)
            });

            var postResponse = this.httpClient.PostAsync(
                string.Format("api/posts"), formData).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, postResponse.StatusCode);
        }
    }
}
