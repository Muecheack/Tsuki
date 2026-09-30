using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cs.Repo.Migrations
{
    /// <inheritdoc />
    public partial class TagsUserUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TagsUser_UserId",
                table: "TagsUser");

            migrationBuilder.CreateIndex(
                name: "IX_TagsUser_UserId_TagsId",
                table: "TagsUser",
                columns: new[] { "UserId", "TagsId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TagsUser_UserId_TagsId",
                table: "TagsUser");

            migrationBuilder.CreateIndex(
                name: "IX_TagsUser_UserId",
                table: "TagsUser",
                column: "UserId");
        }
    }
}
