using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PatientManagementService.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "Id", "Address", "CreatedAt", "DateOfBirth", "Email", "FirstName", "Gender", "LastName", "MiddleName", "PhoneNumber", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "123 Main Street, Tashkent, Uzbekistan", new DateTime(2025, 5, 30, 17, 57, 13, 528, DateTimeKind.Utc).AddTicks(1542), new DateTime(1985, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "john.doe@patient.com", "John", 1, "Doe", "William", "+998901234567", new DateTime(2025, 5, 30, 17, 57, 13, 528, DateTimeKind.Utc).AddTicks(1544) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "456 Oak Avenue, Tashkent, Uzbekistan", new DateTime(2025, 5, 30, 17, 57, 13, 528, DateTimeKind.Utc).AddTicks(1545), new DateTime(1990, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "jane.smith@patient.com", "Jane", 2, "Smith", "Elizabeth", "+998907654321", new DateTime(2025, 5, 30, 17, 57, 13, 528, DateTimeKind.Utc).AddTicks(1546) },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "789 Navoi Street, Tashkent, Uzbekistan", new DateTime(2025, 5, 30, 17, 57, 13, 528, DateTimeKind.Utc).AddTicks(1547), new DateTime(1978, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "ahmed.karimov@patient.com", "Ahmed", 1, "Karimov", null, "+998901111111", new DateTime(2025, 5, 30, 17, 57, 13, 528, DateTimeKind.Utc).AddTicks(1547) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patient_Email",
                table: "Patients",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Patient_FullName",
                table: "Patients",
                columns: new[] { "FirstName", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_Patient_PhoneNumber",
                table: "Patients",
                column: "PhoneNumber",
                filter: "[PhoneNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
