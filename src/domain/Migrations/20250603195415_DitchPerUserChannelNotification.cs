using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADAM.Domain.Migrations
{
    /// <inheritdoc />
    public partial class DitchPerUserChannelNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConversationReferences_UserId",
                table: "ConversationReferences");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationReferences_UserId",
                table: "ConversationReferences",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConversationReferences_UserId",
                table: "ConversationReferences");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationReferences_UserId",
                table: "ConversationReferences",
                column: "UserId",
                unique: true);
        }
    }
}
