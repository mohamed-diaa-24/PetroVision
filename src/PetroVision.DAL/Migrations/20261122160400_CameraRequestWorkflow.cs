using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroVision.DAL.Migrations
{
    /// <inheritdoc />
    public partial class CameraRequestWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CameraId",
                table: "CameraRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "CameraRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Models",
                table: "CameraRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "CameraRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "CameraRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "CameraRequests",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // Existing rows (if any) belong to the requester's department; rows without one cannot be reviewed.
            migrationBuilder.Sql(@"UPDATE cr SET cr.[DepartmentId] = u.[DepartmentId]
FROM [CameraRequests] cr JOIN [Users] u ON u.[Id] = cr.[RequestedById]
WHERE u.[DepartmentId] IS NOT NULL;
DELETE FROM [CameraRequests] WHERE [DepartmentId] = 0;");

            migrationBuilder.CreateIndex(
                name: "IX_CameraRequests_CameraId",
                table: "CameraRequests",
                column: "CameraId");

            migrationBuilder.CreateIndex(
                name: "IX_CameraRequests_DepartmentId",
                table: "CameraRequests",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_CameraRequests_Cameras_CameraId",
                table: "CameraRequests",
                column: "CameraId",
                principalTable: "Cameras",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CameraRequests_Departments_DepartmentId",
                table: "CameraRequests",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CameraRequests_Cameras_CameraId",
                table: "CameraRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_CameraRequests_Departments_DepartmentId",
                table: "CameraRequests");

            migrationBuilder.DropIndex(
                name: "IX_CameraRequests_CameraId",
                table: "CameraRequests");

            migrationBuilder.DropIndex(
                name: "IX_CameraRequests_DepartmentId",
                table: "CameraRequests");

            migrationBuilder.DropColumn(name: "CameraId", table: "CameraRequests");
            migrationBuilder.DropColumn(name: "DepartmentId", table: "CameraRequests");
            migrationBuilder.DropColumn(name: "Models", table: "CameraRequests");
            migrationBuilder.DropColumn(name: "Reason", table: "CameraRequests");
            migrationBuilder.DropColumn(name: "ReviewNote", table: "CameraRequests");
            migrationBuilder.DropColumn(name: "Type", table: "CameraRequests");
        }
    }
}
