using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MudBlazorCrmApp.Models;
using MudBlazorCrmApp.Shared.Models;

namespace MudBlazorCrmApp.Data
{
    // Note the explicit Identity generics: <ApplicationUser, IdentityRole, string>
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // --- Domain sets ---
        public DbSet<Customer> Customer => Set<Customer>();
        public DbSet<Address> Address => Set<Address>();
        public DbSet<ProductCategory> ProductCategory => Set<ProductCategory>();
        public DbSet<ServiceCategory> ServiceCategory => Set<ServiceCategory>();
        public DbSet<Contact> Contact => Set<Contact>();
        public DbSet<Opportunity> Opportunity => Set<Opportunity>();
        public DbSet<Lead> Lead => Set<Lead>();
        public DbSet<Product> Product => Set<Product>();
        public DbSet<Service> Service => Set<Service>();
        public DbSet<Sale> Sale => Set<Sale>();
        public DbSet<Vendor> Vendor => Set<Vendor>();
        public DbSet<SupportCase> SupportCase => Set<SupportCase>();
        public DbSet<TodoTask> TodoTask => Set<TodoTask>();
        public DbSet<Reward> Reward => Set<Reward>();

        // --- Timestamp helpers ---
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Customer &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var customer = (Customer)entry.Entity;
                if (entry.State == EntityState.Added)
                {
                    customer.CreatedDate ??= DateTime.UtcNow;
                }
                customer.ModifiedDate = DateTime.UtcNow;
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ---- MySQL-safe lengths for keys & indexed columns (utf8mb4) ----
            // 191 chars = 764 bytes at 4 bytes/char (<= 767 byte older index limit)
            const int L = 191;

            // Roles
            builder.Entity<IdentityRole>(b =>
            {
                b.Property(r => r.Id).HasColumnType($"varchar({L})");
                b.Property(r => r.Name).HasMaxLength(L).HasColumnType($"varchar({L})");
                b.Property(r => r.NormalizedName).HasMaxLength(L).HasColumnType($"varchar({L})");
                b.Property(r => r.ConcurrencyStamp).HasMaxLength(255).HasColumnType("varchar(255)");
            });

            // Users (your ApplicationUser)
            builder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.Id).HasColumnType($"varchar({L})");
                b.Property(u => u.UserName).HasMaxLength(L).HasColumnType($"varchar({L})");
                b.Property(u => u.NormalizedUserName).HasMaxLength(L).HasColumnType($"varchar({L})");
                b.Property(u => u.Email).HasMaxLength(255).HasColumnType("varchar(255)");
                b.Property(u => u.NormalizedEmail).HasMaxLength(L).HasColumnType($"varchar({L})");
                b.Property(u => u.ConcurrencyStamp).HasMaxLength(255).HasColumnType("varchar(255)");
                b.Property(u => u.SecurityStamp).HasMaxLength(255).HasColumnType("varchar(255)");
                b.Property(u => u.PhoneNumber).HasMaxLength(50).HasColumnType("varchar(50)");
            });

            // UserLogins (composite key)
            builder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.Property(l => l.LoginProvider).HasMaxLength(L).HasColumnType($"varchar({L})");
                b.Property(l => l.ProviderKey).HasMaxLength(L).HasColumnType($"varchar({L})");
                b.Property(l => l.ProviderDisplayName).HasMaxLength(255).HasColumnType("varchar(255)");
                b.Property(l => l.UserId).HasMaxLength(L).HasColumnType($"varchar({L})");
            });

            // UserTokens (composite key)
            builder.Entity<IdentityUserToken<string>>(b =>
            {
                b.Property(t => t.UserId).HasMaxLength(L).HasColumnType($"varchar({L})");
                b.Property(t => t.LoginProvider).HasMaxLength(L).HasColumnType($"varchar({L})");
                b.Property(t => t.Name).HasMaxLength(L).HasColumnType($"varchar({L})");
                // Value can be long; not indexed
                // (no need to force type here unless you want to)
            });

            // UserRoles (composite key)
            builder.Entity<IdentityUserRole<string>>(b =>
            {
                b.Property(r => r.UserId).HasMaxLength(L).HasColumnType($"varchar({L})");
                b.Property(r => r.RoleId).HasMaxLength(L).HasColumnType($"varchar({L})");
            });

            // Claims
            builder.Entity<IdentityRoleClaim<string>>(b =>
            {
                b.Property(rc => rc.RoleId).HasMaxLength(L).HasColumnType($"varchar({L})");
            });
            builder.Entity<IdentityUserClaim<string>>(b =>
            {
                b.Property(uc => uc.UserId).HasMaxLength(L).HasColumnType($"varchar({L})");
            });

            // ---- Pomelo-wide charset/collation (applies to all created tables) ----
            builder.HasCharSet("utf8mb4");
            builder.UseCollation("utf8mb4_0900_ai_ci");

            // If you have decimals/money fields in domain models, you can standardize here, e.g.:
            // builder.Entity<Product>().Property(p => p.Price).HasPrecision(19, 4);
        }
    }
}
