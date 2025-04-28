using Microsoft.EntityFrameworkCore;
using URLShortener.API.Models;

namespace URLShortener.API.Data
{
    public class UrlContext : DbContext
    {
        public UrlContext(DbContextOptions<UrlContext> options) : base(options)
        {
        }

        public DbSet<UrlModel> Urls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UrlModel>()
                .HasIndex(u => u.ShortUrl)
                .IsUnique();
        }
    }
} 