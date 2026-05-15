using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ListSummary : Migration
    {
        private static readonly string[] SummaryTables = ["blog_posts", "cases", "products", "services"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var table in SummaryTables)
            {
                migrationBuilder.AddColumn<string>(
                    name: "Summary",
                    table: table,
                    type: "character varying(280)",
                    maxLength: 280,
                    nullable: false,
                    defaultValue: "");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var table in SummaryTables)
            {
                migrationBuilder.DropColumn(name: "Summary", table: table);
            }
        }
    }
}
