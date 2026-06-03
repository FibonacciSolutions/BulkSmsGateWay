using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OmniRoute.Core.Entities;

namespace OmniRoute.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<MessageTemplate> MessageTemplates { get; set; }
        public DbSet<MessageLog> MessageLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Tenant Configuration
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(t => t.TenantId);
                entity.Property(t => t.AccountBalance).HasPrecision(18, 4);
            });

            // 2. API Key Configuration
            modelBuilder.Entity<ApiKey>(entity =>
            {
                entity.HasKey(a => a.ApiKeyId);
                entity.HasOne(a => a.Tenant)
                      .WithMany(t => t.ApiKeys)
                      .HasForeignKey(a => a.TenantId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 3. Message Template Configuration
            modelBuilder.Entity<MessageTemplate>(entity =>
            {
                entity.HasKey(m => m.TemplateId);
                entity.HasOne(m => m.Tenant)
                      .WithMany()
                      .HasForeignKey(m => m.TenantId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 4. Message Log Configuration
            modelBuilder.Entity<MessageLog>(entity =>
            {
                entity.HasKey(m => m.MessageId);
                entity.Property(m => m.CostCharged).HasPrecision(18, 4);

                entity.HasOne(m => m.Tenant)
                      .WithMany(t => t.MessageLogs)
                      .HasForeignKey(m => m.TenantId)
                      .OnDelete(DeleteBehavior.Restrict); // Prevents accidental cascading loss of financial logs
            });
        }
    }
}