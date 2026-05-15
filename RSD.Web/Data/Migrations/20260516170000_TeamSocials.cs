using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class TeamSocials : Migration
    {
        private static readonly string[] UrlColumns = ["LinkedInUrl", "XUrl", "GitHubUrl"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var column in UrlColumns)
            {
                migrationBuilder.AddColumn<string>(
                    name: column,
                    table: "team_members",
                    type: "character varying(500)",
                    maxLength: 500,
                    nullable: false,
                    defaultValue: "");
            }
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "team_members",
                type: "character varying(320)",
                maxLength: 320,
                nullable: false,
                defaultValue: "");

            // The shared "Management"-scoped social_links rows are replaced by
            // per-manager URLs on team_members. Drop the now-orphaned rows.
            migrationBuilder.Sql("DELETE FROM social_links WHERE \"Scope\" = 'Management';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Note: the deleted Management-scoped social_links rows are not restored on Down.
            migrationBuilder.DropColumn(name: "Email", table: "team_members");
            foreach (var column in UrlColumns)
            {
                migrationBuilder.DropColumn(name: column, table: "team_members");
            }
        }
    }
}
