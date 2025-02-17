﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cuttr.Infrastructure.Common;
using Cuttr.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cuttr.Infrastructure
{
    public class CuttrDbContext : DbContext
    {
        public CuttrDbContext(DbContextOptions<CuttrDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserEF> Users { get; set; }
        public DbSet<PlantEF> Plants { get; set; }
        public DbSet<SwipeEF> Swipes { get; set; }
        public DbSet<MatchEF> Matches { get; set; }
        public DbSet<MessageEF> Messages { get; set; }
        public DbSet<ReportEF> Reports { get; set; }
        public DbSet<UserPreferencesEF> UserPreferences { get; set; }
        public DbSet<RefreshTokenEF> RefreshTokens { get; set; }
        public DbSet<ConnectionEF> Connections { get; set; }
        public DbSet<TradeProposalEF> TradeProposals { get; set; }
        // New DbSet for the join entity
        public DbSet<TradeProposalPlantEF> TradeProposalPlants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
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

                entity.Property(u => u.Location)
                    .HasColumnType("geography");

                entity.HasMany(u => u.RefreshTokens)
                    .WithOne(rt => rt.User)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RefreshToken configuration
            modelBuilder.Entity<RefreshTokenEF>(entity =>
            {
                entity.HasKey(e => e.RefreshTokenId);

                entity.Property(e => e.TokenHash)
                    .IsRequired()
                    .HasMaxLength(512);

                entity.Property(e => e.ExpiresAt)
                    .IsRequired();

                entity.Property(e => e.IsRevoked)
                    .HasDefaultValue(false);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Plant configuration
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

            // Swipe configuration
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

            // Match configuration
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
                    .WithMany(c => c.Matches)
                    .HasForeignKey(m => m.ConnectionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(m => new { m.PlantId1, m.PlantId2 })
                    .IsUnique();
            });

            // Connection configuration
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

                entity.HasMany(c => c.Messages)
                    .WithOne(m => m.Connection)
                    .HasForeignKey(m => m.ConnectionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.TradeProposals)
                    .WithOne(tp => tp.Connection)
                    .HasForeignKey(tp => tp.ConnectionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Message configuration
            modelBuilder.Entity<MessageEF>(entity =>
            {
                entity.HasKey(m => m.MessageId);

                entity.Property(m => m.MessageText)
                    .IsRequired();

                entity.HasOne(m => m.Connection)
                    .WithMany(conn => conn.Messages)
                    .HasForeignKey(m => m.ConnectionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.SenderUser)
                    .WithMany(u => u.SentMessages)
                    .HasForeignKey(m => m.SenderUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TradeProposal configuration
            modelBuilder.Entity<TradeProposalEF>(entity =>
            {
                entity.HasKey(tp => tp.TradeProposalId);

                // New: store who created the proposal
                entity.Property(tp => tp.ProposalOwnerUserId)
                      .IsRequired();

                // New: default both confirmations to false
                entity.Property(tp => tp.OwnerCompletionConfirmed)
                      .HasDefaultValue(false);
                entity.Property(tp => tp.ResponderCompletionConfirmed)
                      .HasDefaultValue(false);

                entity.Property(tp => tp.TradeProposalStatus)
                      .IsRequired();

                entity.HasOne(tp => tp.Connection)
                      .WithMany(c => c.TradeProposals)
                      .HasForeignKey(tp => tp.ConnectionId)
                      .OnDelete(DeleteBehavior.Cascade);
                // Note: The collection navigation to TradeProposalPlants is configured below in its own mapping.
            });

            // New TradeProposalPlant configuration (join entity)
            modelBuilder.Entity<TradeProposalPlantEF>(entity =>
            {
                entity.HasKey(tpp => new { tpp.TradeProposalId, tpp.PlantId });

                entity.HasOne(tpp => tpp.TradeProposal)
                    .WithMany(tp => tp.TradeProposalPlants)
                    .HasForeignKey(tpp => tpp.TradeProposalId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tpp => tpp.Plant)
                    .WithMany()
                    .HasForeignKey(tpp => tpp.PlantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Report configuration
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

            // UserPreferences configuration
            modelBuilder.Entity<UserPreferencesEF>(entity =>
            {
                entity.HasKey(p => p.UserId);

                entity.HasOne(p => p.User)
                    .WithOne(u => u.Preferences)
                    .HasForeignKey<UserPreferencesEF>(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Automatically set default values for CreatedAt and UpdatedAt properties on entities implementing ICreatedAt and IUpdatedAt.
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
