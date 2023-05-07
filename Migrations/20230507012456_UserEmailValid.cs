using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Login.Migrations
{
    /// <inheritdoc />
    public partial class UserEmailValid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsValidated",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ValidationCode",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsValidated",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ValidationCode",
                table: "Users");
        }
    }
}
