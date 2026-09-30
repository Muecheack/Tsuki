using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cs.Repo.Migrations
{
    /// <inheritdoc />
    public partial class DropAvatarAI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarAI",
                table: "User");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarAI",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
