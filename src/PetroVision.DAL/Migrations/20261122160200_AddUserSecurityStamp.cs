using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroVision.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSecurityStamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "Users",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            // Give every existing user a random stamp (old tokens without a stamp stop working).
            migrationBuilder.Sql("UPDATE [Users] SET [SecurityStamp] = REPLACE(CONVERT(nvarchar(36), NEWID()), '-', '');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "Users");
        }
    }
}
