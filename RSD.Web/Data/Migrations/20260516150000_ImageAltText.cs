using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ImageAltText : Migration
    {
        private static readonly string[] AllContentTables =
        [
            "blog_posts", "cases", "contact_points", "messenger_links", "mission_stats",
            "partners", "privacy_policies", "products", "services", "social_links",
            "team_members", "tech_stack_items", "terms_of_service", "testimonials", "values",
        ];

        private static readonly string[] CoverTables = ["blog_posts", "cases", "products", "services"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var table in CoverTables)
            {
                migrationBuilder.AddColumn<string>(
                    name: "CoverImageAlt",
                    table: table,
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: false,
                    defaultValue: "");
            }

            foreach (var table in AllContentTables)
            {
                migrationBuilder.AddColumn<string>(
                    name: "SeoOgImageAlt",
                    table: table,
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: false,
                    defaultValue: "");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var table in CoverTables)
            {
                migrationBuilder.DropColumn(name: "CoverImageAlt", table: table);
            }

            foreach (var table in AllContentTables)
            {
                migrationBuilder.DropColumn(name: "SeoOgImageAlt", table: table);
            }
        }
    }
}
