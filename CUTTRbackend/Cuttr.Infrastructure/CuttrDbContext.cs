using Cuttr.Infrastructure.Entities;
using Cuttr.Infrastructure.Common;
using Cuttr.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cuttr.Infrastructure
{
    public class CuttrDbContext : DbContext
    {
        public CuttrDbContext(DbContextOptions<CuttrDbContext> options)
            : base(options)
        {
        }

        // Existing DbSets
        public DbSet<UserEF> Users { get; set; }
        public DbSet<PlantEF> Plants { get; set; }
        public DbSet<SwipeEF> Swipes { get; set; }
        public DbSet<MatchEF> Matches { get; set; }
        public DbSet<MessageEF> Messages { get; set; }
        public DbSet<ReportEF> Reports { get; set; }
        public DbSet<UserPreferencesEF> UserPreferences { get; set; }
        public DbSet<RefreshTokenEF> RefreshTokens { get; set; }

        // New DbSets
        public DbSet<ConnectionEF> Connections { get; set; }
        public DbSet<TradeProposalEF> TradeProposals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===============================
            // UserEF
            // ===============================
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

                // Geo
                entity.Property(u => u.Location)
                      .HasColumnType("geography");
            });

            // ===============================
            // PlantEF
            // ===============================
            modelBuilder.Entity<PlantEF>(entity =>
            {
                entity.Property(p => p.SpeciesName)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.HasOne(p => p.User)
                      .WithMany(u => u.Plants)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===============================
            // SwipeEF
            // ===============================
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

                entity.HasIndex(s => new { s.SwiperPlantId, s.SwipedPlantId })
                      .IsUnique();
            });

            // ===============================
            // MatchEF
            // ===============================
            modelBuilder.Entity<MatchEF>(entity =>
            {
                entity.HasKey(m => m.MatchId);

                entity.HasOne(m => m.Plant1)
                      .WithMany()
                      .HasForeignKey(m => m.PlantId1)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Plant2)
                      .WithMany()
                      .HasForeignKey(m => m.PlantId2)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Connection)
                      .WithMany(m => m.Matches)
                      .HasForeignKey(m => m.ConnectionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(m => new { m.PlantId1, m.PlantId2 })
                      .IsUnique();

                entity.HasCheckConstraint("CK_MatchEF_PlantIdOrder", "[PlantId1] < [PlantId2]");
            });

            // ===============================
            // ConnectionEF (NEW)
            // ===============================
            modelBuilder.Entity<ConnectionEF>(entity =>
            {
                entity.HasKey(c => c.ConnectionId);

                entity.HasOne(c => c.User1)
                      .WithMany()
                      .HasForeignKey(c => c.UserId1)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.User2)
                      .WithMany()
                      .HasForeignKey(c => c.UserId2)
                      .OnDelete(DeleteBehavior.Restrict);

                // A Connection has many Messages
                entity.HasMany(c => c.Messages)
                      .WithOne(m => m.Connection)
                      .HasForeignKey(m => m.ConnectionId)
                      .OnDelete(DeleteBehavior.Cascade);

                // A Connection has many TradeProposals
                entity.HasMany(c => c.TradeProposals)
                      .WithOne(tp => tp.Connection)
                      .HasForeignKey(tp => tp.ConnectionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===============================
            // MessageEF (UPDATED)
            // ===============================
            modelBuilder.Entity<MessageEF>(entity =>
            {
                entity.HasKey(m => m.MessageId);

                entity.Property(m => m.MessageText)
                      .IsRequired();

                // Now referencing Connection, not Match
                entity.HasOne(m => m.Connection)
                      .WithMany(conn => conn.Messages)
                      .HasForeignKey(m => m.ConnectionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.SenderUser)
                      .WithMany(u => u.SentMessages)
                      .HasForeignKey(m => m.SenderUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===============================
            // TradeProposalEF (NEW)
            // ===============================
            modelBuilder.Entity<TradeProposalEF>(entity =>
            {
                entity.HasKey(tp => tp.TradeProposalId);

                entity.HasOne(tp => tp.Connection)
                      .WithMany(c => c.TradeProposals)
                      .HasForeignKey(tp => tp.ConnectionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===============================
            // ReportEF
            // ===============================
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

            // ===============================
            // UserPreferencesEF
            // ===============================
            modelBuilder.Entity<UserPreferencesEF>(entity =>
            {
                entity.HasKey(p => p.UserId);

                entity.HasOne(p => p.User)
                      .WithOne(u => u.Preferences)
                      .HasForeignKey<UserPreferencesEF>(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===============================
            // Apply Default Timestamps for ICreatedAt / IUpdatedAt
            // ===============================
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
