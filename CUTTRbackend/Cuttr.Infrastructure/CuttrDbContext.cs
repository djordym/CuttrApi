using Cuttr.Infrastructure.Common;
using Cuttr.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Infrastructure
{
    public class CuttrDbContext : DbContext
    {
        public CuttrDbContext(DbContextOptions<CuttrDbContext> options)
            : base(options)
        {
        }

        // DbSet properties for each entity
        public DbSet<UserEF> Users { get; set; }
        public DbSet<PlantEF> Plants { get; set; }
        public DbSet<SwipeEF> Swipes { get; set; }
        public DbSet<MatchEF> Matches { get; set; }
        public DbSet<MessageEF> Messages { get; set; }
        public DbSet<ReportEF> Reports { get; set; }
        public DbSet<UserPreferencesEF> UserPreferences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Call base method
            base.OnModelCreating(modelBuilder);

            // ----------------------------
            // UserEF Configuration
            // ----------------------------
            modelBuilder.Entity<UserEF>(entity =>
            {
                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(u => u.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Bio)
                    .HasMaxLength(500);

                // Relationships
                entity.HasMany(u => u.Plants)
                    .WithOne(p => p.User)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(u => u.Preferences)
                    .WithOne(p => p.User)
                    .HasForeignKey<UserPreferencesEF>(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.ReportsMade)
                    .WithOne(r => r.ReporterUser)
                    .HasForeignKey(r => r.ReporterUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.ReportsReceived)
                    .WithOne(r => r.ReportedUser)
                    .HasForeignKey(r => r.ReportedUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.SentMessages)
                    .WithOne(m => m.SenderUser)
                    .HasForeignKey(m => m.SenderUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ----------------------------
            // PlantEF Configuration
            // ----------------------------
            modelBuilder.Entity<PlantEF>(entity =>
            {
                entity.Property(p => p.SpeciesName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(p => p.Category)
                    .HasMaxLength(100);

                // Relationships
                entity.HasOne(p => p.User)
                    .WithMany(u => u.Plants)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ----------------------------
            // SwipeEF Configuration
            // ----------------------------
            modelBuilder.Entity<SwipeEF>(entity =>
            {
                entity.HasKey(s => s.SwipeId);

                entity.HasOne(s => s.SwiperPlant)
                    .WithMany()
                    .HasForeignKey(s => s.SwiperPlantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.SwipedPlant)
                    .WithMany()
                    .HasForeignKey(s => s.SwipedPlantId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Unique constraint to prevent duplicate swipes
                entity.HasIndex(s => new { s.SwiperPlantId, s.SwipedPlantId })
                    .IsUnique();
            });

            // ----------------------------
            // MatchEF Configuration
            // ----------------------------
            modelBuilder.Entity<MatchEF>(entity =>
            {
                entity.HasKey(m => m.MatchId);

                // Relationships with Plants
                entity.HasOne(m => m.Plant1)
                    .WithMany()
                    .HasForeignKey(m => m.PlantId1)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Plant2)
                    .WithMany()
                    .HasForeignKey(m => m.PlantId2)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relationships with Users
                entity.HasOne(m => m.User1)
                    .WithMany()
                    .HasForeignKey(m => m.UserId1)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.User2)
                    .WithMany()
                    .HasForeignKey(m => m.UserId2)
                    .OnDelete(DeleteBehavior.Restrict);

                // Unique constraint to prevent duplicate matches
                entity.HasIndex(m => new { m.PlantId1, m.PlantId2 })
                    .IsUnique();

                // Ensure that PlantId1 is always less than PlantId2
                entity.HasCheckConstraint("CK_MatchEF_PlantIdOrder", "[PlantId1] < [PlantId2]");
            });

            // ----------------------------
            // MessageEF Configuration
            // ----------------------------
            modelBuilder.Entity<MessageEF>(entity =>
            {
                entity.HasKey(m => m.MessageId);

                entity.Property(m => m.MessageText)
                    .IsRequired();

                entity.HasOne(m => m.Match)
                    .WithMany(mt => mt.Messages)
                    .HasForeignKey(m => m.MatchId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.SenderUser)
                    .WithMany(u => u.SentMessages)
                    .HasForeignKey(m => m.SenderUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ----------------------------
            // ReportEF Configuration
            // ----------------------------
            modelBuilder.Entity<ReportEF>(entity =>
            {
                entity.HasKey(r => r.ReportId);

                entity.Property(r => r.Reason)
                    .IsRequired();

                entity.HasOne(r => r.ReporterUser)
                    .WithMany(u => u.ReportsMade)
                    .HasForeignKey(r => r.ReporterUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.ReportedUser)
                    .WithMany(u => u.ReportsReceived)
                    .HasForeignKey(r => r.ReportedUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ----------------------------
            // UserPreferencesEF Configuration
            // ----------------------------
            modelBuilder.Entity<UserPreferencesEF>(entity =>
            {
                entity.HasKey(p => p.UserId);

                entity.HasOne(p => p.User)
                    .WithOne(u => u.Preferences)
                    .HasForeignKey<UserPreferencesEF>(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(p => p.PreferredCategories)
                    .HasConversion(
                        v => v, // Assuming it's a JSON string
                        v => v); // Adjust conversion if storing as a collection
            });

            // ----------------------------
            // Additional Configurations
            // ----------------------------

            // Geospatial Indexing on Users for Location-based queries
            modelBuilder.Entity<UserEF>()
                .HasIndex(u => new { u.LocationLatitude, u.LocationLongitude });

            // Automatically set default values for CreatedAt and UpdatedAt
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;

                if (typeof(ICreatedAt).IsAssignableFrom(clrType))
                {
                    modelBuilder.Entity(clrType)
                        .Property("CreatedAt")
                        .HasDefaultValueSql("GETUTCDATE()");
                }

                if (typeof(IUpdatedAt).IsAssignableFrom(clrType))
                {
                    modelBuilder.Entity(clrType)
                        .Property("UpdatedAt")
                        .HasDefaultValueSql("GETUTCDATE()");
                }
            }
        }

        // Overriding SaveChanges to update timestamps
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IUpdatedAt && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                ((IUpdatedAt)entry.Entity).UpdatedAt = DateTime.UtcNow;

                if (entry.State == EntityState.Added && entry.Entity is ICreatedAt)
                {
                    ((ICreatedAt)entry.Entity).CreatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
