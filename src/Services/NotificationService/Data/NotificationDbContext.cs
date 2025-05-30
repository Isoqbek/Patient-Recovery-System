using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<NotificationLog> NotificationLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // NotificationLog entity configuration
        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.PatientId)
                .IsRequired();

            entity.Property(e => e.RecipientType)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.RecipientId)
                .HasMaxLength(200);

            entity.Property(e => e.RecipientEmail)
                .HasMaxLength(300);

            entity.Property(e => e.RecipientPhone)
                .HasMaxLength(20);

            entity.Property(e => e.NotificationType)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.Channel)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.Subject)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(e => e.Message)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(500);

            entity.Property(e => e.RelatedEntityType)
                .HasMaxLength(100);

            entity.Property(e => e.Priority)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.PatientId)
                .HasDatabaseName("IX_NotificationLog_PatientId");

            entity.HasIndex(e => e.RecipientType)
                .HasDatabaseName("IX_NotificationLog_RecipientType");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_NotificationLog_Status");

            entity.HasIndex(e => e.Priority)
                .HasDatabaseName("IX_NotificationLog_Priority");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_NotificationLog_CreatedAt");

            entity.HasIndex(e => new { e.PatientId, e.Status })
                .HasDatabaseName("IX_NotificationLog_PatientId_Status");

            entity.HasIndex(e => new { e.RecipientType, e.Status })
                .HasDatabaseName("IX_NotificationLog_RecipientType_Status");
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var patientId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var patientId2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var notifications = new List<NotificationLog>
        {
            new NotificationLog
            {
                Id = Guid.NewGuid(),
                PatientId = patientId1,
                RecipientType = "Physician",
                RecipientId = "physician-001",
                RecipientEmail = "michael.johnson@hospital.com",
                NotificationType = NotificationType.Alert,
                Channel = NotificationChannel.Email,
                Subject = "Critical Alert: High Temperature",
                Message = "Patient John Doe has a high temperature (38.5°C) requiring immediate attention.",
                Status = NotificationStatus.Sent,
                SentAt = DateTime.UtcNow.AddHours(-2),
                RelatedEntityId = Guid.NewGuid(),
                RelatedEntityType = "Alert",
                Priority = NotificationPriority.Critical,
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                UpdatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new NotificationLog
            {
                Id = Guid.NewGuid(),
                PatientId = patientId2,
                RecipientType = "Nurse",
                RecipientId = "nurse-001",
                RecipientEmail = "jane.smith@hospital.com",
                NotificationType = NotificationType.Reminder,
                Channel = NotificationChannel.Email,
                Subject = "Medication Reminder",
                Message = "Please ensure patient Jane Smith takes her evening medication.",
                Status = NotificationStatus.Sent,
                SentAt = DateTime.UtcNow.AddHours(-4),
                RelatedEntityId = Guid.NewGuid(),
                RelatedEntityType = "Medication",
                Priority = NotificationPriority.Normal,
                CreatedAt = DateTime.UtcNow.AddHours(-4),
                UpdatedAt = DateTime.UtcNow.AddHours(-4)
            },
            new NotificationLog
            {
                Id = Guid.NewGuid(),
                PatientId = patientId1,
                RecipientType = "Patient",
                RecipientId = "patient-001",
                RecipientEmail = "john.doe@patient.com",
                NotificationType = NotificationType.Appointment,
                Channel = NotificationChannel.SMS,
                Subject = "Appointment Reminder",
                Message = "You have an appointment tomorrow at 10:00 AM with Dr. Johnson.",
                Status = NotificationStatus.Pending,
                RelatedEntityId = Guid.NewGuid(),
                RelatedEntityType = "Appointment",
                Priority = NotificationPriority.Normal,
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-30)
            }
        };

        modelBuilder.Entity<NotificationLog>().HasData(notifications);
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
            .Where(x => x.Entity is NotificationLog && (x.State == EntityState.Added || x.State == EntityState.Modified));

        foreach (var entity in entities)
        {
            var now = DateTime.UtcNow;

            if (entity.State == EntityState.Added)
            {
                ((NotificationLog)entity.Entity).CreatedAt = now;
            }

            ((NotificationLog)entity.Entity).UpdatedAt = now;
        }
    }
}