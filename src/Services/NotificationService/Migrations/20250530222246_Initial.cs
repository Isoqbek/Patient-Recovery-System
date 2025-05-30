using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NotificationService.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipientType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RecipientId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RecipientEmail = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RecipientPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NotificationType = table.Column<int>(type: "int", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLogs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "NotificationLogs",
                columns: new[] { "Id", "Channel", "CreatedAt", "ErrorMessage", "Message", "NotificationType", "PatientId", "Priority", "RecipientEmail", "RecipientId", "RecipientPhone", "RecipientType", "RelatedEntityId", "RelatedEntityType", "RetryCount", "SentAt", "Status", "Subject", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("01b73e6e-c665-4c62-a7b1-f00a61009a60"), 1, new DateTime(2025, 5, 30, 18, 22, 46, 323, DateTimeKind.Utc).AddTicks(8625), null, "Please ensure patient Jane Smith takes her evening medication.", 7, new Guid("22222222-2222-2222-2222-222222222222"), 2, "jane.smith@hospital.com", "nurse-001", null, "Nurse", new Guid("4329c102-42be-4b03-853f-1379fd8112f8"), "Medication", 0, new DateTime(2025, 5, 30, 18, 22, 46, 323, DateTimeKind.Utc).AddTicks(8623), 2, "Medication Reminder", new DateTime(2025, 5, 30, 18, 22, 46, 323, DateTimeKind.Utc).AddTicks(8625) },
                    { new Guid("88198807-ec9c-4c70-8767-6fc5c25e2e0f"), 2, new DateTime(2025, 5, 30, 21, 52, 46, 323, DateTimeKind.Utc).AddTicks(8647), null, "You have an appointment tomorrow at 10:00 AM with Dr. Johnson.", 2, new Guid("11111111-1111-1111-1111-111111111111"), 2, "john.doe@patient.com", "patient-001", null, "Patient", new Guid("ece91910-743d-45a5-a596-8c4115711b05"), "Appointment", 0, null, 1, "Appointment Reminder", new DateTime(2025, 5, 30, 21, 52, 46, 323, DateTimeKind.Utc).AddTicks(8648) },
                    { new Guid("9f28ff6c-b6b4-4c38-bd9e-4ff04f7595ed"), 1, new DateTime(2025, 5, 30, 20, 22, 46, 323, DateTimeKind.Utc).AddTicks(8618), null, "Patient John Doe has a high temperature (38.5°C) requiring immediate attention.", 1, new Guid("11111111-1111-1111-1111-111111111111"), 4, "michael.johnson@hospital.com", "physician-001", null, "Physician", new Guid("9024aec8-f27d-4dfe-8eb8-18e5008443e6"), "Alert", 0, new DateTime(2025, 5, 30, 20, 22, 46, 323, DateTimeKind.Utc).AddTicks(8612), 2, "Critical Alert: High Temperature", new DateTime(2025, 5, 30, 20, 22, 46, 323, DateTimeKind.Utc).AddTicks(8619) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLog_CreatedAt",
                table: "NotificationLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLog_PatientId",
                table: "NotificationLogs",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLog_PatientId_Status",
                table: "NotificationLogs",
                columns: new[] { "PatientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLog_Priority",
                table: "NotificationLogs",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLog_RecipientType",
                table: "NotificationLogs",
                column: "RecipientType");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLog_RecipientType_Status",
                table: "NotificationLogs",
                columns: new[] { "RecipientType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLog_Status",
                table: "NotificationLogs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationLogs");
        }
    }
}
