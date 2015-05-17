namespace SocialNetwork.Services.Models.Posts
{
    using System.ComponentModel.DataAnnotations;

    public class AddPostBindingModel
    {
        [Required]
        public string PostContent { get; set; }

        [Required]
        public string Username { get; set; }
    }
}