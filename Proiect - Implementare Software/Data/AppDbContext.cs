using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Proiect_Implementare_Software.Models;

namespace Proiect_Implementare_Software.Data
{
    // Schimbăm DbContext în IdentityDbContext<IdentityUser>
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Person> Persons { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Ride> Rides { get; set; }
        public DbSet<UserOrdersRide> UserOrdersRides { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<PromoCode> PromoCodes { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IMPORTANT: apelăm întâi baza pentru a configura Identity
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserOrdersRide>()
                .HasKey(uor => new { uor.UserID, uor.RideID });

            modelBuilder.Entity<UserOrdersRide>()
                .HasOne(uor => uor.User)
                .WithMany(p => p.OrderedRides)
                .HasForeignKey(uor => uor.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserOrdersRide>()
                .HasOne(uor => uor.Ride)
                .WithMany(r => r.UsersOrdered)
                .HasForeignKey(uor => uor.RideID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Driver)
                .WithOne()
                .HasForeignKey<Vehicle>(v => v.DriverID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PromoCode>()
                .HasOne(p => p.Person)
                .WithMany(p => p.PromoCodes)
                .HasForeignKey(p => p.PersonID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(p => p.Payments)
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Ride)
                .WithOne(r => r.Payment)
                .HasForeignKey<Payment>(p => p.RideID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany(p => p.GivenRatings)
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Driver)
                .WithMany(p => p.ReceivedRatings)
                .HasForeignKey(r => r.DriverID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Ride>()
                .HasOne(r => r.Driver)
                .WithMany(p => p.Drives)
                .HasForeignKey(r => r.DriverID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ride>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Rides)
                .HasForeignKey(r => r.ProductID)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Person>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(p => p.IdentityUserId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
