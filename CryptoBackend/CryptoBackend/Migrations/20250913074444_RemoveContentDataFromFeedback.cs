using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoBackend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveContentDataFromFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentData",
                table: "Feedbacks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentData",
                table: "Feedbacks",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
