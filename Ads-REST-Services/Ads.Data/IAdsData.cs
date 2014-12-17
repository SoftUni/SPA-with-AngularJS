namespace Ads.Data
{
    using Ads.Models;

    public interface IAdsData
    {
        IRepository<ApplicationUser> Users { get; }

        IRepository<Advertisement> Ads { get; }

        IRepository<Town> Towns { get; }

        IRepository<Category> Categories { get; }

        int SaveChanges();
    }
}
