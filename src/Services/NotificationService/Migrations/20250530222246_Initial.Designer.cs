﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NotificationService.Data;

#nullable disable

namespace NotificationService.Migrations
{
    [DbContext(typeof(NotificationDbContext))]
    [Migration("20250530222246_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("NotificationService.Models.NotificationLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<int>("Channel")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("ErrorMessage")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<int>("NotificationType")
                        .HasColumnType("int");

                    b.Property<Guid>("PatientId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<string>("RecipientEmail")
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<string>("RecipientId")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("RecipientPhone")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("RecipientType")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<Guid?>("RelatedEntityId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("RelatedEntityType")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("RetryCount")
                        .HasColumnType("int");

                    b.Property<DateTime?>("SentAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.HasKey("Id");

                    b.HasIndex("CreatedAt")
                        .HasDatabaseName("IX_NotificationLog_CreatedAt");

                    b.HasIndex("PatientId")
                        .HasDatabaseName("IX_NotificationLog_PatientId");

                    b.HasIndex("Priority")
                        .HasDatabaseName("IX_NotificationLog_Priority");

                    b.HasIndex("RecipientType")
                        .HasDatabaseName("IX_NotificationLog_RecipientType");

                    b.HasIndex("Status")
                        .HasDatabaseName("IX_NotificationLog_Status");

                    b.HasIndex("PatientId", "Status")
                        .HasDatabaseName("IX_NotificationLog_PatientId_Status");

                    b.HasIndex("RecipientType", "Status")
                        .HasDatabaseName("IX_NotificationLog_RecipientType_Status");

                    b.ToTable("NotificationLogs");

                    b.HasData(
                        new
                        {
                            Id = new Guid("9f28ff6c-b6b4-4c38-bd9e-4ff04f7595ed"),
                            Channel = 1,
                            CreatedAt = new DateTime(2025, 5, 30, 20, 22, 46, 323, DateTimeKind.Utc).AddTicks(8618),
                            Message = "Patient John Doe has a high temperature (38.5°C) requiring immediate attention.",
                            NotificationType = 1,
                            PatientId = new Guid("11111111-1111-1111-1111-111111111111"),
                            Priority = 4,
                            RecipientEmail = "michael.johnson@hospital.com",
                            RecipientId = "physician-001",
                            RecipientType = "Physician",
                            RelatedEntityId = new Guid("9024aec8-f27d-4dfe-8eb8-18e5008443e6"),
                            RelatedEntityType = "Alert",
                            RetryCount = 0,
                            SentAt = new DateTime(2025, 5, 30, 20, 22, 46, 323, DateTimeKind.Utc).AddTicks(8612),
                            Status = 2,
                            Subject = "Critical Alert: High Temperature",
                            UpdatedAt = new DateTime(2025, 5, 30, 20, 22, 46, 323, DateTimeKind.Utc).AddTicks(8619)
                        },
                        new
                        {
                            Id = new Guid("01b73e6e-c665-4c62-a7b1-f00a61009a60"),
                            Channel = 1,
                            CreatedAt = new DateTime(2025, 5, 30, 18, 22, 46, 323, DateTimeKind.Utc).AddTicks(8625),
                            Message = "Please ensure patient Jane Smith takes her evening medication.",
                            NotificationType = 7,
                            PatientId = new Guid("22222222-2222-2222-2222-222222222222"),
                            Priority = 2,
                            RecipientEmail = "jane.smith@hospital.com",
                            RecipientId = "nurse-001",
                            RecipientType = "Nurse",
                            RelatedEntityId = new Guid("4329c102-42be-4b03-853f-1379fd8112f8"),
                            RelatedEntityType = "Medication",
                            RetryCount = 0,
                            SentAt = new DateTime(2025, 5, 30, 18, 22, 46, 323, DateTimeKind.Utc).AddTicks(8623),
                            Status = 2,
                            Subject = "Medication Reminder",
                            UpdatedAt = new DateTime(2025, 5, 30, 18, 22, 46, 323, DateTimeKind.Utc).AddTicks(8625)
                        },
                        new
                        {
                            Id = new Guid("88198807-ec9c-4c70-8767-6fc5c25e2e0f"),
                            Channel = 2,
                            CreatedAt = new DateTime(2025, 5, 30, 21, 52, 46, 323, DateTimeKind.Utc).AddTicks(8647),
                            Message = "You have an appointment tomorrow at 10:00 AM with Dr. Johnson.",
                            NotificationType = 2,
                            PatientId = new Guid("11111111-1111-1111-1111-111111111111"),
                            Priority = 2,
                            RecipientEmail = "john.doe@patient.com",
                            RecipientId = "patient-001",
                            RecipientType = "Patient",
                            RelatedEntityId = new Guid("ece91910-743d-45a5-a596-8c4115711b05"),
                            RelatedEntityType = "Appointment",
                            RetryCount = 0,
                            Status = 1,
                            Subject = "Appointment Reminder",
                            UpdatedAt = new DateTime(2025, 5, 30, 21, 52, 46, 323, DateTimeKind.Utc).AddTicks(8648)
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
