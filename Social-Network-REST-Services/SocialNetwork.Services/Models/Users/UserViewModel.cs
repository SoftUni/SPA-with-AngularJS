using System.Linq;

namespace SocialNetwork.Services.Models.Users
{
    using SocialNetwork.Models;

    public class UserViewModel
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string Name { get; set; }

        public string ProfileImageData { get; set; }

        public Gender Gender { get; set; }

        public string CoverImageData { get; set; }

        public bool IsFriend { get; set; }

        public bool HasPendingRequest { get; set; }

        public static UserViewModel Create(ApplicationUser user, ApplicationUser loggedUser)
        {
            return new UserViewModel()
            {
                Id = user.Id,
                Username = user.UserName,
                Name = user.Name,
                ProfileImageData = user.ProfileImageData,
                Gender = user.Gender,
                CoverImageData = user.CoverImageData,
                IsFriend = user.Friends
                    .Any(fr => fr.Id == loggedUser.Id),
                HasPendingRequest = user.FriendRequests
                    .Any(r => r.Status == FriendRequestStatus.Pending && 
                        (r.FromId == loggedUser.Id || r.ToId == loggedUser.Id))
            };
        }
    }
}