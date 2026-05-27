using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WellnessAPI.Migrations
{
    /// <inheritdoc />
    public partial class Phase3Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Statusi",
                table: "Terminet",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "LokacioniId",
                table: "Terminet",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProposedEnd",
                table: "Terminet",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProposedStart",
                table: "Terminet",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderEmailSentAt",
                table: "Terminet",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderSmsSentAt",
                table: "Terminet",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RescheduleNote",
                table: "Terminet",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "RescheduleProposedAt",
                table: "Terminet",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RescheduleProposedByUserId",
                table: "Terminet",
                type: "varchar(450)",
                maxLength: 450,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "LokacioniId",
                table: "Terapistet",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Terapistet",
                type: "varchar(450)",
                maxLength: 450,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Klientet",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "Klientet",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Klientet",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LoyaltyTier",
                table: "Klientet",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Bronze")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GdprErasureRequested",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "GdprErasureRequestedAt",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PrivacyPolicyAccepted",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrivacyPolicyAcceptedAt",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SmsOptIn",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TotpEnabledAt",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TotpSecret",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "Anetaresimet",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PaymentProvider",
                table: "Anetaresimet",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "Anetaresimet",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Anetaresimet",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Manual")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "StripeSessionId",
                table: "Anetaresimet",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ConsentLogs",
                columns: table => new
                {
                    ConsentLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    KlientId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConsentType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Version = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Accepted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserAgent = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentLogs", x => x.ConsentLogId);
                    table.ForeignKey(
                        name: "FK_ConsentLogs_Klientet_KlientId",
                        column: x => x.KlientId,
                        principalTable: "Klientet",
                        principalColumn: "KlientId",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Lokacionet",
                columns: table => new
                {
                    LokacioniId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Emri = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Adresa = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefoni = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Aktiv = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lokacionet", x => x.LokacioniId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Link = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsRead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Channel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subject = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Body = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.TemplateId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Terminet_LokacioniId",
                table: "Terminet",
                column: "LokacioniId");

            migrationBuilder.CreateIndex(
                name: "IX_Terapistet_LokacioniId",
                table: "Terapistet",
                column: "LokacioniId");

            migrationBuilder.CreateIndex(
                name: "IX_Terapistet_UserId",
                table: "Terapistet",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Anetaresimet_StripeSessionId",
                table: "Anetaresimet",
                column: "StripeSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentLogs_KlientId_ConsentType_Version",
                table: "ConsentLogs",
                columns: new[] { "KlientId", "ConsentType", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsentLogs_UserId_ConsentType_Version",
                table: "ConsentLogs",
                columns: new[] { "UserId", "ConsentType", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_Lokacionet_Emri",
                table: "Lokacionet",
                column: "Emri",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Key_Channel",
                table: "Templates",
                columns: new[] { "Key", "Channel" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Terapistet_Lokacionet_LokacioniId",
                table: "Terapistet",
                column: "LokacioniId",
                principalTable: "Lokacionet",
                principalColumn: "LokacioniId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Terminet_Lokacionet_LokacioniId",
                table: "Terminet",
                column: "LokacioniId",
                principalTable: "Lokacionet",
                principalColumn: "LokacioniId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Terapistet_Lokacionet_LokacioniId",
                table: "Terapistet");

            migrationBuilder.DropForeignKey(
                name: "FK_Terminet_Lokacionet_LokacioniId",
                table: "Terminet");

            migrationBuilder.DropTable(
                name: "ConsentLogs");

            migrationBuilder.DropTable(
                name: "Lokacionet");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Terminet_LokacioniId",
                table: "Terminet");

            migrationBuilder.DropIndex(
                name: "IX_Terapistet_LokacioniId",
                table: "Terapistet");

            migrationBuilder.DropIndex(
                name: "IX_Terapistet_UserId",
                table: "Terapistet");

            migrationBuilder.DropIndex(
                name: "IX_Anetaresimet_StripeSessionId",
                table: "Anetaresimet");

            migrationBuilder.DropColumn(
                name: "LokacioniId",
                table: "Terminet");

            migrationBuilder.DropColumn(
                name: "ProposedEnd",
                table: "Terminet");

            migrationBuilder.DropColumn(
                name: "ProposedStart",
                table: "Terminet");

            migrationBuilder.DropColumn(
                name: "ReminderEmailSentAt",
                table: "Terminet");

            migrationBuilder.DropColumn(
                name: "ReminderSmsSentAt",
                table: "Terminet");

            migrationBuilder.DropColumn(
                name: "RescheduleNote",
                table: "Terminet");

            migrationBuilder.DropColumn(
                name: "RescheduleProposedAt",
                table: "Terminet");

            migrationBuilder.DropColumn(
                name: "RescheduleProposedByUserId",
                table: "Terminet");

            migrationBuilder.DropColumn(
                name: "LokacioniId",
                table: "Terapistet");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Terapistet");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Klientet");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Klientet");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Klientet");

            migrationBuilder.DropColumn(
                name: "LoyaltyTier",
                table: "Klientet");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GdprErasureRequested",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GdprErasureRequestedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PrivacyPolicyAccepted",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PrivacyPolicyAcceptedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SmsOptIn",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TotpEnabledAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TotpSecret",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Anetaresimet");

            migrationBuilder.DropColumn(
                name: "PaymentProvider",
                table: "Anetaresimet");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "Anetaresimet");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Anetaresimet");

            migrationBuilder.DropColumn(
                name: "StripeSessionId",
                table: "Anetaresimet");

            migrationBuilder.AlterColumn<string>(
                name: "Statusi",
                table: "Terminet",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(40)",
                oldMaxLength: 40)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
