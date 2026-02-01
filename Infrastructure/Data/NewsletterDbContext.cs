using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NewsletterApp.Infrastructure.Data
{
    public class NewsletterDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public NewsletterDbContext(DbContextOptions<NewsletterDbContext> options)
            : base(options)
        {
        }

        public DbSet<Subscriber> Subscribers { get; set; }
        public DbSet<SubscriptionHistory> SubscriptionHistories { get; set; }
        public DbSet<LookupCategory> LookupCategories { get; set; }
        public DbSet<LookupItem> LookupItems { get; set; }
        public DbSet<Newsletter> Newsletters { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Lookup Configuration

            modelBuilder.Entity<LookupCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<LookupItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Value).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Label).IsRequired().HasMaxLength(100);
                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.CategoryId);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            #endregion

            #region Subscriber Configuration

            modelBuilder.Entity<Subscriber>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.Property(e => e.CommunicationMethods)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                    )
                    .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

                entity.Property(e => e.Interests)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                    )
                    .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));
            });

            #endregion

            #region Global Query Filters for Soft Delete

            modelBuilder.Entity<Newsletter>().HasQueryFilter(e => !e.IsDeleted);

            #endregion
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
            {
                switch (entry.State)
                {
                    case EntityState.Deleted:
                        // If it's already soft-deleted, we allow permanent deletion
                        if (!entry.Entity.IsDeleted)
                        {
                            entry.State = EntityState.Modified;
                            entry.Entity.IsDeleted = true;
                            entry.Entity.DeletedAt = DateTime.UtcNow;
                        }
                        break;
                }
            }

            foreach (var entry in ChangeTracker.Entries<IAuditEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        if (string.IsNullOrWhiteSpace(entry.Entity.CreatedBy)) entry.Entity.CreatedBy = "System";
                        
                        // Fix: Initialize Updated fields on creation to satisfy NOT NULL constraints
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        if (string.IsNullOrWhiteSpace(entry.Entity.UpdatedBy)) entry.Entity.UpdatedBy = "System";
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        if (string.IsNullOrWhiteSpace(entry.Entity.UpdatedBy)) entry.Entity.UpdatedBy = "System";
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
