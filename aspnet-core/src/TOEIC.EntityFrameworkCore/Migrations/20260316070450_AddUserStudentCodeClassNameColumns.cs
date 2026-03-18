using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TOEIC.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStudentCodeClassNameColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClassName",
                table: "AbpUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentCode",
                table: "AbpUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassName",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "StudentCode",
                table: "AbpUsers");
        }
    }
}
