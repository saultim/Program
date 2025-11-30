using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TextbookExchange.Migrations
{
    /// <inheritdoc />
    public partial class AddExchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exchanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OfferedBookId = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestedBookId = table.Column<int>(type: "INTEGER", nullable: false),
                    InitiatorId = table.Column<string>(type: "TEXT", nullable: false),
                    TargetUserId = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exchanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exchanges_AspNetUsers_InitiatorId",
                        column: x => x.InitiatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exchanges_AspNetUsers_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exchanges_Books_OfferedBookId",
                        column: x => x.OfferedBookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exchanges_Books_RequestedBookId",
                        column: x => x.RequestedBookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exchanges_InitiatorId",
                table: "Exchanges",
                column: "InitiatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Exchanges_OfferedBookId",
                table: "Exchanges",
                column: "OfferedBookId");

            migrationBuilder.CreateIndex(
                name: "IX_Exchanges_RequestedBookId",
                table: "Exchanges",
                column: "RequestedBookId");

            migrationBuilder.CreateIndex(
                name: "IX_Exchanges_TargetUserId",
                table: "Exchanges",
                column: "TargetUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exchanges");
        }
    }
}
