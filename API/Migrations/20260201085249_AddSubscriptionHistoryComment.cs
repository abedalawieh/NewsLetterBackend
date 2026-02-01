using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsletterApp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionHistoryComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "SubscriptionHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "SubscriptionHistories");
        }
    }
}
