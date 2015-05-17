using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using SocialNetwork.Models;

namespace SocialNetwork.Services.Models
{
    public class EditCommentBindingModel
    {
        public int Id { get; set; }

        [Required]
        [MinLength(2)]
        public string CommentContent { get; set; }
    }
}