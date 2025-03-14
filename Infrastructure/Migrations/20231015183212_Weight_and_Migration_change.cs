using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Weight_and_Migration_change : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ChangedQuantity",
                table: "Products",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ChangedWeight",
                table: "Products",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangedQuantity",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ChangedWeight",
                table: "Products");
        }
    }
}
