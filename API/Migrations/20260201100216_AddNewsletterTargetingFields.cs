using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsletterApp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsletterTargetingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TargetSubscriberType",
                table: "Newsletters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TemplateName",
                table: "Newsletters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetSubscriberType",
                table: "Newsletters");

            migrationBuilder.DropColumn(
                name: "TemplateName",
                table: "Newsletters");
        }
    }
}
