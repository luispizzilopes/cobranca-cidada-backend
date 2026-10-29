using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cobranca.Cidada.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Actor = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Details = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Officials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Party = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Officials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Origins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsInOfficialPlan = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Origins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mandates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OfficialId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mandates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mandates_Officials_OfficialId",
                        column: x => x.OfficialId,
                        principalTable: "Officials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Commitments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    MandateId = table.Column<Guid>(type: "uuid", nullable: false),
                    AreaId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ShortDescription = table.Column<string>(type: "text", nullable: true),
                    HowItWillBeDone = table.Column<string>(type: "text", nullable: true),
                    HasPlanDetail = table.Column<bool>(type: "boolean", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    DateKind = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeadlineText = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DeadlineDate = table.Column<DateOnly>(type: "date", nullable: true),
                    GoalOrCondition = table.Column<string>(type: "text", nullable: true),
                    Classification = table.Column<string[]>(type: "text[]", nullable: false),
                    Evidence = table.Column<string[]>(type: "text[]", nullable: false),
                    PlanPage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CurrentStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CurrentStatusDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commitments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commitments_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Commitments_Mandates_MandateId",
                        column: x => x.MandateId,
                        principalTable: "Mandates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Commitments_Origins_OriginId",
                        column: x => x.OriginId,
                        principalTable: "Origins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommitmentRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    SnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    AuthorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CommitmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitmentRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommitmentRevisions_Commitments_CommitmentId",
                        column: x => x.CommitmentId,
                        principalTable: "Commitments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommitmentSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Page = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    CommitmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitmentSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommitmentSources_Commitments_CommitmentId",
                        column: x => x.CommitmentId,
                        principalTable: "Commitments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatusEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ReferenceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SourceUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsFormalAct = table.Column<bool>(type: "boolean", nullable: false),
                    AuthorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CommitmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatusEntries_Commitments_CommitmentId",
                        column: x => x.CommitmentId,
                        principalTable: "Commitments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Areas_Code",
                table: "Areas",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OccurredAtUtc",
                table: "AuditLogs",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_CommitmentRevisions_CommitmentId_Version",
                table: "CommitmentRevisions",
                columns: new[] { "CommitmentId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Commitments_AreaId",
                table: "Commitments",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Commitments_ExternalId",
                table: "Commitments",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Commitments_IsPublished_AreaId",
                table: "Commitments",
                columns: new[] { "IsPublished", "AreaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Commitments_IsPublished_CurrentStatus",
                table: "Commitments",
                columns: new[] { "IsPublished", "CurrentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Commitments_MandateId",
                table: "Commitments",
                column: "MandateId");

            migrationBuilder.CreateIndex(
                name: "IX_Commitments_OriginId",
                table: "Commitments",
                column: "OriginId");

            migrationBuilder.CreateIndex(
                name: "IX_Commitments_Slug",
                table: "Commitments",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommitmentSources_CommitmentId",
                table: "CommitmentSources",
                column: "CommitmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Mandates_OfficialId",
                table: "Mandates",
                column: "OfficialId");

            migrationBuilder.CreateIndex(
                name: "IX_Mandates_Title",
                table: "Mandates",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Officials_Name",
                table: "Officials",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Origins_Code",
                table: "Origins",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusEntries_CommitmentId_ReferenceDate",
                table: "StatusEntries",
                columns: new[] { "CommitmentId", "ReferenceDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "CommitmentRevisions");

            migrationBuilder.DropTable(
                name: "CommitmentSources");

            migrationBuilder.DropTable(
                name: "StatusEntries");

            migrationBuilder.DropTable(
                name: "Commitments");

            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropTable(
                name: "Mandates");

            migrationBuilder.DropTable(
                name: "Origins");

            migrationBuilder.DropTable(
                name: "Officials");
        }
    }
}
