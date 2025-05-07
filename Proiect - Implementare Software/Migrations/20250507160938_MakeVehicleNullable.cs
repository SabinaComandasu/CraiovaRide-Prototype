using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proiect___Implementare_Software.Migrations
{
    /// <inheritdoc />
    public partial class MakeVehicleNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rides_Vehicles_VehicleID",
                table: "Rides");

            migrationBuilder.AlterColumn<int>(
                name: "VehicleID",
                table: "Rides",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_Vehicles_VehicleID",
                table: "Rides",
                column: "VehicleID",
                principalTable: "Vehicles",
                principalColumn: "VehicleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rides_Vehicles_VehicleID",
                table: "Rides");

            migrationBuilder.AlterColumn<int>(
                name: "VehicleID",
                table: "Rides",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_Vehicles_VehicleID",
                table: "Rides",
                column: "VehicleID",
                principalTable: "Vehicles",
                principalColumn: "VehicleID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
