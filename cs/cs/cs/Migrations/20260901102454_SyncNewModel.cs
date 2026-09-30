using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace cs.Repo.Migrations
{
    /// <inheritdoc />
    public partial class SyncNewModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_AIs_AIsId",
                table: "Tags");

            migrationBuilder.DropTable(
                name: "AIMessages");

            migrationBuilder.DropTable(
                name: "FemaleTag");

            migrationBuilder.DropTable(
                name: "MaleTag");

            migrationBuilder.DropTable(
                name: "MidTag");

            migrationBuilder.DropTable(
                name: "UserMessages");

            migrationBuilder.DropTable(
                name: "AIs");

            migrationBuilder.DropIndex(
                name: "IX_Tags_AIsId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "AIsId",
                table: "Tags");

            migrationBuilder.AddColumn<string>(
                name: "AvatarAI",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AvatarUrlId",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 数据修复：现有 User 行默认值 0 无对应头像，统一指到头像 1（否则加外键约束会失败）
            migrationBuilder.Sql("UPDATE [User] SET [AvatarUrlId] = 1 WHERE [AvatarUrlId] = 0");

            migrationBuilder.AddColumn<string>(
                name: "TagName",
                table: "Tags",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Session",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Session", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TagsUser",
                columns: table => new
                {
                    TagsAIId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagsUser", x => new { x.TagsAIId, x.UserId });
                    table.ForeignKey(
                        name: "FK_TagsUser_Tags_TagsAIId",
                        column: x => x.TagsAIId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagsUser_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessionParticipant",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionParticipant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionParticipant_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionParticipant_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 1,
                column: "TagName",
                value: "哥哥");

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "TagName" },
                values: new object[,]
                {
                    { 2, "弟弟" },
                    { 3, "妹妹" },
                    { 4, "萝莉" },
                    { 5, "姐姐" },
                    { 6, "腐女" },
                    { 7, "娇柔" },
                    { 8, "百合" },
                    { 9, "傲娇" },
                    { 10, "温柔" },
                    { 11, "体贴" },
                    { 12, "自恋" },
                    { 13, "迟钝" },
                    { 14, "优柔寡断" },
                    { 15, "清纯" },
                    { 16, "慵懒" },
                    { 17, "偶像" },
                    { 18, "天才" },
                    { 19, "中二病" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_AvatarUrlId",
                table: "User",
                column: "AvatarUrlId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SessionId",
                table: "Messages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_UserId",
                table: "Messages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipant_SessionId",
                table: "SessionParticipant",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipant_UserId",
                table: "SessionParticipant",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TagsUser_UserId",
                table: "TagsUser",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_AvatarUrl_AvatarUrlId",
                table: "User",
                column: "AvatarUrlId",
                principalTable: "AvatarUrl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // 移除 AvatarUrlId 的默认值 0 约束：避免将来插入 User 时静默落到不存在的头像 0 上
            migrationBuilder.Sql(
                """
                DECLARE @var nvarchar(max);
                SELECT @var = QUOTENAME([d].[name])
                FROM [sys].[default_constraints] [d]
                INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                WHERE ([d].[parent_object_id] = OBJECT_ID(N'[User]') AND [c].[name] = N'AvatarUrlId');
                IF @var IS NOT NULL EXEC(N'ALTER TABLE [User] DROP CONSTRAINT ' + @var + ';');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_AvatarUrl_AvatarUrlId",
                table: "User");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "SessionParticipant");

            migrationBuilder.DropTable(
                name: "TagsUser");

            migrationBuilder.DropTable(
                name: "Session");

            migrationBuilder.DropIndex(
                name: "IX_User_AvatarUrlId",
                table: "User");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DropColumn(
                name: "AvatarAI",
                table: "User");

            migrationBuilder.DropColumn(
                name: "AvatarUrlId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "TagName",
                table: "Tags");

            migrationBuilder.AddColumn<int>(
                name: "AIsId",
                table: "Tags",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AIs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AIAvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AIName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIs_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FemaleTag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TagsId = table.Column<int>(type: "int", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FemaleTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FemaleTag_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaleTag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TagsId = table.Column<int>(type: "int", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaleTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaleTag_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MidTag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TagsId = table.Column<int>(type: "int", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MidTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MidTag_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMessages_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AIsId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIMessages_AIs_AIsId",
                        column: x => x.AIsId,
                        principalTable: "AIs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FemaleTag",
                columns: new[] { "Id", "Tag", "TagsId" },
                values: new object[,]
                {
                    { 1, "妹妹", 1 },
                    { 2, "萝莉", 1 },
                    { 3, "姐姐", 1 },
                    { 4, "腐女", 1 },
                    { 5, "娇柔", 1 },
                    { 6, "百合", 1 }
                });

            migrationBuilder.InsertData(
                table: "MaleTag",
                columns: new[] { "Id", "Tag", "TagsId" },
                values: new object[,]
                {
                    { 1, "哥哥", 1 },
                    { 2, "弟弟", 1 }
                });

            migrationBuilder.InsertData(
                table: "MidTag",
                columns: new[] { "Id", "Tag", "TagsId" },
                values: new object[,]
                {
                    { 1, "傲娇", 1 },
                    { 2, "温柔", 1 },
                    { 3, "体贴", 1 },
                    { 4, "自恋", 1 },
                    { 5, "迟钝", 1 },
                    { 6, "优柔寡断", 1 },
                    { 7, "清纯", 1 },
                    { 8, "慵懒", 1 },
                    { 9, "偶像", 1 },
                    { 10, "天才", 1 },
                    { 11, "中二病", 1 }
                });

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 1,
                column: "AIsId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_AIsId",
                table: "Tags",
                column: "AIsId");

            migrationBuilder.CreateIndex(
                name: "IX_AIMessages_AIsId",
                table: "AIMessages",
                column: "AIsId");

            migrationBuilder.CreateIndex(
                name: "IX_AIs_UserId",
                table: "AIs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FemaleTag_TagsId",
                table: "FemaleTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_MaleTag_TagsId",
                table: "MaleTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_MidTag_TagsId",
                table: "MidTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMessages_UserId",
                table: "UserMessages",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_AIs_AIsId",
                table: "Tags",
                column: "AIsId",
                principalTable: "AIs",
                principalColumn: "Id");
        }
    }
}
