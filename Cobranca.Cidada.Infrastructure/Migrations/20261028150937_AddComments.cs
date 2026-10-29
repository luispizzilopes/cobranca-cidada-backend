using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cobranca.Cidada.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoterKey = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    CommitmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Commitments_CommitmentId",
                        column: x => x.CommitmentId,
                        principalTable: "Commitments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CommitmentId_CreatedAtUtc",
                table: "Comments",
                columns: new[] { "CommitmentId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_VoterKey",
                table: "Comments",
                column: "VoterKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");
        }
    }
}
