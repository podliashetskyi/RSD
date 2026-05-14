using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class LegalPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "terms_of_service",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LastUpdatedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    BodyHtml = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    SeoMetaTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SeoMetaDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SeoOgImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_terms_of_service", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "privacy_policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LastUpdatedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    BodyHtml = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    SeoMetaTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SeoMetaDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SeoOgImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_privacy_policies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_terms_of_service_Slug",
                table: "terms_of_service",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_terms_of_service_Status",
                table: "terms_of_service",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_privacy_policies_Slug",
                table: "privacy_policies",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_privacy_policies_Status",
                table: "privacy_policies",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "terms_of_service");
            migrationBuilder.DropTable(name: "privacy_policies");
        }
    }
}
