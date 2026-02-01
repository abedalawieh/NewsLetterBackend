using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsletterApp.API.Migrations
{
    public partial class NormalizeSubscriberLookups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubscriberCommunicationMethods",
                columns: table => new
                {
                    SubscriberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LookupItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberCommunicationMethods", x => new { x.SubscriberId, x.LookupItemId });
                    table.ForeignKey(
                        name: "FK_SubscriberCommunicationMethods_LookupItems_LookupItemId",
                        column: x => x.LookupItemId,
                        principalTable: "LookupItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriberCommunicationMethods_Subscribers_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscribers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriberInterests",
                columns: table => new
                {
                    SubscriberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LookupItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberInterests", x => new { x.SubscriberId, x.LookupItemId });
                    table.ForeignKey(
                        name: "FK_SubscriberInterests_LookupItems_LookupItemId",
                        column: x => x.LookupItemId,
                        principalTable: "LookupItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriberInterests_Subscribers_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscribers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberCommunicationMethods_LookupItemId",
                table: "SubscriberCommunicationMethods",
                column: "LookupItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberInterests_LookupItemId",
                table: "SubscriberInterests",
                column: "LookupItemId");

            migrationBuilder.Sql(@"
                INSERT INTO SubscriberCommunicationMethods (SubscriberId, LookupItemId)
                SELECT s.Id, li.Id
                FROM Subscribers s
                CROSS APPLY string_split(s.CommunicationMethods, ',') AS ss
                INNER JOIN LookupCategories lc ON lc.Name = 'CommunicationMethod' AND lc.IsDeleted = 0
                INNER JOIN LookupItems li ON li.CategoryId = lc.Id AND li.Value = LTRIM(RTRIM(ss.value)) AND li.IsDeleted = 0
                WHERE s.CommunicationMethods IS NOT NULL AND LTRIM(RTRIM(s.CommunicationMethods)) <> '';
            ");

            migrationBuilder.Sql(@"
                INSERT INTO SubscriberInterests (SubscriberId, LookupItemId)
                SELECT s.Id, li.Id
                FROM Subscribers s
                CROSS APPLY string_split(s.Interests, ',') AS ss
                INNER JOIN LookupCategories lc ON lc.Name = 'Interest' AND lc.IsDeleted = 0
                INNER JOIN LookupItems li ON li.CategoryId = lc.Id AND li.Value = LTRIM(RTRIM(ss.value)) AND li.IsDeleted = 0
                WHERE s.Interests IS NOT NULL AND LTRIM(RTRIM(s.Interests)) <> '';
            ");

            migrationBuilder.DropColumn(
                name: "CommunicationMethods",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "Interests",
                table: "Subscribers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriberCommunicationMethods");

            migrationBuilder.DropTable(
                name: "SubscriberInterests");

            migrationBuilder.AddColumn<string>(
                name: "CommunicationMethods",
                table: "Subscribers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Interests",
                table: "Subscribers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
