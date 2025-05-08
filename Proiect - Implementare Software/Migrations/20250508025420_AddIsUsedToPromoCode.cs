using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proiect___Implementare_Software.Migrations
{
    /// <inheritdoc />
    public partial class AddIsUsedToPromoCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "PromoCodes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "PromoCodes");
        }
    }
}
