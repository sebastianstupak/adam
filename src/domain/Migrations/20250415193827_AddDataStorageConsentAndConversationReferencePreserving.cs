using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ADAM.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddDataStorageConsentAndConversationReferencePreserving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AcceptsDataStorage",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ConversationReferences",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ActivityId = table.Column<string>(type: "text", nullable: false),
                    ChannelId = table.Column<string>(type: "text", nullable: false),
                    ServiceUrl = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationReferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationReferences_UserId",
                table: "ConversationReferences",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationReferences");

            migrationBuilder.DropColumn(
                name: "AcceptsDataStorage",
                table: "Users");
        }
    }
}
