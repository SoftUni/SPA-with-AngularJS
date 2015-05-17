namespace SocialNetwork.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    public class ApplicationUser : IdentityUser
    {
        private ICollection<Post> wallPosts;
        private ICollection<Post> ownPosts;
        private ICollection<Comment> comments;
        private ICollection<FriendRequest> friendRequests;
        private ICollection<ApplicationUser> friends;

        public ApplicationUser()
        {
            this.wallPosts = new HashSet<Post>();
            this.ownPosts = new HashSet<Post>();
            this.comments = new HashSet<Comment>();
            this.friendRequests = new HashSet<FriendRequest>();
            this.friends = new HashSet<ApplicationUser>();
        }

        [Required]
        public string Name { get; set; }

        public string ProfileImageData { get; set; }

        public string ProfileImageDataMinified { get; set; }

        public string CoverImageData { get; set; }

        public Gender Gender { get; set; }

        public virtual ICollection<Post> WallPosts
        {
            get { return this.wallPosts; }
            set { this.wallPosts = value; }
        }

        public virtual ICollection<Post> OwnPosts
        {
            get { return this.ownPosts; }
            set { this.ownPosts = value; }
        }

        public virtual ICollection<Comment> Comments
        {
            get { return this.comments; }
            set { this.comments = value; }
        }

        public virtual ICollection<ApplicationUser> Friends
        {
            get { return this.friends; }
            set { this.friends = value; }
        }

        public virtual ICollection<FriendRequest> FriendRequests
        {
            get { return this.friendRequests; }
            set { this.friendRequests = value; }
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(
            UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);

            userIdentity.AddClaim(new Claim(ClaimTypes.Name, userIdentity.Name));
            userIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, this.Id));

            // Add custom user claims here
            return userIdentity;
        }
    }
}
