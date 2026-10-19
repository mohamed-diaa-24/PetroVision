using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroVision.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertAndIncidentWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SourceAlertId",
                table: "Incidents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DismissReason",
                table: "Alerts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_SourceAlertId",
                table: "Incidents",
                column: "SourceAlertId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Incidents_SourceAlertId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "SourceAlertId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "DismissReason",
                table: "Alerts");
        }
    }
}
