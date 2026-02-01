using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsletterApp.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNewsletterTargetSubscriberType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetSubscriberType",
                table: "Newsletters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TargetSubscriberType",
                table: "Newsletters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
