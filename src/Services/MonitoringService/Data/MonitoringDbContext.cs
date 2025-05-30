using Microsoft.EntityFrameworkCore;
using MonitoringService.Models;

namespace MonitoringService.Data;

public class MonitoringDbContext : DbContext
{
    public MonitoringDbContext(DbContextOptions<MonitoringDbContext> options) : base(options)
    {
    }

    public DbSet<Alert> Alerts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Alert entity configuration
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.PatientId)
                .IsRequired();

            entity.Property(e => e.AlertDateTime)
                .IsRequired();

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.Severity)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.AcknowledgedBy)
                .HasMaxLength(200);

            entity.Property(e => e.ResolvedBy)
                .HasMaxLength(200);

            entity.Property(e => e.ResolutionNotes)
                .HasMaxLength(1000);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.PatientId)
                .HasDatabaseName("IX_Alert_PatientId");

            entity.HasIndex(e => e.Severity)
                .HasDatabaseName("IX_Alert_Severity");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Alert_Status");

            entity.HasIndex(e => e.AlertDateTime)
                .HasDatabaseName("IX_Alert_AlertDateTime");

            entity.HasIndex(e => new { e.PatientId, e.Status })
                .HasDatabaseName("IX_Alert_PatientId_Status");

            entity.HasIndex(e => new { e.Severity, e.Status })
                .HasDatabaseName("IX_Alert_Severity_Status");
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var patientId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var patientId2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var patientId3 = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var alerts = new List<Alert>
        {
            new Alert
            {
                Id = Guid.NewGuid(),
                PatientId = patientId1,
                AlertDateTime = DateTime.UtcNow.AddHours(-2),
                Title = "High Blood Pressure Alert",
                Description = "Patient's blood pressure reading (150/95) exceeds normal range",
                Severity = AlertSeverity.Warning,
                Status = AlertStatus.New,
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                UpdatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new Alert
            {
                Id = Guid.NewGuid(),
                PatientId = patientId2,
                AlertDateTime = DateTime.UtcNow.AddHours(-6),
                Title = "Elevated Temperature",
                Description = "Patient temperature (38.5°C) indicates potential fever",
                Severity = AlertSeverity.Critical,
                Status = AlertStatus.Acknowledged,
                AcknowledgedBy = "Dr. Michael Johnson",
                AcknowledgedAt = DateTime.UtcNow.AddHours(-5),
                CreatedAt = DateTime.UtcNow.AddHours(-6),
                UpdatedAt = DateTime.UtcNow.AddHours(-5)
            },
            new Alert
            {
                Id = Guid.NewGuid(),
                PatientId = patientId3,
                AlertDateTime = DateTime.UtcNow.AddDays(-1),
                Title = "Medication Adherence Concern",
                Description = "Patient has missed scheduled medication doses",
                Severity = AlertSeverity.Warning,
                Status = AlertStatus.Resolved,
                AcknowledgedBy = "Nurse Jane Smith",
                AcknowledgedAt = DateTime.UtcNow.AddHours(-12),
                ResolvedBy = "Nurse Jane Smith",
                ResolvedAt = DateTime.UtcNow.AddHours(-8),
                ResolutionNotes = "Spoke with patient, medication schedule clarified",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddHours(-8)
            }
        };

        modelBuilder.Entity<Alert>().HasData(alerts);
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
        var entities = ChangeTracker
            .Entries()
            .Where(x => x.Entity is Alert && (x.State == EntityState.Added || x.State == EntityState.Modified));

        foreach (var entity in entities)
        {
            var now = DateTime.UtcNow;

            if (entity.State == EntityState.Added)
            {
                ((Alert)entity.Entity).CreatedAt = now;
            }

            ((Alert)entity.Entity).UpdatedAt = now;

            // Auto-set timestamps for status changes
            var alert = (Alert)entity.Entity;
            if (entity.State == EntityState.Modified)
            {
                var statusProperty = entity.Property(nameof(Alert.Status));
                if (statusProperty.IsModified)
                {
                    if (alert.Status == AlertStatus.Acknowledged && alert.AcknowledgedAt == null)
                    {
                        alert.AcknowledgedAt = now;
                    }
                    else if (alert.Status == AlertStatus.Resolved && alert.ResolvedAt == null)
                    {
                        alert.ResolvedAt = now;
                    }
                }
            }
        }
    }
}