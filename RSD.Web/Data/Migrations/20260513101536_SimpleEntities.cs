using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SimpleEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contact_points",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Lines = table.Column<List<string>>(type: "text[]", nullable: false),
                    IsLink = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_contact_points", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "messenger_links",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LargeIconPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SmallIconPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BgColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Href = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_messenger_links", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "mission_stats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_mission_stats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "partners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PhotoPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContactHref = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_partners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "social_links",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IconPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Href = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_social_links", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "team_members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AvatarPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsManagement = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_team_members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tech_stack_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LogoPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_tech_stack_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "testimonials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AvatarPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AuthorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AuthorRole = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayOnHome = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_testimonials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "values",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IconPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_values", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contact_points_DisplayOrder",
                table: "contact_points",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_contact_points_Slug",
                table: "contact_points",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_contact_points_Status",
                table: "contact_points",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_messenger_links_DisplayOrder",
                table: "messenger_links",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_messenger_links_Slug",
                table: "messenger_links",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_messenger_links_Status",
                table: "messenger_links",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_mission_stats_DisplayOrder",
                table: "mission_stats",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_mission_stats_Slug",
                table: "mission_stats",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_mission_stats_Status",
                table: "mission_stats",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_partners_DisplayOrder",
                table: "partners",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_partners_Slug",
                table: "partners",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_partners_Status",
                table: "partners",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_social_links_DisplayOrder",
                table: "social_links",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_social_links_Scope",
                table: "social_links",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "IX_social_links_Slug",
                table: "social_links",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_social_links_Status",
                table: "social_links",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_team_members_DisplayOrder",
                table: "team_members",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_team_members_IsManagement",
                table: "team_members",
                column: "IsManagement");

            migrationBuilder.CreateIndex(
                name: "IX_team_members_Slug",
                table: "team_members",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_team_members_Status",
                table: "team_members",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_tech_stack_items_DisplayOrder",
                table: "tech_stack_items",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_tech_stack_items_Slug",
                table: "tech_stack_items",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_tech_stack_items_Status",
                table: "tech_stack_items",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_testimonials_DisplayOrder",
                table: "testimonials",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_testimonials_Slug",
                table: "testimonials",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_testimonials_Status",
                table: "testimonials",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_values_DisplayOrder",
                table: "values",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_values_Slug",
                table: "values",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_values_Status",
                table: "values",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contact_points");

            migrationBuilder.DropTable(
                name: "messenger_links");

            migrationBuilder.DropTable(
                name: "mission_stats");

            migrationBuilder.DropTable(
                name: "partners");

            migrationBuilder.DropTable(
                name: "social_links");

            migrationBuilder.DropTable(
                name: "team_members");

            migrationBuilder.DropTable(
                name: "tech_stack_items");

            migrationBuilder.DropTable(
                name: "testimonials");

            migrationBuilder.DropTable(
                name: "values");
        }
    }
}
