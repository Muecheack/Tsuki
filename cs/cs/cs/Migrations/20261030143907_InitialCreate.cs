using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace cs.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AvatarUrl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvatarUrl", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Session",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Session", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TagName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true, collation: "NOCASE"),
                    Gender = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Call_Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    AvatarUrlId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("User_pk", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_AvatarUrl_AvatarUrlId",
                        column: x => x.AvatarUrlId,
                        principalTable: "AvatarUrl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "TagsUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    TagsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagsUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagsUser_Tags_TagsId",
                        column: x => x.TagsId,
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

            migrationBuilder.InsertData(
                table: "AvatarUrl",
                columns: new[] { "Id", "Description", "Name", "Url" },
                values: new object[,]
                {
                    { 1, "米黄色头发蓝瞳开心小萝莉双手头顶比耶正对镜头", "20200229054606_anihu", "Resource/Avatar/20200229054606_anihu.jpg" },
                    { 2, "米黄色头发蓝瞳呆滞小萝莉双手揣兜正对镜头", "20200229054608_qzack", "Resource/Avatar/20200229054608_qzack.jpg" },
                    { 3, "米黄色低马尾橙瞳好奇小萝莉侧脸看猫猫", "20200229054609_sfisa", "Resource/Avatar/20200229054609_sfisa.jpg" },
                    { 4, "白发散发制服蓝瞳蠢萌小萝莉双手举抱枕在头顶正对镜头", "20200229054611_eheop", "Resource/Avatar/20200229054611_eheop.jpg" },
                    { 5, "浅蓝发红羊角蓝瞳呆萌少女抱着抱枕正对镜头", "20200526220507_ksbbi", "Resource/Avatar/20200526220507_ksbbi.jpg" },
                    { 6, "橙发卷发橙瞳温柔大姐姐单手捂嘴正对镜头", "20200819204403_tydvl", "Resource/Avatar/20200819204403_tydvl.jpg" },
                    { 7, "黑发披肩发型棕瞳黑衣常服开朗少女双手举蛋糕在头顶正对镜头", "20201114165124_efbb5", "Resource/Avatar/20201114165124_efbb5.png" },
                    { 8, "黄发卷毛散发蓝瞳可爱小萝莉正对镜头", "20201114175727_8a7c9", "Resource/Avatar/20201114175727_8a7c9.jpg" },
                    { 9, "灰发双马尾黑瞳黑色华服忧郁小萝莉正对镜头", "20220226004938_f5ce4", "Resource/Avatar/20220226004938_f5ce4.png" },
                    { 10, "红发黄瞳开朗少女手持酒杯正对镜头", "2YSeem4vF6LnmN0", "Resource/Avatar/2YSeem4vF6LnmN0.thumb.400_0.jpg" },
                    { 11, "蓝发短发蓝瞳深蓝制服酷酷少女手持相机正对镜头", "EWSoo7dytVNZ6e4", "Resource/Avatar/EWSoo7dytVNZ6e4.thumb.400_0.jpg" },
                    { 12, "蓝发双马尾蓝瞳白色裙子阳关少女背手斜眼注释镜头", "OIP-C", "Resource/Avatar/OIP-C.jpg" },
                    { 13, "棕发灰瞳制服少女眨瞳正对镜头", "V2SAAv0gim04Gx2", "Resource/Avatar/V2SAAv0gim04Gx2.thumb.400_0.jpg" },
                    { 14, "蓝发碧瞳体操服慵懒萝莉扎头发正对镜头", "YxS007AoTq1LMYn", "Resource/Avatar/YxS007AoTq1LMYn.thumb.400_0.jpg" },
                    { 15, "白发卷毛散发白色制服粉瞳好奇少女正对镜头", "d3SQQVvwUqBqDxA", "Resource/Avatar/d3SQQVvwUqBqDxA.thumb.400_0.jpg" },
                    { 16, "粉发粉瞳可爱少女双手比心正对镜头", "gVSmmA0yFQnevbx", "Resource/Avatar/gVSmmA0yFQnevbx.thumb.400_0.jpg" },
                    { 17, "白发蓝瞳慵懒萝莉抱着粉色抱枕困对镜头", "muecheack1", "Resource/Avatar/muecheack1.jpg" },
                    { 18, "灰发蓝瞳可爱猫娘少女", "neko_head", "Resource/Avatar/neko_head.png" }
                });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "TagName" },
                values: new object[,]
                {
                    { 1, "哥哥" },
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
                name: "IX_TagsUser_TagsId",
                table: "TagsUser",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_TagsUser_UserId_TagsId",
                table: "TagsUser",
                columns: new[] { "UserId", "TagsId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_AvatarUrlId",
                table: "User",
                column: "AvatarUrlId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "SessionParticipant");

            migrationBuilder.DropTable(
                name: "TagsUser");

            migrationBuilder.DropTable(
                name: "Session");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "AvatarUrl");
        }
    }
}
