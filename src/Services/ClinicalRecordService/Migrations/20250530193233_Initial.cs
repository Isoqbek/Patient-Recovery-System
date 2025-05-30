using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClinicalRecordService.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicalEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntryType = table.Column<int>(type: "int", nullable: false),
                    EntryDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecordedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalEntries", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ClinicalEntries",
                columns: new[] { "Id", "CreatedAt", "Data", "EntryDateTime", "EntryType", "Notes", "PatientId", "RecordedBy", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("1307b246-b93c-431b-b0a4-ae4794bd806c"), new DateTime(2025, 5, 29, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7582), "{\"TreatmentPlan\":\"Lifestyle modification and medication\",\"Medications\":[\"Metformin 500mg\",\"Lisinopril 10mg\"],\"FollowUp\":\"2 weeks\"}", new DateTime(2025, 5, 29, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7497), 9, "Treatment plan for hypertension and diabetes", new Guid("33333333-3333-3333-3333-333333333333"), "Dr. Michael Johnson", new DateTime(2025, 5, 29, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7583) },
                    { new Guid("49ce7e46-b156-4711-be10-7bcc60491ac6"), new DateTime(2025, 5, 24, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7294), "{\"Severity\":\"Mild\",\"Location\":\"Frontal\",\"Duration\":\"2 hours\"}", new DateTime(2025, 5, 24, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7261), 2, "Patient reports mild headache", new Guid("11111111-1111-1111-1111-111111111111"), "Patient Self-Report", new DateTime(2025, 5, 24, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7294) },
                    { new Guid("767fa5dc-e9df-42b7-a99e-6421f3970700"), new DateTime(2025, 5, 26, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7393), "{\"Temperature\":37.2,\"BloodPressure\":\"130/85\",\"HeartRate\":88,\"RespiratoryRate\":18}", new DateTime(2025, 5, 26, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7336), 1, "Post-surgery vital signs monitoring", new Guid("22222222-2222-2222-2222-222222222222"), "Nurse John Wilson", new DateTime(2025, 5, 26, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7394) },
                    { new Guid("ce91585b-7e32-41a5-bce6-bc9dc5191d7f"), new DateTime(2025, 5, 23, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7257), "{\"Temperature\":36.8,\"BloodPressure\":\"120/80\",\"HeartRate\":72,\"RespiratoryRate\":16}", new DateTime(2025, 5, 23, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7013), 1, "Morning vital signs check", new Guid("11111111-1111-1111-1111-111111111111"), "Nurse Jane Smith", new DateTime(2025, 5, 23, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7258) },
                    { new Guid("d2e02c90-6568-4d00-81b1-7ddb9a03a48a"), new DateTime(2025, 5, 27, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7434), "{\"TestType\":\"Complete Blood Count\",\"Hemoglobin\":\"12.5 g/dL\",\"WhiteBloodCells\":\"7200/µL\",\"Status\":\"Normal\"}", new DateTime(2025, 5, 27, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7395), 6, "Blood test results", new Guid("22222222-2222-2222-2222-222222222222"), "Lab Technician Sarah", new DateTime(2025, 5, 27, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7435) },
                    { new Guid("d338f04f-d856-4192-870a-77c3a070c59f"), new DateTime(2025, 5, 25, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7333), "{\"MedicationName\":\"Paracetamol\",\"Dosage\":\"500mg\",\"Frequency\":\"Every 6 hours\",\"Duration\":\"3 days\"}", new DateTime(2025, 5, 25, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7296), 4, "Prescribed paracetamol for headache relief", new Guid("11111111-1111-1111-1111-111111111111"), "Dr. Michael Johnson", new DateTime(2025, 5, 25, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7334) },
                    { new Guid("f2b166ab-7962-43e7-bd77-473c02759bd0"), new DateTime(2025, 5, 28, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7494), "{\"PrimaryDiagnosis\":\"Hypertension\",\"SecondaryDiagnosis\":\"Type 2 Diabetes\",\"Confidence\":\"High\"}", new DateTime(2025, 5, 28, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7458), 8, "Initial diagnosis based on symptoms and examination", new Guid("33333333-3333-3333-3333-333333333333"), "Dr. Michael Johnson", new DateTime(2025, 5, 28, 19, 32, 33, 234, DateTimeKind.Utc).AddTicks(7495) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalEntry_EntryDateTime",
                table: "ClinicalEntries",
                column: "EntryDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalEntry_EntryType",
                table: "ClinicalEntries",
                column: "EntryType");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalEntry_PatientId",
                table: "ClinicalEntries",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalEntry_PatientId_EntryDateTime",
                table: "ClinicalEntries",
                columns: new[] { "PatientId", "EntryDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalEntry_PatientId_EntryType",
                table: "ClinicalEntries",
                columns: new[] { "PatientId", "EntryType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicalEntries");
        }
    }
}
