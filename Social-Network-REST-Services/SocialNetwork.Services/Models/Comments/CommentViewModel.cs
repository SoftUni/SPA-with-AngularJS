namespace SocialNetwork.Services.Models.Comments
{
    using System;
    using System.Linq;

    using SocialNetwork.Models;
    using SocialNetwork.Services.Models.Users;

    public class CommentViewModel
    {
        public int Id { get; set; }

        public UserViewModelMinified Author { get; set; }

        public int LikesCount { get; set; }

        public string CommentContent { get; set; }

        public DateTime Date { get; set; }

        public bool Liked { get; set; }

        public static CommentViewModel Create(Comment c, ApplicationUser currentUser)
        {
            return new CommentViewModel()
            {
                Id = c.Id,
                Author = UserViewModelMinified.Create(c.Author),
                LikesCount = c.Likes.Count,
                CommentContent = c.Content,
                Date = c.Date,
                Liked = c.Likes
                    .Any(l => l.UserId == currentUser.Id)
            };
        }
    }
}