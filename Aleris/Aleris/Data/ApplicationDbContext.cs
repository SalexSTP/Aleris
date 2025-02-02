using Aleris.Models.Company;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aleris.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<CompanySettings> CompanySettings { get; set; }
        public DbSet<CompanyTeam> CompanyTeams { get; set; }
        public DbSet<CompanyMember> CompanyMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Company>()
                .HasOne(c => c.CompanySettings)
                .WithOne(cs => cs.Company)
                .HasForeignKey<Company>(cs => cs.Id);

            builder.Entity<Company>()
                .HasOne(c => c.CompanyTeam)
                .WithOne(ct => ct.Company)
                .HasForeignKey<CompanyTeam>(ct => ct.Id);

            builder.Entity<CompanyTeam>()
                .HasMany(ct => ct.CompanyMembers)
                .WithOne(cm => cm.CompanyTeam)
                .HasForeignKey(cm => cm.CompanyTeamId);

            builder.Entity<CompanySettings>()
                .Property(cs => cs.IsVatRegistered)
                .HasConversion<int>();

            builder.Entity<CompanySettings>()
                .Property(cs => cs.IsVatIncludedInPrices)
                .HasConversion<int>();

            builder.Entity<CompanySettings>()
                .Property(cs => cs.PricePrecision)
                .HasConversion<int>();

            builder.Entity<CompanySettings>()
                .Property(cs => cs.QuantityPrecision)
                .HasConversion<int>();

            builder.Entity<CompanySettings>()
                .Property(cs => cs.AllowNegativeQuantities)
                .HasConversion<int>();

            builder.Entity<CompanySettings>()
                .Property(cs => cs.MethodOfRevision)
                .HasConversion<int>();

            builder.Entity<CompanySettings>()
                .Property(cs => cs.AutoProduction)
                .HasConversion<int>();

            builder.Entity<CompanySettings>()
                .Property(cs => cs.WorkWithTraders)
                .HasConversion<int>();

            builder.Entity<CompanyMember>()
                .Property(cm => cm.Role)
                .HasConversion<int>();
        }
    }
}
