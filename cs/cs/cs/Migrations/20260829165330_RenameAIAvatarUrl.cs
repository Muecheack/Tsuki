using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace cs.Repo.Migrations
{
    /// <inheritdoc />
    public partial class RenameAIAvatarUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MaleTag",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MaleTag",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MaleTag",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.RenameColumn(
                name: "AIAvatorUrl",
                table: "AIs",
                newName: "AIAvatarUrl");

            migrationBuilder.UpdateData(
                table: "FemaleTag",
                keyColumn: "Id",
                keyValue: 1,
                column: "Tag",
                value: "妹妹");

            migrationBuilder.UpdateData(
                table: "FemaleTag",
                keyColumn: "Id",
                keyValue: 2,
                column: "Tag",
                value: "萝莉");

            migrationBuilder.UpdateData(
                table: "FemaleTag",
                keyColumn: "Id",
                keyValue: 3,
                column: "Tag",
                value: "姐姐");

            migrationBuilder.UpdateData(
                table: "FemaleTag",
                keyColumn: "Id",
                keyValue: 4,
                column: "Tag",
                value: "腐女");

            migrationBuilder.UpdateData(
                table: "FemaleTag",
                keyColumn: "Id",
                keyValue: 5,
                column: "Tag",
                value: "娇柔");

            migrationBuilder.InsertData(
                table: "FemaleTag",
                columns: new[] { "Id", "Tag", "TagsId" },
                values: new object[] { 6, "百合", 1 });

            migrationBuilder.UpdateData(
                table: "MaleTag",
                keyColumn: "Id",
                keyValue: 1,
                column: "Tag",
                value: "哥哥");

            migrationBuilder.UpdateData(
                table: "MaleTag",
                keyColumn: "Id",
                keyValue: 2,
                column: "Tag",
                value: "弟弟");

            migrationBuilder.UpdateData(
                table: "MidTag",
                keyColumn: "Id",
                keyValue: 1,
                column: "Tag",
                value: "傲娇");

            migrationBuilder.UpdateData(
                table: "MidTag",
                keyColumn: "Id",
                keyValue: 2,
                column: "Tag",
                value: "温柔");

            migrationBuilder.UpdateData(
                table: "MidTag",
                keyColumn: "Id",
                keyValue: 3,
                column: "Tag",
                value: "体贴");

            migrationBuilder.UpdateData(
                table: "MidTag",
                keyColumn: "Id",
                keyValue: 4,
                column: "Tag",
                value: "自恋");

            migrationBuilder.UpdateData(
                table: "MidTag",
                keyColumn: "Id",
                keyValue: 5,
                column: "Tag",
                value: "迟钝");

            migrationBuilder.InsertData(
                table: "MidTag",
                columns: new[] { "Id", "Tag", "TagsId" },
                values: new object[,]
                {
                    { 6, "优柔寡断", 1 },
                    { 7, "清纯", 1 },
                    { 8, "慵懒", 1 },
                    { 9, "偶像", 1 },
                    { 10, "天才", 1 },
                    { 11, "中二病", 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FemaleTag",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "MidTag",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "MidTag",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "MidTag",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "MidTag",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "MidTag",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "MidTag",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.RenameColumn(
                name: "AIAvatarUrl",
                table: "AIs",
                newName: "AIAvatorUrl");

            migrationBuilder.UpdateData(
                table: "FemaleTag",
                keyColumn: "Id",
                keyValue: 1,
                column: "Tag",
                value: "xxx");

            migrationBuilder.UpdateData(
                table: "FemaleTag",
                keyColumn: "Id",
                keyValue: 2,
                column: "Tag",
                value: "xxx");

            migrationBuilder.UpdateData(
                table: "FemaleTag",
                keyColumn: "Id",
                keyValue: 3,
                column: "Tag",
                value: "xxx");

            migrationBuilder.UpdateData(
                table: "FemaleTag",
                keyColumn: "Id",
                keyValue: 4,
                column: "Tag",
                value: "xxx");

            migrationBuilder.UpdateData(
                table: "FemaleTag",
                keyColumn: "Id",
                keyValue: 5,
                column: "Tag",
                value: "xxx");

            migrationBuilder.UpdateData(
                table: "MaleTag",
                keyColumn: "Id",
                keyValue: 1,
                column: "Tag",
                value: "xxx");

            migrationBuilder.UpdateData(
                table: "MaleTag",
                keyColumn: "Id",
                keyValue: 2,
                column: "Tag",
                value: "xxx");

            migrationBuilder.InsertData(
                table: "MaleTag",
                columns: new[] { "Id", "Tag", "TagsId" },
                values: new object[,]
                {
                    { 3, "xxx", 1 },
                    { 4, "xxx", 1 },
                    { 5, "xxx", 1 }
                });

            migrationBuilder.UpdateData(
                table: "MidTag",
                keyColumn: "Id",
                keyValue: 1,
                column: "Tag",
                value: "xxx");

            migrationBuilder.UpdateData(
                table: "MidTag",
                keyColumn: "Id",
                keyValue: 2,
                column: "Tag",
                value: "xxx");

            migrationBuilder.UpdateData(
                table: "MidTag",
                keyColumn: "Id",
                keyValue: 3,
                column: "Tag",
                value: "xxx");

            migrationBuilder.UpdateData(
                table: "MidTag",
                keyColumn: "Id",
                keyValue: 4,
                column: "Tag",
                value: "xxx");

            migrationBuilder.UpdateData(
                table: "MidTag",
                keyColumn: "Id",
                keyValue: 5,
                column: "Tag",
                value: "xxx");
        }
    }
}
