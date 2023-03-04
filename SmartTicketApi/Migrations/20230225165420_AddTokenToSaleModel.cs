using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTicketApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenToSaleModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Token",
                table: "Sale",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Token",
                table: "Sale");
        }
    }
}
