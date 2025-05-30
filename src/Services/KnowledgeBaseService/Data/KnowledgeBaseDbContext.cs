using Microsoft.EntityFrameworkCore;
using KnowledgeBaseService.Models;

namespace KnowledgeBaseService.Data;

public class KnowledgeBaseDbContext : DbContext
{
    public KnowledgeBaseDbContext(DbContextOptions<KnowledgeBaseDbContext> options) : base(options)
    {
    }

    public DbSet<KnowledgeArticle> KnowledgeArticles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // KnowledgeArticle entity configuration
        modelBuilder.Entity<KnowledgeArticle>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(e => e.Category)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.Keywords)
                .HasMaxLength(1000);

            entity.Property(e => e.Source)
                .HasMaxLength(200);

            entity.Property(e => e.Version)
                .HasMaxLength(50);

            entity.Property(e => e.AuthorOrEditor)
                .HasMaxLength(200);

            entity.Property(e => e.Priority)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.Summary)
                .HasMaxLength(500);

            entity.Property(e => e.References)
                .HasMaxLength(2000);

            entity.Property(e => e.Language)
                .HasMaxLength(100);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.Category)
                .HasDatabaseName("IX_KnowledgeArticle_Category");

            entity.HasIndex(e => e.IsVerified)
                .HasDatabaseName("IX_KnowledgeArticle_IsVerified");

            entity.HasIndex(e => e.IsPublished)
                .HasDatabaseName("IX_KnowledgeArticle_IsPublished");

            entity.HasIndex(e => e.Priority)
                .HasDatabaseName("IX_KnowledgeArticle_Priority");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_KnowledgeArticle_CreatedAt");

            entity.HasIndex(e => new { e.Category, e.IsPublished })
                .HasDatabaseName("IX_KnowledgeArticle_Category_IsPublished");

            // Full-text search index on Title and Content (if supported)
            entity.HasIndex(e => e.Title)
                .HasDatabaseName("IX_KnowledgeArticle_Title");
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var articles = new List<KnowledgeArticle>
        {
            new KnowledgeArticle
            {
                Id = Guid.NewGuid(),
                Title = "Hypertension Management Guidelines",
                Category = ArticleCategory.DiagnosisGuideline,
                Content = @"# Hypertension Management Guidelines

## Definition
Hypertension is defined as systolic blood pressure ≥140 mmHg or diastolic blood pressure ≥90 mmHg on repeated measurements.

## Classification
- Normal: <120/80 mmHg
- Elevated: 120-129/<80 mmHg
- Stage 1: 130-139/80-89 mmHg
- Stage 2: ≥140/90 mmHg

## Initial Assessment
- Medical history and physical examination
- Laboratory tests: CBC, metabolic panel, lipid profile, TSH
- ECG and echocardiogram if indicated
- Assessment of cardiovascular risk factors

## Treatment Approach
1. **Lifestyle Modifications**
   - Weight reduction if overweight
   - DASH diet (low sodium, high potassium)
   - Regular aerobic exercise (30+ minutes, 3-4 times/week)
   - Smoking cessation
   - Alcohol moderation

2. **Pharmacological Treatment**
   - First-line agents: ACE inhibitors, ARBs, thiazide diuretics, calcium channel blockers
   - Consider combination therapy for Stage 2 hypertension
   - Target BP: <130/80 mmHg for most patients

## Monitoring
- Blood pressure checks every 3-6 months once controlled
- Annual laboratory monitoring
- Assess for end-organ damage",
                Keywords = "hypertension, blood pressure, cardiovascular, ACE inhibitors, DASH diet, guidelines",
                Source = "American Heart Association Guidelines 2023",
                Version = "2023.1",
                IsVerified = true,
                IsPublished = true,
                AuthorOrEditor = "Cardiology Department",
                Priority = ArticlePriority.High,
                Summary = "Comprehensive guidelines for diagnosis and management of hypertension including lifestyle modifications and pharmacological treatment.",
                References = "AHA/ACC Hypertension Guidelines 2023; JNC 8 Guidelines; ESC/ESH Guidelines 2023",
                LastReviewedDate = DateTime.UtcNow.AddDays(-30),
                NextReviewDate = DateTime.UtcNow.AddDays(335),
                ViewCount = 156,
                Language = "English",
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                UpdatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new KnowledgeArticle
            {
                Id = Guid.NewGuid(),
                Title = "Post-Operative Cardiac Rehabilitation Protocol",
                Category = ArticleCategory.RehabilitationExercise,
                Content = @"# Post-Operative Cardiac Rehabilitation Protocol

## Phase I: Inpatient Phase (1-3 days post-surgery)
- Bed mobility and basic self-care activities
- Short-distance walking (25-50 feet)
- Monitor vitals during activity
- Patient education on precautions

## Phase II: Early Recovery (1-2 weeks post-discharge)
- Progressive walking program
- Start: 5-10 minutes, 2-3 times daily
- Progress by 1-2 minutes every 2-3 days
- Target: 20-30 minutes continuous walking

## Phase III: Structured Rehabilitation (3-12 weeks)
- Supervised exercise sessions 3x/week
- Aerobic exercise: 20-40 minutes at 60-80% target heart rate
- Resistance training (light weights after 6-8 weeks)
- Education on risk factors and lifestyle modification

## Phase IV: Maintenance (Long-term)
- Continue regular exercise program
- Monthly follow-up for first 6 months
- Annual cardiology evaluation
- Ongoing lifestyle counseling

## Precautions
- No lifting >10 lbs for 6-8 weeks
- Avoid driving for 4-6 weeks
- Monitor for signs of infection or complications
- Stop exercise if chest pain, severe shortness of breath, or dizziness occurs",
                Keywords = "cardiac rehabilitation, post-operative, heart surgery, exercise protocol, recovery",
                Source = "American Association of Cardiovascular and Pulmonary Rehabilitation",
                Version = "2023.2",
                IsVerified = true,
                IsPublished = true,
                AuthorOrEditor = "Cardiac Rehabilitation Team",
                Priority = ArticlePriority.High,
                Summary = "Structured protocol for cardiac rehabilitation following heart surgery, organized in four progressive phases.",
                References = "AACVPR Guidelines 2023; AHA Scientific Statement on Cardiac Rehabilitation",
                LastReviewedDate = DateTime.UtcNow.AddDays(-45),
                NextReviewDate = DateTime.UtcNow.AddDays(320),
                ViewCount = 89,
                Language = "English",
                CreatedAt = DateTime.UtcNow.AddDays(-90),
                UpdatedAt = DateTime.UtcNow.AddDays(-45)
            },
            new KnowledgeArticle
            {
                Id = Guid.NewGuid(),
                Title = "Pain Management for Chronic Conditions",
                Category = ArticleCategory.SymptomManagement,
                Content = @"# Pain Management for Chronic Conditions

## Assessment
- Use validated pain scales (0-10 numeric rating scale)
- Assess pain location, quality, timing, and triggers
- Evaluate functional impact and psychological factors
- Review medical history and current medications

## Non-Pharmacological Approaches
1. **Physical Therapy**
   - Gentle exercises and stretching
   - Heat/cold therapy
   - TENS units
   - Massage therapy

2. **Cognitive-Behavioral Techniques**
   - Relaxation training
   - Mindfulness meditation
   - Stress management
   - Pain coping strategies

3. **Lifestyle Modifications**
   - Regular sleep schedule
   - Gentle exercise program
   - Healthy diet
   - Avoid triggers

## Pharmacological Management
1. **First-line**: Acetaminophen, topical agents
2. **Second-line**: NSAIDs (short-term), muscle relaxants
3. **Adjuvant therapies**: Anticonvulsants, antidepressants
4. **Opioids**: Reserved for severe pain, short-term use with careful monitoring

## Monitoring and Follow-up
- Regular pain assessments
- Functional improvement measures
- Monitor for medication side effects
- Adjust treatment plan based on response",
                Keywords = "chronic pain, pain management, non-pharmacological, opioids, physical therapy",
                Source = "International Association for the Study of Pain",
                Version = "2023.1",
                IsVerified = true,
                IsPublished = true,
                AuthorOrEditor = "Pain Management Committee",
                Priority = ArticlePriority.Normal,
                Summary = "Comprehensive approach to managing chronic pain using both non-pharmacological and pharmacological strategies.",
                References = "IASP Guidelines; CDC Opioid Prescribing Guidelines; WHO Pain Management Guidelines",
                LastReviewedDate = DateTime.UtcNow.AddDays(-20),
                NextReviewDate = DateTime.UtcNow.AddDays(345),
                ViewCount = 234,
                Language = "English",
                CreatedAt = DateTime.UtcNow.AddDays(-120),
                UpdatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new KnowledgeArticle
            {
                Id = Guid.NewGuid(),
                Title = "Emergency Response: Acute Myocardial Infarction",
                Category = ArticleCategory.EmergencyProcedure,
                Content = @"# Emergency Response: Acute Myocardial Infarction (AMI)

## Recognition
**Classic Symptoms:**
- Severe chest pain/pressure >20 minutes
- Pain radiating to left arm, jaw, or back
- Shortness of breath
- Nausea/vomiting
- Diaphoresis

**Atypical Presentations** (especially in women, elderly, diabetics):
- Fatigue, weakness
- Epigastric pain
- Syncope

## Immediate Actions (First 10 minutes)
1. **Call emergency services immediately**
2. **Administer aspirin 324mg** (if not allergic)
3. **Give nitroglycerin** if available and BP >90 systolic
4. **Apply oxygen** if SpO2 <90%
5. **Monitor vital signs**
6. **Prepare for transport**

## Hospital Protocol
1. **12-lead ECG within 10 minutes**
2. **Laboratory tests**: Troponins, CBC, BMP, PT/PTT, lipids
3. **Assess for reperfusion therapy**:
   - Primary PCI (preferred if available within 90 minutes)
   - Fibrinolytic therapy if PCI not available

## Door-to-Balloon Time Goals
- Primary PCI: <90 minutes
- Transfer for PCI: <120 minutes
- Door-to-needle (fibrinolytics): <30 minutes

## Post-AMI Care
- Dual antiplatelet therapy
- Beta-blockers, ACE inhibitors, statins
- Cardiac rehabilitation referral
- Risk factor modification education",
                Keywords = "myocardial infarction, heart attack, emergency, STEMI, NSTEMI, PCI, aspirin",
                Source = "American Heart Association STEMI Guidelines",
                Version = "2023.1",
                IsVerified = true,
                IsPublished = true,
                AuthorOrEditor = "Emergency Medicine Department",
                Priority = ArticlePriority.Critical,
                Summary = "Emergency protocol for recognition and immediate management of acute myocardial infarction.",
                References = "AHA/ACC STEMI Guidelines 2023; ESC Guidelines for AMI 2023",
                LastReviewedDate = DateTime.UtcNow.AddDays(-15),
                NextReviewDate = DateTime.UtcNow.AddDays(350),
                ViewCount = 445,
                Language = "English",
                CreatedAt = DateTime.UtcNow.AddDays(-180),
                UpdatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new KnowledgeArticle
            {
                Id = Guid.NewGuid(),
                Title = "Diabetes Mellitus Type 2: Comprehensive Management",
                Category = ArticleCategory.TreatmentProtocol,
                Content = @"# Diabetes Mellitus Type 2: Comprehensive Management

## Diagnosis
- Fasting glucose ≥126 mg/dL (7.0 mmol/L)
- Random glucose ≥200 mg/dL with symptoms
- HbA1c ≥6.5% (48 mmol/mol)
- 2-hour glucose ≥200 mg/dL during OGTT

## Treatment Goals
- HbA1c <7% for most adults
- Blood pressure <130/80 mmHg
- LDL cholesterol <100 mg/dL (<70 mg/dL if CVD)

## Lifestyle Management
1. **Medical Nutrition Therapy**
   - Carbohydrate counting
   - Portion control
   - Weight management if overweight
   - Limit refined sugars and processed foods

2. **Physical Activity**
   - 150+ minutes moderate aerobic activity/week
   - 2+ resistance training sessions/week
   - Reduce sedentary time

## Pharmacological Treatment
**First-line**: Metformin 500-1000mg twice daily

**Add-on therapies based on patient factors**:
- SGLT2 inhibitors (if CVD/CKD)
- GLP-1 agonists (if obesity/CVD)
- Insulin (if HbA1c >10% or symptomatic)

## Monitoring
- HbA1c every 3-6 months
- Annual eye exam
- Annual foot exam
- Nephropathy screening (ACR, eGFR)
- Cardiovascular risk assessment

## Complications Screening
- Retinopathy: Annual dilated eye exam
- Nephropathy: Annual urine microalbumin, serum creatinine
- Neuropathy: Annual foot exam with monofilament testing
- Cardiovascular: Lipid profile, blood pressure monitoring",
                Keywords = "diabetes, type 2, metformin, HbA1c, complications, screening, glucose",
                Source = "American Diabetes Association Standards of Care",
                Version = "2023.1",
                IsVerified = true,
                IsPublished = true,
                AuthorOrEditor = "Endocrinology Department",
                Priority = ArticlePriority.High,
                Summary = "Evidence-based approach to diagnosis, treatment, and monitoring of Type 2 diabetes mellitus.",
                References = "ADA Standards of Care 2023; AACE Diabetes Guidelines; WHO Diabetes Guidelines",
                LastReviewedDate = DateTime.UtcNow.AddDays(-60),
                NextReviewDate = DateTime.UtcNow.AddDays(305),
                ViewCount = 178,
                Language = "English",
                CreatedAt = DateTime.UtcNow.AddDays(-200),
                UpdatedAt = DateTime.UtcNow.AddDays(-60)
            }
        };

        modelBuilder.Entity<KnowledgeArticle>().HasData(articles);
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
            .Where(x => x.Entity is KnowledgeArticle && (x.State == EntityState.Added || x.State == EntityState.Modified));

        foreach (var entity in entities)
        {
            var now = DateTime.UtcNow;

            if (entity.State == EntityState.Added)
            {
                ((KnowledgeArticle)entity.Entity).CreatedAt = now;
            }

            ((KnowledgeArticle)entity.Entity).UpdatedAt = now;
        }
    }
}