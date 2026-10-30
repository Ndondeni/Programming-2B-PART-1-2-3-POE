using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marathon.Migrations
{
    /// <inheritdoc />
    public partial class AddEvenStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "events",
                newName: "EventStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EventStatus",
                table: "events",
                newName: "Status");
        }
    }
}
