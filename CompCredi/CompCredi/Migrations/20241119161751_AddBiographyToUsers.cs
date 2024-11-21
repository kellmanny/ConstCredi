using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompCredi.Migrations
{
    /// <inheritdoc />
    public partial class AddBiographyToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentInteractionId",
                table: "Interactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_ParentInteractionId",
                table: "Interactions",
                column: "ParentInteractionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Interactions_Interactions_ParentInteractionId",
                table: "Interactions",
                column: "ParentInteractionId",
                principalTable: "Interactions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interactions_Interactions_ParentInteractionId",
                table: "Interactions");

            migrationBuilder.DropIndex(
                name: "IX_Interactions_ParentInteractionId",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "ParentInteractionId",
                table: "Interactions");
        }
    }
}
