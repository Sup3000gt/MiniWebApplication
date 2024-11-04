using Microsoft.EntityFrameworkCore;
using MiniWebApplication.Models;
using System.Collections.Generic;

namespace MiniWebApplication.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        public DbSet<PaymentCard> PaymentCards { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<ProductPopularity> ProductPopularities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Order>()
                .HasOne(o => o.PaymentCard)
                .WithMany()
                .HasForeignKey(o => o.PaymentCardId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes

            // Configure decimal properties for precision and scale
            modelBuilder.Entity<PaymentCard>()
                .Property(p => p.Balance)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Order>()
                .HasOne(o => o.PaymentCard)
                .WithMany()
                .HasForeignKey(o => o.PaymentCardId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.Price)
                .HasColumnType("decimal(18, 2)");
        }

    }
}
