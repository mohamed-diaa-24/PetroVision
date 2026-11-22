using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroVision.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeleteCameras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Cameras_CameraId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Cameras_CameraId",
                table: "Incidents");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Cameras",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Cameras",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Cameras_CameraId",
                table: "Alerts",
                column: "CameraId",
                principalTable: "Cameras",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Cameras_CameraId",
                table: "Incidents",
                column: "CameraId",
                principalTable: "Cameras",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Cameras_CameraId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Cameras_CameraId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Cameras");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Cameras_CameraId",
                table: "Alerts",
                column: "CameraId",
                principalTable: "Cameras",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Cameras_CameraId",
                table: "Incidents",
                column: "CameraId",
                principalTable: "Cameras",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
