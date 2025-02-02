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
        public DbSet<CompanyMember> CompanyMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Company>()
                .HasOne(c => c.CompanySettings)
                .WithOne(cs => cs.Company)
                .HasForeignKey<CompanySettings>(cs => cs.CompanyId);

            builder.Entity<Company>()
                .HasMany(c => c.CompanyMembers)
                .WithOne(cm => cm.Company)
                .HasForeignKey(cm => cm.CompanyId);

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
