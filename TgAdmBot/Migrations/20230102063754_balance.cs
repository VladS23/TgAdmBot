using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TgAdmBot.Migrations
{
    /// <inheritdoc />
    public partial class balance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Billings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    type = table.Column<int>(type: "INTEGER", nullable: false),
                    callback = table.Column<string>(type: "TEXT", nullable: false),
                    billingId = table.Column<string>(type: "TEXT", nullable: false),
                    amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    payTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    payerId = table.Column<long>(type: "INTEGER", nullable: false),
                    PayFormLink = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Billings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Billings");
        }
    }
}
