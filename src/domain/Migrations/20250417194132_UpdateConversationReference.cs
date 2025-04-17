using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADAM.Domain.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConversationReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivityId",
                table: "ConversationReferences");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "ConversationReferences",
                newName: "ConversationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConversationId",
                table: "ConversationReferences",
                newName: "ChannelId");

            migrationBuilder.AddColumn<string>(
                name: "ActivityId",
                table: "ConversationReferences",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
