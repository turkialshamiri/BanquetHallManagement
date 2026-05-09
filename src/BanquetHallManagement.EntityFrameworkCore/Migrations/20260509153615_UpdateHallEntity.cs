using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanquetHallManagement.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHallEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Halls",
                newName: "PricePerHour");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Halls",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Halls",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Halls",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Halls",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Halls");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Halls");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Halls");

            migrationBuilder.RenameColumn(
                name: "PricePerHour",
                table: "Halls",
                newName: "Price");

            migrationBuilder.AlterColumn<bool>(
                name: "Status",
                table: "Halls",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
