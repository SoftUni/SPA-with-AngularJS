namespace SocialNetwork.Data.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    using SocialNetwork.Models;
    using System.Threading;

    public sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            this.AutomaticMigrationsEnabled = true;
            this.AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            // Seed only if database is empty
            if (!context.Users.Any())
            {
                var users = this.SeedApplicationUsers(context);
                var posts = this.SeedPosts(context, users);
                var comments = this.SeedComments(context, posts, users);
                this.SeedLikes(context, posts, comments, users);
                this.SeedFriends(context, users);
                this.SeedFriendRequests(context, users);
            }
        }

        private void SeedLikes(ApplicationDbContext context, IList<Post> posts, IList<Comment> comments, IList<ApplicationUser> users)
        {
            var postLikes = new List<PostLike>()
            {
                new PostLike()
                {
                    Post = posts[0],
                    User = users[0]
                },
                new PostLike()
                {
                    Post = posts[0],
                    User = users[1]
                },
                new PostLike()
                {
                    Post = posts[1],
                    User = users[0]
                }
            };

            foreach (var postLike in postLikes)
            {
                context.PostLikes.Add(postLike);
            }

            var commentLikes = new List<CommentLike>()
            {
                new CommentLike()
                {
                    Comment = comments[0],
                    User = users[2]
                },
                new CommentLike()
                {
                    Comment = comments[3],
                    User = users[0]
                },
                new CommentLike()
                {
                    Comment = comments[3],
                    User = users[1]
                },
                new CommentLike()
                {
                    Comment = comments[5],
                    User = users[0]
                }
            };

            foreach (var commentLike in commentLikes)
            {
                context.CommentLikes.Add(commentLike);
            }

            context.SaveChanges();
        }

        private void SeedFriends(ApplicationDbContext context, IList<ApplicationUser> users)
        {
            users[0].Friends.Add(users[1]);
            users[1].Friends.Add(users[0]);

            users[1].Friends.Add(users[2]);
            users[2].Friends.Add(users[1]);

            context.SaveChanges();
        }

        private void SeedFriendRequests(ApplicationDbContext context, IList<ApplicationUser> users)
        {
            var requests = new List<FriendRequest>()
            {
                new FriendRequest()
                {
                    From = users[1],
                    To = users[3]
                },
                new FriendRequest()
                {
                    From = users[3],
                    To = users[0]
                },
                new FriendRequest()
                {
                    From = users[4],
                    To = users[0]
                }
            };

            foreach (var friendRequest in requests)
            {
                context.FriendRequests.Add(friendRequest);
            }

            context.SaveChanges();
        }

        private IList<ApplicationUser> SeedApplicationUsers(ApplicationDbContext context)
        {
            var users = new List<ApplicationUser>()
            {
                new ApplicationUser()
                {
                    UserName = "John",
                    Name = "John",
                    Email = "perhundel@abv.bg",
                    PhoneNumber = "+569 14215 13"
                },
                new ApplicationUser()
                {
                    UserName = "Jack",
                    Name = "Jack",
                    Email = "penio@yahoo.com",
                    PhoneNumber = "+359 12414 22"
                },
                new ApplicationUser()
                {
                    UserName = "Tanio",
                    Name = "Tanio",
                    Email = "kon@horsedick.com",
                    PhoneNumber = "+359 44123 213"
                },
                new ApplicationUser()
                {
                    UserName = "Pesho",
                    Name = "Peshaka",
                    Email = "pesho@peshev.com",
                    PhoneNumber = "+359 44123 213"
                },
                new ApplicationUser()
                {
                    UserName = "Maria",
                    Name = "Maria",
                    Email = "maria@gmail.com",
                    PhoneNumber = "+359 24123 23"
                }
            };

            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore)
            {
                PasswordValidator = new PasswordValidator
                {
                    RequiredLength = 2,
                    RequireNonLetterOrDigit = false,
                    RequireDigit = false,
                    RequireLowercase = false,
                    RequireUppercase = false
                }
            };

            foreach (var user in users)
            {
                var password = user.UserName;

                var userCreateResult = userManager.Create(user, password);
                if (!userCreateResult.Succeeded)
                {
                    throw new Exception(string.Join(Environment.NewLine, userCreateResult.Errors));
                }
            }

            context.SaveChanges();

            return users;
        }

        private IList<Post> SeedPosts(ApplicationDbContext context, IList<ApplicationUser> users)
        {
            var posts = new List<Post>()
            {
                new Post()
                {
                    Author = users[0],
                    Date = new DateTime(2015, 12, 22),
                    WallOwner = users[0],
                    Content = "Howdy folks!"
                },
                new Post()
                {
                    Author = users[1],
                    Date = new DateTime(2010, 10, 5),
                    WallOwner = users[0],
                    Content = "Welp..."
                },
                new Post() // John's friend has a post on his wall
                {
                    Author = users[2],
                    Date = new DateTime(2005, 11, 11),
                    WallOwner = users[1],
                    Content = "Friend wall"
                },
                new Post() // John's friend has a post on someone else's wall
                {
                    Author = users[1],
                    Date = new DateTime(2005, 11, 11),
                    WallOwner = users[2],
                    Content = "Other wall"
                },
                new Post() // Non-friend has post on own wall
                {
                    Author = users[2],
                    Date = new DateTime(2005, 11, 11),
                    WallOwner = users[2],
                    Content = "Restricted wall"
                }
            };

            for (int i = 0; i < 20; i++)
            {
                // Simulate time intervals
                Thread.Sleep(20);

                posts.Add(new Post()
                {
                    Author = users[2],
                    Date = DateTime.Now,
                    WallOwner = users[0],
                    Content = string.Format("#{0}", i)
                });
            }

            foreach (var post in posts)
            {
                context.Posts.Add(post);
            }

            context.SaveChanges();

            return posts;
        }

        private IList<Comment> SeedComments(ApplicationDbContext context, IList<Post> posts, IList<ApplicationUser> users)
        {
            var comments = new List<Comment>()
            {
                new Comment()
                {
                    Post = posts.FirstOrDefault(),
                    Author = users[0],
                    Content = "what the flying is this",
                    Date = new DateTime(2013, 4, 13)
                },
                new Comment()
                {
                    Post = posts.FirstOrDefault(),
                    Author = users[1],
                    Content = "oh wow test post",
                    Date = new DateTime(2013, 4, 13)
                },
                new Comment()
                {
                    Post = posts.FirstOrDefault(),
                    Author = users[2],
                    Content = "bag",
                    Date = new DateTime(2013, 4, 13)
                },
                new Comment()
                {
                    Post = posts.FirstOrDefault(),
                    Author = users[1],
                    Content = "dammit dad",
                    Date = new DateTime(2013, 4, 13)
                },
                new Comment()
                {
                    Post = posts[3],
                    Author = users[1],
                    Content = "foreign comment",
                    Date = new DateTime(2013, 4, 13)
                },
                new Comment()
                {
                    Post = posts[2],
                    Author = users[0],
                    Content = "unliked comment",
                    Date = new DateTime(2013, 4, 13)
                },
                new Comment()
                {
                    Post = posts[0],
                    Author = users[0],
                    Content = "own unliked comment",
                    Date = new DateTime(2013, 4, 13)
                }
            };

            foreach (var comment in comments)
            {
                context.Comments.Add(comment);
            }

            context.SaveChanges();

            return comments;
        }
    }
}
