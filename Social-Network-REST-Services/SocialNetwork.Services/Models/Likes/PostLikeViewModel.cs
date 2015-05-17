namespace SocialNetwork.Services.Models.Likes
{
    using SocialNetwork.Models;
    using SocialNetwork.Services.Models.Users;

    public class PostLikeViewModel
    {
        public int PostId { get; set; }

        public UserViewModelMinified User { get; set; }

        public static PostLikeViewModel Create(PostLike postLike)
        {
            return new PostLikeViewModel()
            {
                PostId = postLike.PostId,
                User = UserViewModelMinified.Create(postLike.User)
            };
        }
    }
}