using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompCredi.Migrations
{
    /// <inheritdoc />
    public partial class AddRepostFieldsToPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReposterId",
                table: "Posts",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReposterId",
                table: "Posts");
        }
    }
}
