using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TextbookExchange.Migrations
{
    /// <inheritdoc />
    public partial class RemoveForExchangeFromDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForExchange",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Books");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ForExchange",
                table: "Books",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Books",
                type: "TEXT",
                nullable: true);
        }
    }
}
