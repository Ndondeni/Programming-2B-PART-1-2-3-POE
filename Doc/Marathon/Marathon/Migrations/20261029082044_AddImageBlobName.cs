using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marathon.Migrations
{
    /// <inheritdoc />
    public partial class AddImageBlobName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageBlobName",
                table: "results",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageBlobName",
                table: "events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAccountName",
                table: "accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageBlobName",
                table: "results");

            migrationBuilder.DropColumn(
                name: "ImageBlobName",
                table: "events");

            migrationBuilder.DropColumn(
                name: "UserAccountName",
                table: "accounts");
        }
    }
}
