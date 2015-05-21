namespace SocialNetwork.Services.Models.Posts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SocialNetwork.Services.Models.Users;
    using SocialNetwork.Services.Models.Comments;
    using SocialNetwork.Models;

    public class PostViewModel
    {
        public int Id { get; set; }

        public UserViewModelPreview Author { get; set; }

        public UserViewModelPreview WallOwner { get; set; }

        public string PostContent { get; set; }

        public DateTime Date { get; set; }

        public int LikesCount { get; set; }

        public bool Liked { get; set; }

        public int TotalCommentsCount { get; set; }

        public IEnumerable<CommentViewModel> Comments { get; set; }

        public static PostViewModel Create(Post p, ApplicationUser currentUser)
        {
            return new PostViewModel()
            {
                Id = p.Id,
                Author = UserViewModelPreview.Create(p.Author, currentUser),
                WallOwner = UserViewModelPreview.Create(p.WallOwner, currentUser),
                PostContent = p.Content,
                Date = p.Date,
                LikesCount = p.Likes.Count,
                Liked = p.Likes
                    .Any(l => l.UserId == currentUser.Id),
                TotalCommentsCount = p.Comments.Count,
                Comments = p.Comments
                    .Reverse()
                    .Take(3)
                    .Select(c => CommentViewModel.Create(c, currentUser))
            };
        }
    }
}