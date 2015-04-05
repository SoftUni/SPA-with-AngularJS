namespace Ads.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    public class ApplicationUser : IdentityUser
    {
        private ICollection<Advertisement> ads;

        public ApplicationUser()
        {
            this.ads = new HashSet<Advertisement>();
        }

        [Required]
        public string Name { get; set; }

        public int? TownId { get; set; }

        public virtual Town Town { get; set; }

        public virtual ICollection<Advertisement> Ads
        {
            get { return this.ads; }
            set { this.ads = value; }
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(
            UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);

            // Add custom user claims here
            return userIdentity;
        }
    }
}
