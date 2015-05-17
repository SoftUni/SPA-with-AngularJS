namespace SocialNetwork.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Comment
    {
        private ICollection<CommentLike> likes;

        public Comment()
        {
            this.likes = new List<CommentLike>();
        }
        
        [Key]
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; }

        public virtual Post Post { get; set; }

        [Required]
        public string AuthorId { get; set; }

        public virtual ApplicationUser Author { get; set; }

        [Required]
        [MinLength(2)]
        public string Content { get; set; }

        public virtual ICollection<CommentLike> Likes
        {
            get { return this.likes; }
            set { this.likes = value; }
        }

        [Required]
        public DateTime Date { get; set; }
    }
}
