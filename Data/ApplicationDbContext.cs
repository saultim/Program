using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TextbookExchange.Models;

namespace TextbookExchange.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<Exchange> Exchanges { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Complaint> Complaints { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Book>()
                .HasOne(b => b.User)
                .WithMany(u => u.Books)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Exchange>()
    .HasOne(e => e.OfferedBook)
    .WithMany()
    .HasForeignKey(e => e.OfferedBookId)
    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Exchange>()
                .HasOne(e => e.RequestedBook)
                .WithMany()
                .HasForeignKey(e => e.RequestedBookId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Exchange>()
                .HasOne(e => e.Initiator)
                .WithMany()
                .HasForeignKey(e => e.InitiatorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Exchange>()
                .HasOne(e => e.TargetUser)
                .WithMany()
                .HasForeignKey(e => e.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Review>()
    .HasOne(r => r.Author)
    .WithMany()
    .HasForeignKey(r => r.AuthorId)
    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.TargetUser)
                .WithMany()
                .HasForeignKey(r => r.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Complaint>()
                .HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Complaint>()
                .HasOne(c => c.TargetUser)
                .WithMany()
                .HasForeignKey(c => c.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}