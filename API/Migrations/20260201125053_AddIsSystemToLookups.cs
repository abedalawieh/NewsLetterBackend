using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsletterApp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSystemToLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "LookupItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "LookupCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "LookupItems");

            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "LookupCategories");
        }
    }
}
