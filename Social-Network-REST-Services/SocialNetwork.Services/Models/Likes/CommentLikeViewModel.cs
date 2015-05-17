namespace SocialNetwork.Services.Models.Likes
{
    using SocialNetwork.Models;
    using SocialNetwork.Services.Models.Users;

    public class CommentLikeViewModel
    {
        public int CommentId { get; set; }

        public UserViewModelMinified User { get; set; }

        public static CommentLikeViewModel Create(CommentLike commentLike)
        {
            return new CommentLikeViewModel()
            {
                CommentId = commentLike.CommentId,
                User = UserViewModelMinified.Create(commentLike.User)
            };
        }
    }
}