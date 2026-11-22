using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroVision.DAL.Migrations
{
    /// <inheritdoc />
    public partial class CameraNameUniquePerDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cameras_DepartmentId",
                table: "Cameras");

            migrationBuilder.DropIndex(
                name: "IX_Cameras_Name",
                table: "Cameras");

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_DepartmentId_Name",
                table: "Cameras",
                columns: new[] { "DepartmentId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cameras_DepartmentId_Name",
                table: "Cameras");

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_DepartmentId",
                table: "Cameras",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_Name",
                table: "Cameras",
                column: "Name",
                unique: true);
        }
    }
}
