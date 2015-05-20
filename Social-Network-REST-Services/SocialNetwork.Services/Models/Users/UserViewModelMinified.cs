using System.Linq;
using System.Linq.Expressions;
using Microsoft.Ajax.Utilities;

namespace SocialNetwork.Services.Models.Users
{
    using SocialNetwork.Models;
using System;

    public class UserViewModelMinified
    {
        //public static Expression<Func<ApplicationUser, UserViewModelMinified>> ViewModel
        //{
        //    get
        //    {
        //        return user => new UserViewModelMinified()
        //        {
        //            Id = user.Id,
        //            Name = user.Name,
        //            Username = user.UserName,
        //            Gender = user.Gender,
        //            ProfileImageData = user.ProfileImageDataMinified
        //        };
        //    }
        //}

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