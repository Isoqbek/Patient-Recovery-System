using Microsoft.EntityFrameworkCore;
using RehabilitationService.Models;

namespace RehabilitationService.Data;

public class RehabilitationDbContext : DbContext
{
    public RehabilitationDbContext(DbContextOptions<RehabilitationDbContext> options) : base(options)
    {
    }

    public DbSet<RehabilitationPlan> RehabilitationPlans { get; set; }
    public DbSet<RehabilitationProgressLog> RehabilitationProgressLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // RehabilitationPlan entity configuration
        modelBuilder.Entity<RehabilitationPlan>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.PatientId)
                .IsRequired();

            entity.Property(e => e.PlanName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.StartDate)
                .IsRequired();

            entity.Property(e => e.Goals)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.AssignedTherapist)
                .HasMaxLength(200);

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(200);

            entity.Property(e => e.PlanType)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.Difficulty)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.SpecialInstructions)
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.PatientId)
                .HasDatabaseName("IX_RehabilitationPlan_PatientId");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_RehabilitationPlan_Status");

            entity.HasIndex(e => e.PlanType)
                .HasDatabaseName("IX_RehabilitationPlan_PlanType");

            entity.HasIndex(e => e.StartDate)
                .HasDatabaseName("IX_RehabilitationPlan_StartDate");

            entity.HasIndex(e => new { e.PatientId, e.Status })
                .HasDatabaseName("IX_RehabilitationPlan_PatientId_Status");
        });

        // RehabilitationProgressLog entity configuration
        modelBuilder.Entity<RehabilitationProgressLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.RehabilitationPlanId)
                .IsRequired();

            entity.Property(e => e.LogDate)
                .IsRequired();

            entity.Property(e => e.Notes)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.ActivityDetails)
                .HasMaxLength(2000);

            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(200);

            entity.Property(e => e.ProgressType)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.CompletionStatus)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.Challenges)
                .HasMaxLength(500);

            entity.Property(e => e.Achievements)
                .HasMaxLength(500);

            entity.Property(e => e.TherapistNotes)
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Foreign key relationship
            entity.HasOne(e => e.RehabilitationPlan)
                .WithMany(p => p.ProgressLogs)
                .HasForeignKey(e => e.RehabilitationPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.RehabilitationPlanId)
                .HasDatabaseName("IX_RehabilitationProgressLog_RehabilitationPlanId");

            entity.HasIndex(e => e.LogDate)
                .HasDatabaseName("IX_RehabilitationProgressLog_LogDate");

            entity.HasIndex(e => e.ProgressType)
                .HasDatabaseName("IX_RehabilitationProgressLog_ProgressType");

            entity.HasIndex(e => e.CompletionStatus)
                .HasDatabaseName("IX_RehabilitationProgressLog_CompletionStatus");
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var patientId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var patientId2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var patientId3 = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var plan1Id = Guid.NewGuid();
        var plan2Id = Guid.NewGuid();
        var plan3Id = Guid.NewGuid();

        var rehabilitationPlans = new List<RehabilitationPlan>
        {
            new RehabilitationPlan
            {
                Id = plan1Id,
                PatientId = patientId1,
                PlanName = "Post-Surgery Cardiac Rehabilitation",
                Description = "Comprehensive cardiac rehabilitation program following heart surgery",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(60),
                Goals = "Improve cardiovascular fitness, reduce risk factors, and enhance quality of life. Target: 30 minutes of moderate exercise 5 days per week.",
                Status = PlanStatus.Active,
                AssignedTherapist = "Dr. Sarah Wilson",
                CreatedBy = "Dr. Michael Johnson",
                PlanType = PlanType.Cardiac,
                Difficulty = PlanDifficulty.Intermediate,
                EstimatedDurationWeeks = 12,
                SpecialInstructions = "Monitor heart rate during exercise. Stop if chest pain occurs.",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new RehabilitationPlan
            {
                Id = plan2Id,
                PatientId = patientId2,
                PlanName = "Knee Replacement Recovery Program",
                Description = "Physical therapy program for knee replacement surgery recovery",
                StartDate = DateTime.UtcNow.AddDays(-14),
                EndDate = DateTime.UtcNow.AddDays(75),
                Goals = "Restore knee function, improve mobility, and strengthen leg muscles. Target: Full range of motion and ability to walk without assistance.",
                Status = PlanStatus.Active,
                AssignedTherapist = "Lisa Rodriguez, PT",
                CreatedBy = "Dr. Michael Johnson",
                PlanType = PlanType.Orthopedic,
                Difficulty = PlanDifficulty.Beginner,
                EstimatedDurationWeeks = 16,
                SpecialInstructions = "Ice after exercises. No weight bearing beyond doctor's orders.",
                CreatedAt = DateTime.UtcNow.AddDays(-14),
                UpdatedAt = DateTime.UtcNow.AddDays(-14)
            },
            new RehabilitationPlan
            {
                Id = plan3Id,
                PatientId = patientId3,
                PlanName = "Stroke Recovery - Speech and Motor Skills",
                Description = "Comprehensive rehabilitation for stroke recovery focusing on speech and motor skills",
                StartDate = DateTime.UtcNow.AddDays(-45),
                EndDate = DateTime.UtcNow.AddDays(135),
                Goals = "Improve speech clarity, restore fine motor skills, and enhance cognitive function. Target: Independent daily living activities.",
                Status = PlanStatus.Active,
                AssignedTherapist = "Maria Garcia, SLP",
                CreatedBy = "Dr. Michael Johnson",
                PlanType = PlanType.Neurological,
                Difficulty = PlanDifficulty.Advanced,
                EstimatedDurationWeeks = 24,
                SpecialInstructions = "Family involvement encouraged. Progress may be gradual.",
                CreatedAt = DateTime.UtcNow.AddDays(-45),
                UpdatedAt = DateTime.UtcNow.AddDays(-45)
            }
        };

        var progressLogs = new List<RehabilitationProgressLog>
        {
            // Progress logs for Plan 1 (Cardiac Rehab)
            new RehabilitationProgressLog
            {
                Id = Guid.NewGuid(),
                RehabilitationPlanId = plan1Id,
                LogDate = DateTime.UtcNow.AddDays(-5),
                Notes = "Completed 25 minutes of walking on treadmill. Heart rate remained stable.",
                ActivityDetails = "Treadmill walking: 25 minutes at 3.0 mph, incline 2%",
                PainLevel = 2,
                EnergyLevel = 7,
                MoodLevel = 8,
                SubmittedBy = "Patient Self-Report",
                ProgressType = ProgressType.Exercise,
                CompletionStatus = CompletionStatus.Completed,
                DurationMinutes = 25,
                Achievements = "Increased duration by 5 minutes from last session",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new RehabilitationProgressLog
            {
                Id = Guid.NewGuid(),
                RehabilitationPlanId = plan1Id,
                LogDate = DateTime.UtcNow.AddDays(-2),
                Notes = "Therapy session focused on strength training and education",
                ActivityDetails = "Upper body strength exercises, 15 reps x 3 sets each",
                PainLevel = 1,
                EnergyLevel = 8,
                MoodLevel = 9,
                SubmittedBy = "Dr. Sarah Wilson",
                ProgressType = ProgressType.Therapy,
                CompletionStatus = CompletionStatus.Completed,
                DurationMinutes = 45,
                TherapistNotes = "Patient showing excellent progress. Ready to advance to next level.",
                Achievements = "Completed all exercises without difficulty",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },

            // Progress logs for Plan 2 (Knee Rehab)
            new RehabilitationProgressLog
            {
                Id = Guid.NewGuid(),
                RehabilitationPlanId = plan2Id,
                LogDate = DateTime.UtcNow.AddDays(-3),
                Notes = "Range of motion exercises. Some stiffness noted in the morning.",
                ActivityDetails = "Knee flexion exercises: achieved 90 degrees. Quadriceps strengthening.",
                PainLevel = 4,
                EnergyLevel = 6,
                MoodLevel = 7,
                SubmittedBy = "Lisa Rodriguez, PT",
                ProgressType = ProgressType.Therapy,
                CompletionStatus = CompletionStatus.Completed,
                DurationMinutes = 30,
                Challenges = "Morning stiffness limiting initial range of motion",
                Achievements = "Achieved 90-degree flexion for first time",
                TherapistNotes = "Good progress. Continue current exercises, add balance training next week.",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },

            // Progress logs for Plan 3 (Stroke Recovery)
            new RehabilitationProgressLog
            {
                Id = Guid.NewGuid(),
                RehabilitationPlanId = plan3Id,
                LogDate = DateTime.UtcNow.AddDays(-1),
                Notes = "Speech therapy session. Working on articulation and word retrieval.",
                ActivityDetails = "Speech exercises: 20 minutes articulation, 15 minutes word games",
                PainLevel = null,
                EnergyLevel = 5,
                MoodLevel = 6,
                SubmittedBy = "Maria Garcia, SLP",
                ProgressType = ProgressType.Therapy,
                CompletionStatus = CompletionStatus.Completed,
                DurationMinutes = 35,
                Challenges = "Difficulty with complex sentences, some frustration noted",
                Achievements = "Improvement in single word clarity",
                TherapistNotes = "Steady progress. Family reports better communication at home.",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        modelBuilder.Entity<RehabilitationPlan>().HasData(rehabilitationPlans);
        modelBuilder.Entity<RehabilitationProgressLog>().HasData(progressLogs);
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
            .Where(x => (x.Entity is RehabilitationPlan || x.Entity is RehabilitationProgressLog) &&
                       (x.State == EntityState.Added || x.State == EntityState.Modified));

        foreach (var entity in entities)
        {
            var now = DateTime.UtcNow;

            if (entity.State == EntityState.Added)
            {
                if (entity.Entity is RehabilitationPlan plan)
                {
                    plan.CreatedAt = now;
                    plan.UpdatedAt = now;
                }
                else if (entity.Entity is RehabilitationProgressLog log)
                {
                    log.CreatedAt = now;
                    log.UpdatedAt = now;
                }
            }
            else if (entity.State == EntityState.Modified)
            {
                if (entity.Entity is RehabilitationPlan plan)
                {
                    plan.UpdatedAt = now;
                }
                else if (entity.Entity is RehabilitationProgressLog log)
                {
                    log.UpdatedAt = now;
                }
            }
        }
    }
}