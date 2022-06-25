using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using srk_website.Models;

namespace srk_website.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ContactModel> Contact { get; set; }
        public DbSet<ImageSlideShowModel> ImageSlideShow { get; set; }
        public DbSet<ServiceModel> Service { get; set; }
        public DbSet<AboutModel> About { get; set; }
    }
}