namespace SocialNetwork.Models
{
    using System.ComponentModel.DataAnnotations;

    public class CommentLike
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CommentId { get; set; }

        public virtual Comment Comment { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
