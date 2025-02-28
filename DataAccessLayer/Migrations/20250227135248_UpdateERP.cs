using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateERP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductMachines_Machines_MakineId",
                table: "ProductMachines");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductMachines_Products_ProductId",
                table: "ProductMachines");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMachines_Machines_MakineId",
                table: "ProductMachines",
                column: "MakineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMachines_Products_ProductId",
                table: "ProductMachines",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductMachines_Machines_MakineId",
                table: "ProductMachines");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductMachines_Products_ProductId",
                table: "ProductMachines");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMachines_Machines_MakineId",
                table: "ProductMachines",
                column: "MakineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMachines_Products_ProductId",
                table: "ProductMachines",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
