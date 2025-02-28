using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductMachines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"), // ✅ IDENTITY ekledik!
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    MakineId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductMachines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductMachines_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductMachines_Machines",
                        column: x => x.MakineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductMachines_ProductId",
                table: "ProductMachines",
                column: "ProductId");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductMachines",
                table: "ProductMachines");

            migrationBuilder.DropIndex(
                name: "IX_ProductMachines_ProductId",
                table: "ProductMachines");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ProductMachines",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductMachines",
                table: "ProductMachines",
                columns: new[] { "ProductId", "MakineId" });
        }
    }
}
