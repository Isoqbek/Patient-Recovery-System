using Microsoft.EntityFrameworkCore;
using ClinicalRecordService.Models;
using Newtonsoft.Json;

namespace ClinicalRecordService.Data;

public class ClinicalRecordDbContext : DbContext
{
    public ClinicalRecordDbContext(DbContextOptions<ClinicalRecordDbContext> options) : base(options)
    {
    }

    public DbSet<ClinicalEntry> ClinicalEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ClinicalEntry entity configuration
        modelBuilder.Entity<ClinicalEntry>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.PatientId)
                .IsRequired();

            entity.Property(e => e.EntryType)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.EntryDateTime)
                .IsRequired();

            entity.Property(e => e.RecordedBy)
                .HasMaxLength(200);

            entity.Property(e => e.Notes)
                .HasMaxLength(2000);

            entity.Property(e => e.Data)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.PatientId)
                .HasDatabaseName("IX_ClinicalEntry_PatientId");

            entity.HasIndex(e => e.EntryType)
                .HasDatabaseName("IX_ClinicalEntry_EntryType");

            entity.HasIndex(e => e.EntryDateTime)
                .HasDatabaseName("IX_ClinicalEntry_EntryDateTime");

            entity.HasIndex(e => new { e.PatientId, e.EntryDateTime })
                .HasDatabaseName("IX_ClinicalEntry_PatientId_EntryDateTime");

            entity.HasIndex(e => new { e.PatientId, e.EntryType })
                .HasDatabaseName("IX_ClinicalEntry_PatientId_EntryType");
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var patientId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var patientId2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var patientId3 = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var entries = new List<ClinicalEntry>
        {
            // John Doe entries
            new ClinicalEntry
            {
                Id = Guid.NewGuid(),
                PatientId = patientId1,
                EntryType = EntryType.VitalSign,
                EntryDateTime = DateTime.UtcNow.AddDays(-7),
                RecordedBy = "Nurse Jane Smith",
                Notes = "Morning vital signs check",
                Data = JsonConvert.SerializeObject(new
                {
                    Temperature = 36.8,
                    BloodPressure = "120/80",
                    HeartRate = 72,
                    RespiratoryRate = 16
                }),
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new ClinicalEntry
            {
                Id = Guid.NewGuid(),
                PatientId = patientId1,
                EntryType = EntryType.Symptom,
                EntryDateTime = DateTime.UtcNow.AddDays(-6),
                RecordedBy = "Patient Self-Report",
                Notes = "Patient reports mild headache",
                Data = JsonConvert.SerializeObject(new
                {
                    Severity = "Mild",
                    Location = "Frontal",
                    Duration = "2 hours"
                }),
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                UpdatedAt = DateTime.UtcNow.AddDays(-6)
            },
            new ClinicalEntry
            {
                Id = Guid.NewGuid(),
                PatientId = patientId1,
                EntryType = EntryType.Medication,
                EntryDateTime = DateTime.UtcNow.AddDays(-5),
                RecordedBy = "Dr. Michael Johnson",
                Notes = "Prescribed paracetamol for headache relief",
                Data = JsonConvert.SerializeObject(new
                {
                    MedicationName = "Paracetamol",
                    Dosage = "500mg",
                    Frequency = "Every 6 hours",
                    Duration = "3 days"
                }),
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },

            // Jane Smith entries
            new ClinicalEntry
            {
                Id = Guid.NewGuid(),
                PatientId = patientId2,
                EntryType = EntryType.VitalSign,
                EntryDateTime = DateTime.UtcNow.AddDays(-4),
                RecordedBy = "Nurse John Wilson",
                Notes = "Post-surgery vital signs monitoring",
                Data = JsonConvert.SerializeObject(new
                {
                    Temperature = 37.2,
                    BloodPressure = "130/85",
                    HeartRate = 88,
                    RespiratoryRate = 18
                }),
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                UpdatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new ClinicalEntry
            {
                Id = Guid.NewGuid(),
                PatientId = patientId2,
                EntryType = EntryType.TestResult,
                EntryDateTime = DateTime.UtcNow.AddDays(-3),
                RecordedBy = "Lab Technician Sarah",
                Notes = "Blood test results",
                Data = JsonConvert.SerializeObject(new
                {
                    TestType = "Complete Blood Count",
                    Hemoglobin = "12.5 g/dL",
                    WhiteBloodCells = "7200/µL",
                    Status = "Normal"
                }),
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },

            // Ahmed Karimov entries
            new ClinicalEntry
            {
                Id = Guid.NewGuid(),
                PatientId = patientId3,
                EntryType = EntryType.Diagnosis,
                EntryDateTime = DateTime.UtcNow.AddDays(-2),
                RecordedBy = "Dr. Michael Johnson",
                Notes = "Initial diagnosis based on symptoms and examination",
                Data = JsonConvert.SerializeObject(new
                {
                    PrimaryDiagnosis = "Hypertension",
                    SecondaryDiagnosis = "Type 2 Diabetes",
                    Confidence = "High"
                }),
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new ClinicalEntry
            {
                Id = Guid.NewGuid(),
                PatientId = patientId3,
                EntryType = EntryType.Treatment,
                EntryDateTime = DateTime.UtcNow.AddDays(-1),
                RecordedBy = "Dr. Michael Johnson",
                Notes = "Treatment plan for hypertension and diabetes",
                Data = JsonConvert.SerializeObject(new
                {
                    TreatmentPlan = "Lifestyle modification and medication",
                    Medications = new[] { "Metformin 500mg", "Lisinopril 10mg" },
                    FollowUp = "2 weeks"
                }),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        modelBuilder.Entity<ClinicalEntry>().HasData(entries);
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
            .Where(x => x.Entity is ClinicalEntry && (x.State == EntityState.Added || x.State == EntityState.Modified));

        foreach (var entity in entities)
        {
            var now = DateTime.UtcNow;

            if (entity.State == EntityState.Added)
            {
                ((ClinicalEntry)entity.Entity).CreatedAt = now;
            }

            ((ClinicalEntry)entity.Entity).UpdatedAt = now;
        }
    }
}