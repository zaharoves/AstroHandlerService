using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AstroHandlerService.Migrations
{
    /// <inheritdoc />
    public partial class AddEphemeris : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EphemerisSet",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SunDegrees = table.Column<double>(type: "double precision", nullable: true),
                    MoonDegrees = table.Column<double>(type: "double precision", nullable: true),
                    MercuryDegrees = table.Column<double>(type: "double precision", nullable: true),
                    VenusDegrees = table.Column<double>(type: "double precision", nullable: true),
                    MarsDegrees = table.Column<double>(type: "double precision", nullable: true),
                    JupiterDegrees = table.Column<double>(type: "double precision", nullable: true),
                    SaturnDegrees = table.Column<double>(type: "double precision", nullable: true),
                    UranDegrees = table.Column<double>(type: "double precision", nullable: true),
                    NeptuneDegrees = table.Column<double>(type: "double precision", nullable: true),
                    PlutoDegrees = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EphemerisSet", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EphemerisSet");
        }
    }
}
