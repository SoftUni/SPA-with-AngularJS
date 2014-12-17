namespace Ads.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Advertisement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MinLength(1)]
        public string Title { get; set; }

        [Required]
        [MinLength(1)]
        public string Text { get; set; }

        public string ImageDataURL { get; set; }

        [Required]
        public string OwnerId { get; set; }

        public virtual ApplicationUser Owner { get; set; }

        public int? CategoryId { get; set; }
        
        public virtual Category Category { get; set; }

        public int? TownId { get; set; }

        public virtual Town Town { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public AdvertisementStatus Status { get; set; }
    }
}
