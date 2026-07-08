using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ContactPointHrefAndIcon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Href",
                table: "contact_points",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IconPath",
                table: "contact_points",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            // Backfill the rows the seeder created so existing sites keep the
            // footer's icons and mailto/tel links without re-seeding.
            migrationBuilder.Sql(
                """UPDATE contact_points SET "Href" = 'mailto:contactus@remsoft.dev', "IconPath" = 'images/icon-email.svg', "IsLink" = true WHERE "Label" = 'Email' AND "Href" = '';""");
            migrationBuilder.Sql(
                """UPDATE contact_points SET "Href" = 'tel:+14155551234', "IconPath" = 'images/icon-phone.svg', "IsLink" = true WHERE "Label" = 'Phone' AND "Href" = '';""");
            migrationBuilder.Sql(
                """UPDATE contact_points SET "IconPath" = 'images/icon-location.svg' WHERE "Label" = 'Address' AND "IconPath" = '';""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Href",
                table: "contact_points");

            migrationBuilder.DropColumn(
                name: "IconPath",
                table: "contact_points");
        }
    }
}
