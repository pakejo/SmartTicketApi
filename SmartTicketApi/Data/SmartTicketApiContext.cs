using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using SmartTicketApi.Models;

namespace SmartTicketApi.Data
{
    public class SmartTicketApiContext : IdentityDbContext<ApplicationUser>
    {
        public SmartTicketApiContext(DbContextOptions<SmartTicketApiContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Event>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Entity<Event>()
                .HasKey(e => e.Id);

            builder.Entity<Event>()
                .HasOne(e => e.Promoter)
                .WithMany(u => u.Events)
                .HasForeignKey(e => e.PromoterId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Sale>()
                .HasKey(s => s.Id);

            builder.Entity<Sale>()
                .Property(s => s.Id)
                .ValueGeneratedOnAdd();

            builder.Entity<Sale>()
                .HasOne(s => s.Event)
                .WithMany(e => e.Sales)
                .HasForeignKey(s => s.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.Entity<Sale>()
                .HasOne(s => s.User)
                .WithMany(u => u.Sales)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }

        public DbSet<Models.Event> Event { get; set; }
        public DbSet<Models.Sale> Sale { get; set; }
    }
}