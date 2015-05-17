namespace SocialNetwork.Tests.IntegrationTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SocialNetwork.Common;
    using SocialNetwork.Services.Models.Comments;

    [TestClass]
    public class CommentsControllerTests : BaseIntegrationTest
    {
        [TestMethod]
        public void GettingPostCommentsShouldReturnAllCommentsSortedByDate()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var username = loginResponse.Content
                .ReadAsStringAsync().Result.ToJson()["userName"];

            var ownPost = this.Data.Posts.All()
                .First(p => p.Author.UserName == username);

            int commentCount = ownPost.Comments.Count;

            var getResponse = this.Get(
                string.Format("api/posts/{0}/comments", ownPost.Id));

            var responseData = getResponse.Content
                .ReadAsAsync<IEnumerable<CommentViewModel>>().Result;

            Assert.AreEqual(commentCount, responseData.Count());
        }

        [TestMethod]
        public void PostingCommentOnFriendWallPostShouldReturn200Ok()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var username = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];

            var user = this.Data.Users.All()
                .First(u => u.UserName == username);
            var nonFriendPostOnFriendWall = user.Friends
                .First(fr => fr.UserName == "Jack")
                .WallPosts
                .First(p => p.Author.UserName == "Tanio");

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("commentContent", "new content")
            });

            int commentCount = nonFriendPostOnFriendWall.Comments.Count;
            var postResponse = this.httpClient.PostAsync(
                string.Format("api/posts/{0}/comments", nonFriendPostOnFriendWall.Id), formData).Result;

            this.ReloadContext();

            Assert.AreEqual(HttpStatusCode.OK, postResponse.StatusCode);
            Assert.AreEqual(commentCount + 1, this.Data.Posts.GetById(nonFriendPostOnFriendWall.Id).Comments.Count);
        }

        [TestMethod]
        public void PostingCommentOnFriendPostOnNonFriendWallShouldReturn200Ok()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var username = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];

            var user = this.Data.Users.All()
                .First(u => u.UserName == username);
            var friendPostOnNonFriendWall = user.Friends
                .First(fr => fr.UserName == "Jack")
                .OwnPosts
                .First(p => p.WallOwner.UserName == "Tanio");

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("commentContent", "new content")
            });

            int commentCount = friendPostOnNonFriendWall.Comments.Count;
            var postResponse = this.httpClient.PostAsync(
                string.Format("api/posts/{0}/comments", friendPostOnNonFriendWall.Id), formData).Result;

            this.ReloadContext();

            Assert.AreEqual(HttpStatusCode.OK, postResponse.StatusCode);
            Assert.AreEqual(commentCount + 1, this.Data.Posts.GetById(friendPostOnNonFriendWall.Id).Comments.Count);
        }

        [TestMethod]
        public void PostingCommentOnNonFriendPostOnNonFriendWallShouldReturnBadRequest()
        {
            this.Login(SeededUserUsername, SeededUserPassword);

            var nonfriendPostOnNonFriendWall = this.Data.Posts.All()
                .First(p => p.Content == "Restricted wall");

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("content", "new content")
            });

            var postResponse = this.httpClient.PostAsync(
                string.Format("api/posts/{0}/comments", nonfriendPostOnNonFriendWall.Id), formData).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, postResponse.StatusCode);
        }

        [TestMethod]
        public void EditingForeignCommentShouldReturnBadRequest()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var username = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];

            var foreignComment = this.Data.Comments.All()
                .First(c => c.Author.UserName != username);

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("postContent", "new content")
            });

            var editRequest = this.httpClient.PostAsync(
                string.Format("api/posts/{0}/comments", foreignComment.Id), formData).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, editRequest.StatusCode);
        }

        [TestMethod]
        public void EditingOwnCommentShouldReturn200Ok()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var username = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];

            var ownComment = this.Data.Comments.All()
                .First(c => c.Author.UserName == username);

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("commentContent", "edited")
            });

            var editRequest = this.httpClient.PutAsync(
                string.Format("api/posts/{0}/comments/{1}", ownComment.PostId, ownComment.Id), formData).Result;

            Assert.AreEqual(HttpStatusCode.OK, editRequest.StatusCode);
            
            this.ReloadContext();
            Assert.AreEqual("edited", this.Data.Comments.GetById(ownComment.Id).Content);
        }

        [TestMethod]
        public void DeletingOwnCommentShouldReturnRemoveCommentFromDatabase()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var username = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];

            var ownComment = this.Data.Comments.All()
                .First(c => c.Author.UserName == username);

            var deleteRequest = this.httpClient.DeleteAsync(
                string.Format("api/posts/{0}/comments/{1}", ownComment.PostId, ownComment.Id)).Result;

            Assert.AreEqual(HttpStatusCode.OK, deleteRequest.StatusCode);

            this.ReloadContext();
            Assert.IsNull(this.Data.Comments.GetById(ownComment.Id));
        }

        [TestMethod]
        public void DeletingForeignCommentAsPostAuthorShouldReturn200Ok()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var username = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];

            var foreignCommentOnOwnPost = this.Data.Posts.All()
                .First(p => p.Author.UserName == username)
                .Comments
                .First(c => c.Author.UserName != username);

            var deleteRequest = this.httpClient.DeleteAsync(
                string.Format("api/posts/{0}/comments/{1}", foreignCommentOnOwnPost.PostId, foreignCommentOnOwnPost.Id)).Result;

            Assert.AreEqual(HttpStatusCode.OK, deleteRequest.StatusCode);

            this.ReloadContext();
            Assert.IsNull(this.Data.Comments.GetById(foreignCommentOnOwnPost.Id));
        }

        [TestMethod]
        public void DeletingForeignCommentOnForeignPostShouldReturnBadRequest()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var username = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];

            var foreignCommentOnForeignPost = this.Data.Posts.All()
                .Where(p => p.Author.UserName != username)
                .First(p => p.Comments.Any(c => c.Author.UserName != username))
                .Comments
                .First(c => c.Author.UserName != username);

            var deleteRequest = this.httpClient.DeleteAsync(
                string.Format("api/posts/{0}/comments/{1}", foreignCommentOnForeignPost.PostId, foreignCommentOnForeignPost.Id)).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, deleteRequest.StatusCode);
        }

        [TestMethod]
        public void LikingUnlikedCommentShouldIncrementCommentLikes()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var username = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];

            var user = this.Data.Users.All()
                .First(u => u.UserName == username);
            var unlikedOwnComment = user
                .Comments.First(c => c.Likes.Count == 0);

            int commentLikesCount = unlikedOwnComment.Likes.Count;

            var likeResponse = this.httpClient.PostAsync(
                string.Format("api/posts/{0}/comments/{1}/likes", unlikedOwnComment.PostId, unlikedOwnComment.Id), null).Result;

            Assert.AreEqual(HttpStatusCode.OK, likeResponse.StatusCode);

            this.ReloadContext();
            Assert.AreEqual(commentLikesCount + 1, this.Data.Comments.GetById(unlikedOwnComment.Id).Likes.Count);
        }

        [TestMethod]
        public void UnlikingLikedCommentShouldDecrementCommentLikes()
        {
            var loginResponse = this.Login(SeededUserUsername, SeededUserPassword);
            var username = loginResponse.Content.ReadAsStringAsync().Result.ToJson()["userName"];

            var user = this.Data.Users.All()
                .First(u => u.UserName == username);
            var likedComment = this.Data.Comments.All()
                .First(c => c.Likes
                    .Any(l => l.UserId == user.Id));

            int commentLikes = likedComment.Likes.Count;
            var likeResponse = this.httpClient.DeleteAsync(
                string.Format("api/posts/{0}/comments/{1}/likes", likedComment.PostId, likedComment.Id)).Result;

            Assert.AreEqual(HttpStatusCode.OK, likeResponse.StatusCode);

            this.ReloadContext();
            Assert.AreEqual(commentLikes - 1, this.Data.Comments.GetById(likedComment.Id).Likes.Count);
        }
    }
}
