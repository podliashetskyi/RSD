using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProjectEstimates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "project_estimates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Platform = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Domain = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Complexity = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Timeline = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    EstimateMin = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    EstimateMax = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    ContactName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProjectDescription = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    IsHandled = table.Column<bool>(type: "boolean", nullable: false),
                    HandledByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    HandledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_estimates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_estimates_IsHandled",
                table: "project_estimates",
                column: "IsHandled");

            migrationBuilder.CreateIndex(
                name: "IX_project_estimates_SubmittedAt",
                table: "project_estimates",
                column: "SubmittedAt",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "project_estimates");
        }
    }
}
