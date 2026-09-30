using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cs.Repo.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_AvatarUrl_AvatarUrlId",
                table: "User");

            migrationBuilder.AlterColumn<int>(
                name: "AvatarUrlId",
                table: "User",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_User_AvatarUrl_AvatarUrlId",
                table: "User",
                column: "AvatarUrlId",
                principalTable: "AvatarUrl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_AvatarUrl_AvatarUrlId",
                table: "User");

            migrationBuilder.AlterColumn<int>(
                name: "AvatarUrlId",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_User_AvatarUrl_AvatarUrlId",
                table: "User",
                column: "AvatarUrlId",
                principalTable: "AvatarUrl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
