namespace SocialNetwork.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    
    using Microsoft.AspNet.Identity.EntityFramework;

    using SocialNetwork.Data.Contracts;
    using SocialNetwork.Models;

    public class SocialNetworkData : ISocialNetworkData
    {
        private readonly DbContext context;

        private readonly IDictionary<Type, object> repositories;

        public SocialNetworkData()
            : this(new ApplicationDbContext())
        {
        }

        public SocialNetworkData(DbContext context)
        {
            this.context = context;
            this.repositories = new Dictionary<Type, object>();
        }

        public IRepository<ApplicationUser> Users
        {
            get
            {
                return this.GetRepository<ApplicationUser>();
            }
        }

        public IRepository<IdentityRole> UserRoles
        {
            get
            {
                return this.GetRepository<IdentityRole>();
            }
        }

        public IRepository<Post> Posts
        {
            get
            {
                return this.GetRepository<Post>();
            }
        }

        public IRepository<Comment> Comments
        {
            get
            {
                return this.GetRepository<Comment>();
            }
        }

        public IRepository<UserSession> UserSessions
        {
            get
            {
                return this.GetRepository<UserSession>();
            }
        }

        public IRepository<FriendRequest> FriendRequests
        {
            get
            {
                return this.GetRepository<FriendRequest>();
            }
        }

        public IRepository<CommentLike> CommentLikes
        {
            get
            {
                return this.GetRepository<CommentLike>();
            }
        }

        public IRepository<PostLike> PostLikes
        {
            get
            {
                return this.GetRepository<PostLike>();
            }
        }

        public int SaveChanges()
        {
            return this.context.SaveChanges();
        }

        private IRepository<T> GetRepository<T>() where T : class
        {
            if (!this.repositories.ContainsKey(typeof(T)))
            {
                var type = typeof(EfRepository<T>);
                this.repositories.Add(typeof(T), Activator.CreateInstance(type, this.context));
            }

            return (IRepository<T>)this.repositories[typeof(T)];
        }
    }
}
