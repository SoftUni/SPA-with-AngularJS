namespace SocialNetwork.Services.Models.FriendRequests
{
    using SocialNetwork.Models;
    using SocialNetwork.Services.Models.Users;

    public class FriendRequestViewModel
    {
        public int Id { get; set; }

        public FriendRequestStatus Status { get; set; }

        public UserViewModelMinified User { get; set; }

        public static FriendRequestViewModel Create(FriendRequest request)
        {
            return new FriendRequestViewModel()
            {
                Id = request.Id,
                Status = request.Status,
                User = UserViewModelMinified.Create(request.From)
            };
        }
    }
}