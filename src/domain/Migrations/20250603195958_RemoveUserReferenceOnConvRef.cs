using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADAM.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserReferenceOnConvRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationReferences_Users_UserId",
                table: "ConversationReferences");

            migrationBuilder.DropIndex(
                name: "IX_ConversationReferences_UserId",
                table: "ConversationReferences");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ConversationReferences");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "ConversationReferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationReferences_UserId",
                table: "ConversationReferences",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationReferences_Users_UserId",
                table: "ConversationReferences",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
