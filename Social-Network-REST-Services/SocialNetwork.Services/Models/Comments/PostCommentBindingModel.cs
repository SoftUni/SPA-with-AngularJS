namespace SocialNetwork.Services.Models
{
    using System.ComponentModel.DataAnnotations;

    public class PostCommentBindingModel
    {
        [Required]
        [MinLength(2)]
        public string CommentContent { get; set; }
    }
}