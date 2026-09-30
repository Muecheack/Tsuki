using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cs.Repo.Migrations
{
    /// <inheritdoc />
    public partial class AddApiKeyToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // User 表已存在（脚手架创建），这里只增量补 ApiKey 列
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "User");
        }
    }
}
