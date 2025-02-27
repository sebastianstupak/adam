using ADAM.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ADAM.Domain;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    internal DbSet<MerchantOffer> MerchantOffers { get; set; }

    internal DbSet<User> Users { get; set; }

    internal DbSet<Subscription> Subscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Subscription>()
            .Property(e => e.Type)
            .HasConversion<string>();
    }
}