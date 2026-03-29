using Microsoft.EntityFrameworkCore;
using Scheduling.Domain.Entities;

namespace Scheduling.Infrastructure;

public class SchedulingDbContext : DbContext
{
    public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Person> People => Set<Person>();

    public DbSet<Schedule> Schedules => Set<Schedule>();

    public DbSet<ScheduleEntry> ScheduleEntries => Set<ScheduleEntry>();

    public DbSet<SwapHistory> SwapHistories => Set<SwapHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>(entity =>
        {
            entity.ToTable("People");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.ToTable("Schedules");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.Month, x.Year })
                .IsUnique();

            entity.Property(x => x.Month)
                .IsRequired();

            entity.Property(x => x.Year)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();
        });

        modelBuilder.Entity<ScheduleEntry>(entity =>
        {
            entity.ToTable("ScheduleEntries");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ScheduleId)
                .IsRequired();

            entity.Property(x => x.Date)
                .IsRequired();

            entity.Property(x => x.PersonId)
                .IsRequired();

            entity.HasOne<Schedule>()
                .WithMany()
                .HasForeignKey(x => x.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Person>()
                .WithMany()
                .HasForeignKey(x => x.PersonId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SwapHistory>(entity =>
        {
            entity.ToTable("SwapHistories");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ScheduleEntryId)
                .IsRequired();

            entity.Property(x => x.OldPersonId)
                .IsRequired();

            entity.Property(x => x.NewPersonId)
                .IsRequired();

            entity.Property(x => x.SwappedAt)
                .IsRequired();

            entity.HasOne<ScheduleEntry>()
                .WithMany()
                .HasForeignKey(x => x.ScheduleEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Person>()
                .WithMany()
                .HasForeignKey(x => x.OldPersonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Person>()
                .WithMany()
                .HasForeignKey(x => x.NewPersonId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
