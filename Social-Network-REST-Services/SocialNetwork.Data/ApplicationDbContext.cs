namespace SocialNetwork.Data
{
    using System.Data.Entity;

    using Microsoft.AspNet.Identity.EntityFramework;

    using SocialNetwork.Models;
    using SocialNetwork.Data.Migrations;

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("name=SocialNetwork")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>());
        }

        public IDbSet<Post> Posts { get; set; }

        public IDbSet<Comment> Comments { get; set; }

        public IDbSet<UserSession> Sessions { get; set; }

        public IDbSet<FriendRequest> FriendRequests { get; set; }

        public IDbSet<PostLike> PostLikes { get; set; }

        public IDbSet<CommentLike> CommentLikes { get; set; } 

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Post>()
                .HasRequired<ApplicationUser>(p => p.Author)
                .WithMany(a => a.OwnPosts)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Post>()
                .HasRequired<ApplicationUser>(p => p.WallOwner)
                .WithMany(o => o.WallPosts)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Comment>()
                .HasRequired<ApplicationUser>(p => p.Author)
                .WithMany(a => a.Comments)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<FriendRequest>()
                .HasRequired<ApplicationUser>(r => r.From)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<FriendRequest>()
                .HasRequired<ApplicationUser>(r => r.To)
                .WithMany(u => u.FriendRequests)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany<ApplicationUser>(u => u.Friends)
                .WithMany()
                .Map(uu =>
                {
                    uu.MapLeftKey("UserId");
                    uu.MapRightKey("FriendId");
                    uu.ToTable("UserFriends");
                });

            base.OnModelCreating(modelBuilder);
        }
    }
}
