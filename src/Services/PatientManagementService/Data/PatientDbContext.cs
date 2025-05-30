using Microsoft.EntityFrameworkCore;
using PatientManagementService.Models;

namespace PatientManagementService.Data;

public class PatientDbContext : DbContext
{
    public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Patient entity configuration
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.MiddleName)
                .HasMaxLength(100);

            entity.Property(e => e.DateOfBirth)
                .IsRequired();

            entity.Property(e => e.Gender)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(e => e.Email)
                .HasMaxLength(200);

            entity.Property(e => e.Address)
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => new { e.FirstName, e.LastName })
                .HasDatabaseName("IX_Patient_FullName");

            entity.HasIndex(e => e.Email)
                .HasDatabaseName("IX_Patient_Email")
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");

            entity.HasIndex(e => e.PhoneNumber)
                .HasDatabaseName("IX_Patient_PhoneNumber")
                .HasFilter("[PhoneNumber] IS NOT NULL");
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var patientId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var patientId2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var patientId3 = Guid.Parse("33333333-3333-3333-3333-333333333333");

        modelBuilder.Entity<Patient>().HasData(
            new Patient
            {
                Id = patientId1,
                FirstName = "John",
                LastName = "Doe",
                MiddleName = "William",
                DateOfBirth = new DateTime(1985, 5, 15),
                Gender = Gender.Male,
                PhoneNumber = "+998901234567",
                Email = "john.doe@patient.com",
                Address = "123 Main Street, Tashkent, Uzbekistan",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Patient
            {
                Id = patientId2,
                FirstName = "Jane",
                LastName = "Smith",
                MiddleName = "Elizabeth",
                DateOfBirth = new DateTime(1990, 8, 22),
                Gender = Gender.Female,
                PhoneNumber = "+998907654321",
                Email = "jane.smith@patient.com",
                Address = "456 Oak Avenue, Tashkent, Uzbekistan",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Patient
            {
                Id = patientId3,
                FirstName = "Ahmed",
                LastName = "Karimov",
                DateOfBirth = new DateTime(1978, 12, 10),
                Gender = Gender.Male,
                PhoneNumber = "+998901111111",
                Email = "ahmed.karimov@patient.com",
                Address = "789 Navoi Street, Tashkent, Uzbekistan",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
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
            .Where(x => x.Entity is Patient && (x.State == EntityState.Added || x.State == EntityState.Modified));

        foreach (var entity in entities)
        {
            var now = DateTime.UtcNow;

            if (entity.State == EntityState.Added)
            {
                ((Patient)entity.Entity).CreatedAt = now;
            }

            ((Patient)entity.Entity).UpdatedAt = now;
        }
    }
}