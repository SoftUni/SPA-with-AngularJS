namespace SocialNetwork.Services.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Http;

    using Microsoft.AspNet.Identity;
    
    using SocialNetwork.Models;
    using SocialNetwork.Services.Models;
    using SocialNetwork.Services.Models.Comments;
    using SocialNetwork.Services.Models.Likes;
    using SocialNetwork.Services.UserSessionUtils;
    
    [SessionAuthorize]
    [RoutePrefix("api/posts/{postId}/comments")]
    public class CommentsController : BaseApiController
    {
        [HttpGet]
        [Route]
        public IHttpActionResult Get(int postId)
        {
            var userId = this.User.Identity.GetUserId();
            if (userId == null)
            {
                return this.BadRequest("Invalid session token.");
            }

            var user = this.SocialNetworkData.Users.GetById(userId);

            var existingPost = this.SocialNetworkData.Posts.All()
                .FirstOrDefault(p => p.Id == postId);
            if (existingPost == null)
            {
                return this.NotFound();
            }

            var comments = existingPost.Comments
                .OrderByDescending(c => c.Date)
                .Select(c => CommentViewModel.Create(c, user));

            return this.Ok(comments);
        }

        [HttpPost]
        [Route]
        public IHttpActionResult Post(int postId, PostCommentBindingModel commentBindingModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var post = this.SocialNetworkData.Posts.All()
                .FirstOrDefault(p => p.Id == postId);
            if (post == null)
            {
                return this.NotFound();
            }

            var userId = this.User.Identity.GetUserId();
            if (userId == null)
            {
                return this.BadRequest("Invalid session token.");
            }

            var user = this.SocialNetworkData.Users.All()
                .First(u => u.Id == userId);

            if (!this.HasAccessToPost(user, post))
            {
                return this.BadRequest("Post must be by friend or on friend's wall.");
            }

            var comment = new Comment()
            {
                AuthorId = userId,
                PostId = postId,
                Content = commentBindingModel.CommentContent,
                Date = DateTime.Now
            };
            
            this.SocialNetworkData.Comments.Add(comment);
            this.SocialNetworkData.Comments.SaveChanges();

            return this.Ok(CommentViewModel.Create(comment, user));
        }

        [HttpPut]
        [Route("{commentId}")]
        public IHttpActionResult Put(int postId, int commentId, EditCommentBindingModel comment)
        {
            var existingPost = this.SocialNetworkData.Posts.All()
                .FirstOrDefault(p => p.Id == postId);
            if (existingPost == null)
            {
                return this.NotFound();
            }

            var existingComment = this.SocialNetworkData.Comments.All()
                .FirstOrDefault(c => c.Id == commentId);
            if (existingComment == null)
            {
                return this.NotFound();
            }

            var userId = this.User.Identity.GetUserId();
            if (userId == null)
            {
                return this.BadRequest("Invalid session token.");
            }

            if (existingComment.AuthorId != userId)
            {
                return this.BadRequest("Not comment author.");
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            existingComment.Content = comment.CommentContent;
            this.SocialNetworkData.SaveChanges();

            comment.Id = commentId;
            return this.Ok(comment);
        }

        [HttpDelete]
        [Route("{commentId}")]        
        public IHttpActionResult Delete(int postId, int commentId)
        {
            var existingPost = this.SocialNetworkData.Posts.All()
                .FirstOrDefault(p => p.Id == postId);
            if (existingPost == null)
            {
                return this.NotFound();
            }

            var existingComment = this.SocialNetworkData.Comments.All()
                .FirstOrDefault(c => c.Id == commentId);
            if (existingComment == null)
            {
                return this.NotFound();
            }

            var userId = this.User.Identity.GetUserId();
            if (userId == null)
            {
                return this.BadRequest("Invalid session token.");
            }

            if (!(existingComment.AuthorId == userId || existingPost.AuthorId == userId))
            {
                return this.BadRequest("Not comment author/post owner.");
            }

            this.SocialNetworkData.Comments.Delete(existingComment);

            var commentLikes = existingComment.Likes;
            foreach (var commentLike in commentLikes)
            {
                this.SocialNetworkData.CommentLikes.Delete(commentLike);
            }

            this.SocialNetworkData.SaveChanges();

            return this.Ok();
        }

        [HttpGet]
        [Route("{commentId}/likes")]
        public IHttpActionResult GetLikes(int commentId)
        {
            var existingComment = this.SocialNetworkData.Comments.GetById(commentId);
            if (existingComment == null)
            {
                return this.NotFound();
            }

            var commentLikes = existingComment.Likes
                .Select(CommentLikeViewModel.Create);

            return this.Ok(commentLikes);
        }

        [HttpGet]
        [Route("{commentId}/likes/preview")]
        public IHttpActionResult GetLikesPreview(int commentId)
        {
            var existingComment = this.SocialNetworkData.Comments.GetById(commentId);
            if (existingComment == null)
            {
                return this.NotFound();
            }

            var commentLikes = existingComment.Likes
                .Take(10)
                .Select(CommentLikeViewModel.Create);

            return this.Ok(new
            {
                totalLikeCount = existingComment.Likes.Count,
                commentLikes
            });
        }

        [HttpPost]
        [Route("{commentId}/likes")]
        public IHttpActionResult CommentLike(int postId, int commentId)
        {
            var existingPost = this.SocialNetworkData.Posts.GetById(postId);
            if (existingPost == null)
            {
                return this.NotFound();
            }

            var existingComment = this.SocialNetworkData.Comments.GetById(commentId);
            if (existingComment == null)
            {
                return this.NotFound();
            }

            var userId = this.User.Identity.GetUserId();
            if (userId == null)
            {
                return this.BadRequest("Invalid session token.");
            }

            var user = this.SocialNetworkData.Users.GetById(userId);

            if (!this.HasAccessToPost(user, existingPost))
            {
                return this.BadRequest("No permission to like this comment.");
            }

            bool hasAlreadyLiked = existingComment.Likes.Any(l => l.UserId == userId);
            if (hasAlreadyLiked)
            {
                return this.BadRequest("Comment is already liked.");
            }

            this.SocialNetworkData.CommentLikes.Add(new CommentLike()
            {
                CommentId = existingComment.Id,
                UserId = userId
            });

            this.SocialNetworkData.SaveChanges();

            return this.Ok(new
            {
                commentId = existingComment.Id,
                likesCount = existingComment.Likes.Count,
                liked = true
            });
        }

        [HttpDelete]
        [Route("{commentId}/likes")]
        public IHttpActionResult DeleteLike(int postId, int commentId)
        {
            var existingPost = this.SocialNetworkData.Posts.GetById(postId);
            if (existingPost == null)
            {
                return this.NotFound();
            }

            var existingComment = this.SocialNetworkData.Comments.GetById(commentId);
            if (existingComment == null)
            {
                return this.NotFound();
            }

            var userId = this.User.Identity.GetUserId();
            if (userId == null)
            {
                return this.BadRequest("Invalid session token.");
            }

            var user = this.SocialNetworkData.Users.GetById(userId);

            if (!this.HasAccessToPost(user, existingPost))
            {
                return this.BadRequest("No permission to like this comment.");
            }

            var commentLike = existingComment.Likes
                .FirstOrDefault(l => l.UserId == userId);
            if (commentLike == null)
            {
                return this.BadRequest("Comment has not been liked.");
            }

            this.SocialNetworkData.CommentLikes.Delete(commentLike);
            this.SocialNetworkData.SaveChanges();

            return this.Ok(new
            {
                commentId = existingComment.Id,
                likesCount = existingComment.Likes.Count,
                liked = false
            });
        }
    }
}