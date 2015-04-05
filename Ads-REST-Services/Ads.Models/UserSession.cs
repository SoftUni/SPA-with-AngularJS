namespace Ads.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class UserSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string OwnerUserId { get; set; }

        public virtual ApplicationUser OwnerUser { get; set; }

        [Required]
        [MaxLength(500)]
        public string AuthToken { get; set; }

        [Required]
        public DateTime ExpirationDateTime { get; set; }
    }
}
