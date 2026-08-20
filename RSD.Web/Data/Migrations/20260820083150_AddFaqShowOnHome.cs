using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFaqShowOnHome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowOnHome",
                table: "faq_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowOnHome",
                table: "faq_items");
        }
    }
}
