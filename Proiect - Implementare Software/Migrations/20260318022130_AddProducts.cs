using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proiect___Implementare_Software.Migrations
{
    /// <inheritdoc />
    public partial class AddProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "PromoCodes");

            migrationBuilder.AddColumn<int>(
                name: "ProductID",
                table: "Rides",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PdfPath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rides_ProductID",
                table: "Rides",
                column: "ProductID");

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_Products_ProductID",
                table: "Rides",
                column: "ProductID",
                principalTable: "Products",
                principalColumn: "ProductID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rides_Products_ProductID",
                table: "Rides");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Rides_ProductID",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "ProductID",
                table: "Rides");

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "PromoCodes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
