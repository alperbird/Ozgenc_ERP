using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class FabrikaGenelStok : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "F_Depo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepoAdi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_F_Depo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "F_MalzemeAdlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    F_MalzemeAdi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_F_MalzemeAdlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "F_Tedarikci",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    F_TedarikciAdi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_F_Tedarikci", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "F_StokGiris",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GeneralPartId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderQuantity = table.Column<int>(type: "int", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_F_StokGiris", x => x.Id);
                    table.ForeignKey(
                        name: "FK_F_StokGiris_F_Depo_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "F_Depo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_F_StokGiris_F_MalzemeAdlari_GeneralPartId",
                        column: x => x.GeneralPartId,
                        principalTable: "F_MalzemeAdlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_F_StokGiris_F_Tedarikci_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "F_Tedarikci",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_F_StokGiris_GeneralPartId",
                table: "F_StokGiris",
                column: "GeneralPartId");

            migrationBuilder.CreateIndex(
                name: "IX_F_StokGiris_SupplierId",
                table: "F_StokGiris",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_F_StokGiris_WarehouseId",
                table: "F_StokGiris",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "F_StokGiris");

            migrationBuilder.DropTable(
                name: "F_Depo");

            migrationBuilder.DropTable(
                name: "F_MalzemeAdlari");

            migrationBuilder.DropTable(
                name: "F_Tedarikci");
        }
    }
}
