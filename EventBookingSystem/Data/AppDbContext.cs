using EventBookingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace EventBookingSystem.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //make the ondelete behavior to no action for all relationships to prevent cascade delete
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            //configure decimal properties to have a precision of 18 and scale of 2
            configurationBuilder.Properties<decimal>()
                .HavePrecision(18, 2);
        }

        DbSet<Event> Events { get; set; }
        DbSet<TicketType> TicketTypes { get; set; }
        DbSet<Booking> Bookings { get; set; }
        DbSet<BookingItem> BookingItems { get; set; }
        DbSet<Payment> Payments { get; set; }
        DbSet<Notification> Notifications { get; set; }
    }
}
