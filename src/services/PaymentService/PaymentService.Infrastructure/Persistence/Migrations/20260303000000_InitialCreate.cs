using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Virements",
                columns: table => new
                {
                    VirementId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompteSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompteDestinataireId = table.Column<Guid>(type: "uuid", nullable: false),
                    Montant = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DateVirement = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Statut = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Virements", x => x.VirementId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Virements_CompteSourceId",
                table: "Virements",
                column: "CompteSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Virements_CompteDestinataireId",
                table: "Virements",
                column: "CompteDestinataireId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Virements");
        }
    }
}
