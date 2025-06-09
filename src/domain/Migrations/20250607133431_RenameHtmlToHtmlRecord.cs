using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADAM.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RenameHtmlToHtmlRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MerchantOffers_TimestampedHtmlRecords_HtmlId",
                table: "MerchantOffers");

            migrationBuilder.RenameColumn(
                name: "HtmlId",
                table: "MerchantOffers",
                newName: "HtmlRecordId");

            migrationBuilder.RenameIndex(
                name: "IX_MerchantOffers_HtmlId",
                table: "MerchantOffers",
                newName: "IX_MerchantOffers_HtmlRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantOffers_TimestampedHtmlRecords_HtmlRecordId",
                table: "MerchantOffers",
                column: "HtmlRecordId",
                principalTable: "TimestampedHtmlRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MerchantOffers_TimestampedHtmlRecords_HtmlRecordId",
                table: "MerchantOffers");

            migrationBuilder.RenameColumn(
                name: "HtmlRecordId",
                table: "MerchantOffers",
                newName: "HtmlId");

            migrationBuilder.RenameIndex(
                name: "IX_MerchantOffers_HtmlRecordId",
                table: "MerchantOffers",
                newName: "IX_MerchantOffers_HtmlId");

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantOffers_TimestampedHtmlRecords_HtmlId",
                table: "MerchantOffers",
                column: "HtmlId",
                principalTable: "TimestampedHtmlRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
