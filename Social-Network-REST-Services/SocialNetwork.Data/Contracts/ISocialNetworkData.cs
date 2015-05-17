namespace SocialNetwork.Data.Contracts
{
    using Microsoft.AspNet.Identity.EntityFramework;

    using SocialNetwork.Models;    

    public interface ISocialNetworkData
    {
        IRepository<ApplicationUser> Users { get; }

        IRepository<IdentityRole> UserRoles { get; }

        IRepository<Post> Posts { get; }

        IRepository<Comment> Comments { get; }

        IRepository<UserSession> UserSessions { get; }

        IRepository<FriendRequest> FriendRequests { get; }

        IRepository<PostLike> PostLikes { get; }

        IRepository<CommentLike> CommentLikes { get; } 

        int SaveChanges();
    }
}
