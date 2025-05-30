using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RehabilitationService.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RehabilitationPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Goals = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssignedTherapist = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PlanType = table.Column<int>(type: "int", nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false),
                    EstimatedDurationWeeks = table.Column<int>(type: "int", nullable: false),
                    SpecialInstructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RehabilitationPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RehabilitationProgressLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    RehabilitationPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LogDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ActivityDetails = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PainLevel = table.Column<int>(type: "int", nullable: true),
                    EnergyLevel = table.Column<int>(type: "int", nullable: true),
                    MoodLevel = table.Column<int>(type: "int", nullable: true),
                    SubmittedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProgressType = table.Column<int>(type: "int", nullable: false),
                    CompletionStatus = table.Column<int>(type: "int", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    Challenges = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Achievements = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TherapistNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RehabilitationProgressLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RehabilitationProgressLogs_RehabilitationPlans_RehabilitationPlanId",
                        column: x => x.RehabilitationPlanId,
                        principalTable: "RehabilitationPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "RehabilitationPlans",
                columns: new[] { "Id", "AssignedTherapist", "CreatedAt", "CreatedBy", "Description", "Difficulty", "EndDate", "EstimatedDurationWeeks", "Goals", "PatientId", "PlanName", "PlanType", "SpecialInstructions", "StartDate", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("1d0c07df-b39e-4f31-b9e0-981cab09847d"), "Dr. Sarah Wilson", new DateTime(2025, 4, 30, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8531), "Dr. Michael Johnson", "Comprehensive cardiac rehabilitation program following heart surgery", 2, new DateTime(2025, 7, 29, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8526), 12, "Improve cardiovascular fitness, reduce risk factors, and enhance quality of life. Target: 30 minutes of moderate exercise 5 days per week.", new Guid("11111111-1111-1111-1111-111111111111"), "Post-Surgery Cardiac Rehabilitation", 4, "Monitor heart rate during exercise. Stop if chest pain occurs.", new DateTime(2025, 4, 30, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8522), 2, new DateTime(2025, 4, 30, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8532) },
                    { new Guid("a2fda7cc-39c6-4f52-83df-2f758abed603"), "Maria Garcia, SLP", new DateTime(2025, 4, 15, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8541), "Dr. Michael Johnson", "Comprehensive rehabilitation for stroke recovery focusing on speech and motor skills", 3, new DateTime(2025, 10, 12, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8540), 24, "Improve speech clarity, restore fine motor skills, and enhance cognitive function. Target: Independent daily living activities.", new Guid("33333333-3333-3333-3333-333333333333"), "Stroke Recovery - Speech and Motor Skills", 5, "Family involvement encouraged. Progress may be gradual.", new DateTime(2025, 4, 15, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8539), 2, new DateTime(2025, 4, 15, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8541) },
                    { new Guid("c9e0248d-ab11-44a6-bb71-fdf481389a74"), "Lisa Rodriguez, PT", new DateTime(2025, 5, 16, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8537), "Dr. Michael Johnson", "Physical therapy program for knee replacement surgery recovery", 1, new DateTime(2025, 8, 13, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8536), 16, "Restore knee function, improve mobility, and strengthen leg muscles. Target: Full range of motion and ability to walk without assistance.", new Guid("22222222-2222-2222-2222-222222222222"), "Knee Replacement Recovery Program", 6, "Ice after exercises. No weight bearing beyond doctor's orders.", new DateTime(2025, 5, 16, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8535), 2, new DateTime(2025, 5, 16, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8538) }
                });

            migrationBuilder.InsertData(
                table: "RehabilitationProgressLogs",
                columns: new[] { "Id", "Achievements", "ActivityDetails", "Challenges", "CompletionStatus", "CreatedAt", "DurationMinutes", "EnergyLevel", "LogDate", "MoodLevel", "Notes", "PainLevel", "ProgressType", "RehabilitationPlanId", "SubmittedBy", "TherapistNotes", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("052f3d90-7d78-4d12-b56f-91d3e73e55ec"), "Achieved 90-degree flexion for first time", "Knee flexion exercises: achieved 90 degrees. Quadriceps strengthening.", "Morning stiffness limiting initial range of motion", 3, new DateTime(2025, 5, 27, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8570), 30, 6, new DateTime(2025, 5, 27, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8568), 7, "Range of motion exercises. Some stiffness noted in the morning.", 4, 2, new Guid("c9e0248d-ab11-44a6-bb71-fdf481389a74"), "Lisa Rodriguez, PT", "Good progress. Continue current exercises, add balance training next week.", new DateTime(2025, 5, 27, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8570) },
                    { new Guid("21ff1944-ff23-4c67-9163-448c0a4f7342"), "Improvement in single word clarity", "Speech exercises: 20 minutes articulation, 15 minutes word games", "Difficulty with complex sentences, some frustration noted", 3, new DateTime(2025, 5, 29, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8574), 35, 5, new DateTime(2025, 5, 29, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8572), 6, "Speech therapy session. Working on articulation and word retrieval.", null, 2, new Guid("a2fda7cc-39c6-4f52-83df-2f758abed603"), "Maria Garcia, SLP", "Steady progress. Family reports better communication at home.", new DateTime(2025, 5, 29, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8574) },
                    { new Guid("b077a3ea-cebe-4bfe-bd18-e2751ad4ba11"), "Completed all exercises without difficulty", "Upper body strength exercises, 15 reps x 3 sets each", null, 3, new DateTime(2025, 5, 28, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8554), 45, 8, new DateTime(2025, 5, 28, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8552), 9, "Therapy session focused on strength training and education", 1, 2, new Guid("1d0c07df-b39e-4f31-b9e0-981cab09847d"), "Dr. Sarah Wilson", "Patient showing excellent progress. Ready to advance to next level.", new DateTime(2025, 5, 28, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8555) },
                    { new Guid("b45fd2f0-7eab-4ec2-900f-2db17a9e940a"), "Increased duration by 5 minutes from last session", "Treadmill walking: 25 minutes at 3.0 mph, incline 2%", null, 3, new DateTime(2025, 5, 25, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8547), 25, 7, new DateTime(2025, 5, 25, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8544), 8, "Completed 25 minutes of walking on treadmill. Heart rate remained stable.", 2, 1, new Guid("1d0c07df-b39e-4f31-b9e0-981cab09847d"), "Patient Self-Report", null, new DateTime(2025, 5, 25, 22, 53, 1, 162, DateTimeKind.Utc).AddTicks(8548) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RehabilitationPlan_PatientId",
                table: "RehabilitationPlans",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_RehabilitationPlan_PatientId_Status",
                table: "RehabilitationPlans",
                columns: new[] { "PatientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RehabilitationPlan_PlanType",
                table: "RehabilitationPlans",
                column: "PlanType");

            migrationBuilder.CreateIndex(
                name: "IX_RehabilitationPlan_StartDate",
                table: "RehabilitationPlans",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_RehabilitationPlan_Status",
                table: "RehabilitationPlans",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RehabilitationProgressLog_CompletionStatus",
                table: "RehabilitationProgressLogs",
                column: "CompletionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_RehabilitationProgressLog_LogDate",
                table: "RehabilitationProgressLogs",
                column: "LogDate");

            migrationBuilder.CreateIndex(
                name: "IX_RehabilitationProgressLog_ProgressType",
                table: "RehabilitationProgressLogs",
                column: "ProgressType");

            migrationBuilder.CreateIndex(
                name: "IX_RehabilitationProgressLog_RehabilitationPlanId",
                table: "RehabilitationProgressLogs",
                column: "RehabilitationPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RehabilitationProgressLogs");

            migrationBuilder.DropTable(
                name: "RehabilitationPlans");
        }
    }
}
