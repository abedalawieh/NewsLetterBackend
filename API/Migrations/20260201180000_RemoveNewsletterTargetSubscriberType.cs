using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsletterApp.API.Migrations
{
    public partial class RemoveNewsletterTargetSubscriberType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetSubscriberType",
                table: "Newsletters");
        }

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
