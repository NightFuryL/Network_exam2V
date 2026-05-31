using DatabaseLibrary.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLibrary.Core;

public class CheckersDbContext : DbContext
{
    public DbSet<UserEntity> Users => Set<UserEntity>();

    public DbSet<GameHistory> Games => Set<GameHistory>();

    public CheckersDbContext()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer(
            @"Data Source=NIGHTFURY\LEVMSSQLSERVER;Initial Catalog=CheckersDBB;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(u => u.Name)
                .IsUnique();
        });
        modelBuilder.Entity<GameHistory>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Moves).IsRequired();

            entity.HasOne<UserEntity>()
                .WithMany()
                .HasForeignKey(g => g.WhiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<UserEntity>()
                .WithMany()
                .HasForeignKey(g => g.BlackId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<UserEntity>()
                .WithMany()
                .HasForeignKey(g => g.WinnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

}