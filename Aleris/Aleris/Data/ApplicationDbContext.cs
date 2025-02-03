using Aleris.Models;
using Microsoft.EntityFrameworkCore;

namespace Aleris.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyMember> CompanyMembers { get; set; }
        public DbSet<CompanySettings> CompanySettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Връзка Many-to-Many за CompanyMembers
            modelBuilder.Entity<CompanyMember>()
                .HasOne(cm => cm.User)
                .WithMany(u => u.CompanyMemberships)
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompanyMember>()
                .HasOne(cm => cm.Company)
                .WithMany(c => c.TeamMembers)
                .HasForeignKey(cm => cm.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
