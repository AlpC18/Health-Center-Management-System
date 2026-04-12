using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WellnessAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_MSSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Klientet",
                columns: table => new
                {
                    KlientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Emri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mbiemri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Telefoni = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataLindjes = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gjinia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KushtetShendetesore = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataRegjistrimit = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Klientet", x => x.KlientId);
                });

            migrationBuilder.CreateTable(
                name: "PaketaWellness",
                columns: table => new
                {
                    PaketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmriPaketes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pershkrimi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SherbimiPerfshire = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cmimi = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    KohezgjatjaMuaj = table.Column<int>(type: "int", nullable: false),
                    Aktive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaketaWellness", x => x.PaketId);
                });

            migrationBuilder.CreateTable(
                name: "Produktet",
                columns: table => new
                {
                    ProduktId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmriProduktit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kategoria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pershkrimi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cmimi = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SasiaStok = table.Column<int>(type: "int", nullable: false),
                    Aktiv = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produktet", x => x.ProduktId);
                });

            migrationBuilder.CreateTable(
                name: "Programet",
                columns: table => new
                {
                    ProgramId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmriProgramit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pershkrimi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KohezgjatjaJave = table.Column<int>(type: "int", nullable: false),
                    Qellimi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ushtrimet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dieta = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programet", x => x.ProgramId);
                });

            migrationBuilder.CreateTable(
                name: "Sherbimet",
                columns: table => new
                {
                    SherbimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmriSherbimit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kategoria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pershkrimi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KohezgjatjaMin = table.Column<int>(type: "int", nullable: false),
                    Cmimi = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Aktiv = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sherbimet", x => x.SherbimId);
                });

            migrationBuilder.CreateTable(
                name: "Terapistet",
                columns: table => new
                {
                    TerapistId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Emri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mbiemri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Specializimi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Licenca = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Telefoni = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aktiv = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terapistet", x => x.TerapistId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Anetaresimet",
                columns: table => new
                {
                    AnetaresimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KlientId = table.Column<int>(type: "int", nullable: false),
                    PaketId = table.Column<int>(type: "int", nullable: false),
                    DataFillimit = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataMbarimit = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Statusi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CmimiPaguar = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anetaresimet", x => x.AnetaresimId);
                    table.ForeignKey(
                        name: "FK_Anetaresimet_Klientet_KlientId",
                        column: x => x.KlientId,
                        principalTable: "Klientet",
                        principalColumn: "KlientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Anetaresimet_PaketaWellness_PaketId",
                        column: x => x.PaketId,
                        principalTable: "PaketaWellness",
                        principalColumn: "PaketId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShitjetProduktet",
                columns: table => new
                {
                    ShitjeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KlientId = table.Column<int>(type: "int", nullable: false),
                    ProduktId = table.Column<int>(type: "int", nullable: false),
                    Sasia = table.Column<int>(type: "int", nullable: false),
                    CmimiTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DataShitjes = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShitjetProduktet", x => x.ShitjeId);
                    table.ForeignKey(
                        name: "FK_ShitjetProduktet_Klientet_KlientId",
                        column: x => x.KlientId,
                        principalTable: "Klientet",
                        principalColumn: "KlientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShitjetProduktet_Produktet_ProduktId",
                        column: x => x.ProduktId,
                        principalTable: "Produktet",
                        principalColumn: "ProduktId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KlientProgramet",
                columns: table => new
                {
                    KpId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KlientId = table.Column<int>(type: "int", nullable: false),
                    ProgramId = table.Column<int>(type: "int", nullable: false),
                    DataFillimit = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataMbarimit = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Progresi = table.Column<int>(type: "int", nullable: false),
                    Statusi = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KlientProgramet", x => x.KpId);
                    table.ForeignKey(
                        name: "FK_KlientProgramet_Klientet_KlientId",
                        column: x => x.KlientId,
                        principalTable: "Klientet",
                        principalColumn: "KlientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KlientProgramet_Programet_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programet",
                        principalColumn: "ProgramId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Terminet",
                columns: table => new
                {
                    TerminId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KlientId = table.Column<int>(type: "int", nullable: false),
                    SherbimId = table.Column<int>(type: "int", nullable: false),
                    TerapistId = table.Column<int>(type: "int", nullable: false),
                    DataTerminit = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OraFillimit = table.Column<TimeSpan>(type: "time", nullable: false),
                    OraMbarimit = table.Column<TimeSpan>(type: "time", nullable: false),
                    Statusi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Shenimet = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terminet", x => x.TerminId);
                    table.ForeignKey(
                        name: "FK_Terminet_Klientet_KlientId",
                        column: x => x.KlientId,
                        principalTable: "Klientet",
                        principalColumn: "KlientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Terminet_Sherbimet_SherbimId",
                        column: x => x.SherbimId,
                        principalTable: "Sherbimet",
                        principalColumn: "SherbimId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Terminet_Terapistet_TerapistId",
                        column: x => x.TerapistId,
                        principalTable: "Terapistet",
                        principalColumn: "TerapistId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vlereisimet",
                columns: table => new
                {
                    VleresimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KlientId = table.Column<int>(type: "int", nullable: false),
                    SherbimId = table.Column<int>(type: "int", nullable: false),
                    TerapistId = table.Column<int>(type: "int", nullable: false),
                    Nota = table.Column<int>(type: "int", nullable: false),
                    Komenti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataVleresimit = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vlereisimet", x => x.VleresimId);
                    table.ForeignKey(
                        name: "FK_Vlereisimet_Klientet_KlientId",
                        column: x => x.KlientId,
                        principalTable: "Klientet",
                        principalColumn: "KlientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vlereisimet_Sherbimet_SherbimId",
                        column: x => x.SherbimId,
                        principalTable: "Sherbimet",
                        principalColumn: "SherbimId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vlereisimet_Terapistet_TerapistId",
                        column: x => x.TerapistId,
                        principalTable: "Terapistet",
                        principalColumn: "TerapistId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anetaresimet_KlientId",
                table: "Anetaresimet",
                column: "KlientId");

            migrationBuilder.CreateIndex(
                name: "IX_Anetaresimet_PaketId",
                table: "Anetaresimet",
                column: "PaketId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Klientet_Email",
                table: "Klientet",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KlientProgramet_KlientId",
                table: "KlientProgramet",
                column: "KlientId");

            migrationBuilder.CreateIndex(
                name: "IX_KlientProgramet_ProgramId",
                table: "KlientProgramet",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShitjetProduktet_KlientId",
                table: "ShitjetProduktet",
                column: "KlientId");

            migrationBuilder.CreateIndex(
                name: "IX_ShitjetProduktet_ProduktId",
                table: "ShitjetProduktet",
                column: "ProduktId");

            migrationBuilder.CreateIndex(
                name: "IX_Terapistet_Email",
                table: "Terapistet",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Terminet_KlientId",
                table: "Terminet",
                column: "KlientId");

            migrationBuilder.CreateIndex(
                name: "IX_Terminet_SherbimId",
                table: "Terminet",
                column: "SherbimId");

            migrationBuilder.CreateIndex(
                name: "IX_Terminet_TerapistId",
                table: "Terminet",
                column: "TerapistId");

            migrationBuilder.CreateIndex(
                name: "IX_Vlereisimet_KlientId",
                table: "Vlereisimet",
                column: "KlientId");

            migrationBuilder.CreateIndex(
                name: "IX_Vlereisimet_SherbimId",
                table: "Vlereisimet",
                column: "SherbimId");

            migrationBuilder.CreateIndex(
                name: "IX_Vlereisimet_TerapistId",
                table: "Vlereisimet",
                column: "TerapistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Anetaresimet");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "KlientProgramet");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "ShitjetProduktet");

            migrationBuilder.DropTable(
                name: "Terminet");

            migrationBuilder.DropTable(
                name: "Vlereisimet");

            migrationBuilder.DropTable(
                name: "PaketaWellness");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Programet");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Produktet");

            migrationBuilder.DropTable(
                name: "Klientet");

            migrationBuilder.DropTable(
                name: "Sherbimet");

            migrationBuilder.DropTable(
                name: "Terapistet");
        }
    }
}
