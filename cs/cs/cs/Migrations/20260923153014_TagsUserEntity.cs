using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cs.Repo.Migrations
{
    /// <inheritdoc />
    public partial class TagsUserEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TagsUser_Tags_TagsAIId",
                table: "TagsUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TagsUser",
                table: "TagsUser");

            migrationBuilder.RenameColumn(
                name: "TagsAIId",
                table: "TagsUser",
                newName: "TagsId");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "TagsUser",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "TagName",
                table: "Tags",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "AvatarUrl",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AvatarUrl",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AvatarUrl",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TagsUser",
                table: "TagsUser",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TagsUser_TagsId",
                table: "TagsUser",
                column: "TagsId");

            migrationBuilder.AddForeignKey(
                name: "FK_TagsUser_Tags_TagsId",
                table: "TagsUser",
                column: "TagsId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TagsUser_Tags_TagsId",
                table: "TagsUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TagsUser",
                table: "TagsUser");

            migrationBuilder.DropIndex(
                name: "IX_TagsUser_TagsId",
                table: "TagsUser");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TagsUser");

            migrationBuilder.RenameColumn(
                name: "TagsId",
                table: "TagsUser",
                newName: "TagsAIId");

            migrationBuilder.AlterColumn<string>(
                name: "TagName",
                table: "Tags",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "AvatarUrl",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AvatarUrl",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AvatarUrl",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TagsUser",
                table: "TagsUser",
                columns: new[] { "TagsAIId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TagsUser_Tags_TagsAIId",
                table: "TagsUser",
                column: "TagsAIId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
