using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MonitoringService.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlertDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TriggeringClinicalEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AcknowledgedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Alerts",
                columns: new[] { "Id", "AcknowledgedAt", "AcknowledgedBy", "AlertDateTime", "CreatedAt", "Description", "PatientId", "ResolutionNotes", "ResolvedAt", "ResolvedBy", "Severity", "Status", "Title", "TriggeringClinicalEntryId", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("55ecf745-209d-4698-b948-ecc148a7b42a"), null, null, new DateTime(2025, 5, 30, 20, 27, 40, 669, DateTimeKind.Utc).AddTicks(9654), new DateTime(2025, 5, 30, 20, 27, 40, 669, DateTimeKind.Utc).AddTicks(9660), "Patient's blood pressure reading (150/95) exceeds normal range", new Guid("11111111-1111-1111-1111-111111111111"), null, null, null, 2, 1, "High Blood Pressure Alert", null, new DateTime(2025, 5, 30, 20, 27, 40, 669, DateTimeKind.Utc).AddTicks(9661) },
                    { new Guid("76b5cb1d-8ce4-4446-b1bb-ea8014716994"), new DateTime(2025, 5, 30, 17, 27, 40, 669, DateTimeKind.Utc).AddTicks(9666), "Dr. Michael Johnson", new DateTime(2025, 5, 30, 16, 27, 40, 669, DateTimeKind.Utc).AddTicks(9664), new DateTime(2025, 5, 30, 16, 27, 40, 669, DateTimeKind.Utc).AddTicks(9669), "Patient temperature (38.5°C) indicates potential fever", new Guid("22222222-2222-2222-2222-222222222222"), null, null, null, 3, 2, "Elevated Temperature", null, new DateTime(2025, 5, 30, 17, 27, 40, 669, DateTimeKind.Utc).AddTicks(9670) },
                    { new Guid("d1f713db-d10f-474d-a889-448675d907ed"), new DateTime(2025, 5, 30, 10, 27, 40, 669, DateTimeKind.Utc).AddTicks(9674), "Nurse Jane Smith", new DateTime(2025, 5, 29, 22, 27, 40, 669, DateTimeKind.Utc).AddTicks(9671), new DateTime(2025, 5, 29, 22, 27, 40, 669, DateTimeKind.Utc).AddTicks(9676), "Patient has missed scheduled medication doses", new Guid("33333333-3333-3333-3333-333333333333"), "Spoke with patient, medication schedule clarified", new DateTime(2025, 5, 30, 14, 27, 40, 669, DateTimeKind.Utc).AddTicks(9674), "Nurse Jane Smith", 2, 4, "Medication Adherence Concern", null, new DateTime(2025, 5, 30, 14, 27, 40, 669, DateTimeKind.Utc).AddTicks(9677) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alert_AlertDateTime",
                table: "Alerts",
                column: "AlertDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Alert_PatientId",
                table: "Alerts",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Alert_PatientId_Status",
                table: "Alerts",
                columns: new[] { "PatientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Alert_Severity",
                table: "Alerts",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_Alert_Severity_Status",
                table: "Alerts",
                columns: new[] { "Severity", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Alert_Status",
                table: "Alerts",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");
        }
    }
}
