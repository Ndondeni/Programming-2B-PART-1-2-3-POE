using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.API.Migrations
{
    /// <inheritdoc />
    public partial class FixParticipantRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Categories_CategoryId1",
                table: "Enrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Events_EventId1",
                table: "Enrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Participants_ParticipantId1",
                table: "Enrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_Events_EventId1",
                table: "Results");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_Participants_ParticipantId",
                table: "Results");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_Users_UserId",
                table: "Results");

            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.DropIndex(
                name: "IX_Results_EventId1",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_Results_UserId",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_Enrolments_CategoryId1",
                table: "Enrolments");

            migrationBuilder.DropIndex(
                name: "IX_Enrolments_EventId1",
                table: "Enrolments");

            migrationBuilder.DropIndex(
                name: "IX_Enrolments_ParticipantId1",
                table: "Enrolments");

            migrationBuilder.DropColumn(
                name: "EventId1",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "CategoryId1",
                table: "Enrolments");

            migrationBuilder.DropColumn(
                name: "EventId1",
                table: "Enrolments");

            migrationBuilder.DropColumn(
                name: "ParticipantId1",
                table: "Enrolments");

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Users_ParticipantId",
                table: "Results",
                column: "ParticipantId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Results_Users_ParticipantId",
                table: "Results");

            migrationBuilder.AddColumn<int>(
                name: "EventId1",
                table: "Results",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Results",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId1",
                table: "Enrolments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EventId1",
                table: "Enrolments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParticipantId1",
                table: "Enrolments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    ParticipantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipantName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParticipantType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.ParticipantId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Results_EventId1",
                table: "Results",
                column: "EventId1");

            migrationBuilder.CreateIndex(
                name: "IX_Results_UserId",
                table: "Results",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_CategoryId1",
                table: "Enrolments",
                column: "CategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_EventId1",
                table: "Enrolments",
                column: "EventId1");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_ParticipantId1",
                table: "Enrolments",
                column: "ParticipantId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Categories_CategoryId1",
                table: "Enrolments",
                column: "CategoryId1",
                principalTable: "Categories",
                principalColumn: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Events_EventId1",
                table: "Enrolments",
                column: "EventId1",
                principalTable: "Events",
                principalColumn: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Participants_ParticipantId1",
                table: "Enrolments",
                column: "ParticipantId1",
                principalTable: "Participants",
                principalColumn: "ParticipantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Events_EventId1",
                table: "Results",
                column: "EventId1",
                principalTable: "Events",
                principalColumn: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Participants_ParticipantId",
                table: "Results",
                column: "ParticipantId",
                principalTable: "Participants",
                principalColumn: "ParticipantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Users_UserId",
                table: "Results",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
