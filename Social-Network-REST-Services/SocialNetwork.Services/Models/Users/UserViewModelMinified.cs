namespace SocialNetwork.Services.Models.Users
{
    using SocialNetwork.Models;

    public class UserViewModelMinified
    {
        public string Id { get; set; }

        public string ProfileImageData { get; set; }

        public string Username { get; set; }

        public string Name { get; set; }

        public Gender Gender { get; set; }

        public static UserViewModelMinified Create(ApplicationUser user)
        {
            return new UserViewModelMinified()
            {
                Id = user.Id,
                Name = user.Name,
                Username = user.UserName,
                Gender = user.Gender,
                ProfileImageData = user.ProfileImageDataMinified
            };
        }
    }
}