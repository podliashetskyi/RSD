using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class Filters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "filters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    SeoMetaTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    SeoMetaDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, defaultValue: ""),
                    SeoOgImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, defaultValue: ""),
                    SeoOgImageAlt = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: ""),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_filters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_filters_DisplayOrder",
                table: "filters",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_filters_Slug",
                table: "filters",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_filters_Status",
                table: "filters",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_filters_Type",
                table: "filters",
                column: "Type");

            // Backfill: seed the new table from distinct existing values across
            // cases.Industry / cases.TechTags / blog_posts.Category / blog_posts.Tags.
            // Each insert uses GROUP BY on the raw label and a computed slug prefixed
            // with the type so the same label can coexist across types (e.g. a
            // "React" CaseTechTag and a "React" BlogTag). ON CONFLICT guards against
            // two distinct labels collapsing to the same slug after normalization.
            migrationBuilder.Sql("""
                INSERT INTO filters
                    ("Id", "Type", "Label", "DisplayOrder", "Slug", "Status",
                     "CreatedAt", "UpdatedAt", "PublishedAt", "IsDeleted",
                     "SeoMetaTitle", "SeoMetaDescription", "SeoOgImagePath", "SeoOgImageAlt")
                SELECT
                    gen_random_uuid(),
                    'CaseIndustry',
                    btrim("Industry"),
                    0,
                    'caseindustry-' || trim(both '-' from lower(regexp_replace(btrim("Industry"), '[^a-zA-Z0-9]+', '-', 'g'))),
                    'Published',
                    now(), now(), now(), FALSE,
                    '', '', '', ''
                FROM cases
                WHERE NOT "IsDeleted" AND btrim("Industry") <> ''
                GROUP BY btrim("Industry")
                ON CONFLICT DO NOTHING;
            """);

            migrationBuilder.Sql("""
                INSERT INTO filters
                    ("Id", "Type", "Label", "DisplayOrder", "Slug", "Status",
                     "CreatedAt", "UpdatedAt", "PublishedAt", "IsDeleted",
                     "SeoMetaTitle", "SeoMetaDescription", "SeoOgImagePath", "SeoOgImageAlt")
                SELECT
                    gen_random_uuid(),
                    'CaseTechTag',
                    tag,
                    0,
                    'casetechtag-' || trim(both '-' from lower(regexp_replace(tag, '[^a-zA-Z0-9]+', '-', 'g'))),
                    'Published',
                    now(), now(), now(), FALSE,
                    '', '', '', ''
                FROM (
                    SELECT DISTINCT btrim(unnest("TechTags")) AS tag
                    FROM cases
                    WHERE NOT "IsDeleted"
                ) AS t
                WHERE tag <> ''
                ON CONFLICT DO NOTHING;
            """);

            migrationBuilder.Sql("""
                INSERT INTO filters
                    ("Id", "Type", "Label", "DisplayOrder", "Slug", "Status",
                     "CreatedAt", "UpdatedAt", "PublishedAt", "IsDeleted",
                     "SeoMetaTitle", "SeoMetaDescription", "SeoOgImagePath", "SeoOgImageAlt")
                SELECT
                    gen_random_uuid(),
                    'BlogCategory',
                    btrim("Category"),
                    0,
                    'blogcategory-' || trim(both '-' from lower(regexp_replace(btrim("Category"), '[^a-zA-Z0-9]+', '-', 'g'))),
                    'Published',
                    now(), now(), now(), FALSE,
                    '', '', '', ''
                FROM blog_posts
                WHERE NOT "IsDeleted" AND btrim("Category") <> ''
                GROUP BY btrim("Category")
                ON CONFLICT DO NOTHING;
            """);

            migrationBuilder.Sql("""
                INSERT INTO filters
                    ("Id", "Type", "Label", "DisplayOrder", "Slug", "Status",
                     "CreatedAt", "UpdatedAt", "PublishedAt", "IsDeleted",
                     "SeoMetaTitle", "SeoMetaDescription", "SeoOgImagePath", "SeoOgImageAlt")
                SELECT
                    gen_random_uuid(),
                    'BlogTag',
                    tag,
                    0,
                    'blogtag-' || trim(both '-' from lower(regexp_replace(tag, '[^a-zA-Z0-9]+', '-', 'g'))),
                    'Published',
                    now(), now(), now(), FALSE,
                    '', '', '', ''
                FROM (
                    SELECT DISTINCT btrim(unnest("Tags")) AS tag
                    FROM blog_posts
                    WHERE NOT "IsDeleted"
                ) AS t
                WHERE tag <> ''
                ON CONFLICT DO NOTHING;
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "filters");
        }
    }
}
