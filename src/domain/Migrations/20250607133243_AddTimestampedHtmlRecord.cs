using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADAM.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddTimestampedHtmlRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Html",
                table: "MerchantOffers");

            migrationBuilder.AddColumn<long>(
                name: "HtmlId",
                table: "MerchantOffers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "TimestampedHtmlRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    HtmlContent = table.Column<string>(type: "TEXT", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimestampedHtmlRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MerchantOffers_HtmlId",
                table: "MerchantOffers",
                column: "HtmlId");

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantOffers_TimestampedHtmlRecords_HtmlId",
                table: "MerchantOffers",
                column: "HtmlId",
                principalTable: "TimestampedHtmlRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MerchantOffers_TimestampedHtmlRecords_HtmlId",
                table: "MerchantOffers");

            migrationBuilder.DropTable(
                name: "TimestampedHtmlRecords");

            migrationBuilder.DropIndex(
                name: "IX_MerchantOffers_HtmlId",
                table: "MerchantOffers");

            migrationBuilder.DropColumn(
                name: "HtmlId",
                table: "MerchantOffers");

            migrationBuilder.AddColumn<string>(
                name: "Html",
                table: "MerchantOffers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
