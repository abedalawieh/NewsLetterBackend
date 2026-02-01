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
        public DbSet<SubscriberInterest> SubscriberInterests { get; set; }
        public DbSet<SubscriberCommunicationMethod> SubscriberCommunicationMethods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


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



            modelBuilder.Entity<Subscriber>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<SubscriberInterest>(entity =>
            {
                entity.HasKey(e => new { e.SubscriberId, e.LookupItemId });
                entity.HasOne(e => e.Subscriber)
                    .WithMany(s => s.Interests)
                    .HasForeignKey(e => e.SubscriberId);
                entity.HasOne(e => e.LookupItem)
                    .WithMany()
                    .HasForeignKey(e => e.LookupItemId);
                entity.HasQueryFilter(e => !e.LookupItem.IsDeleted);
            });

            modelBuilder.Entity<SubscriberCommunicationMethod>(entity =>
            {
                entity.HasKey(e => new { e.SubscriberId, e.LookupItemId });
                entity.HasOne(e => e.Subscriber)
                    .WithMany(s => s.CommunicationMethods)
                    .HasForeignKey(e => e.SubscriberId);
                entity.HasOne(e => e.LookupItem)
                    .WithMany()
                    .HasForeignKey(e => e.LookupItemId);
                entity.HasQueryFilter(e => !e.LookupItem.IsDeleted);
            });



            modelBuilder.Entity<Newsletter>().HasQueryFilter(e => !e.IsDeleted);

        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
            {
                switch (entry.State)
                {
                    case EntityState.Deleted:
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
