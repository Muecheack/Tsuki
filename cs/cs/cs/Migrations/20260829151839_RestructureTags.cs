using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace cs.Repo.Migrations
{
    /// <inheritdoc />
    public partial class RestructureTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvatorUrl");

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

            migrationBuilder.DropColumn(
                name: "Tag",
                table: "Tags");

            migrationBuilder.CreateTable(
                name: "AvatarUrl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvatarUrl", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FemaleTag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TagsId = table.Column<int>(type: "int", nullable: false)
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
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TagsId = table.Column<int>(type: "int", nullable: false)
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
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TagsId = table.Column<int>(type: "int", nullable: false)
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
                table: "FemaleTag",
                columns: new[] { "Id", "Tag", "TagsId" },
                values: new object[,]
                {
                    { 1, "xxx", 1 },
                    { 2, "xxx", 1 },
                    { 3, "xxx", 1 },
                    { 4, "xxx", 1 },
                    { 5, "xxx", 1 }
                });

            migrationBuilder.InsertData(
                table: "MaleTag",
                columns: new[] { "Id", "Tag", "TagsId" },
                values: new object[,]
                {
                    { 1, "xxx", 1 },
                    { 2, "xxx", 1 },
                    { 3, "xxx", 1 },
                    { 4, "xxx", 1 },
                    { 5, "xxx", 1 }
                });

            migrationBuilder.InsertData(
                table: "MidTag",
                columns: new[] { "Id", "Tag", "TagsId" },
                values: new object[,]
                {
                    { 1, "xxx", 1 },
                    { 2, "xxx", 1 },
                    { 3, "xxx", 1 },
                    { 4, "xxx", 1 },
                    { 5, "xxx", 1 }
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvatarUrl");

            migrationBuilder.DropTable(
                name: "FemaleTag");

            migrationBuilder.DropTable(
                name: "MaleTag");

            migrationBuilder.DropTable(
                name: "MidTag");

            migrationBuilder.AddColumn<string>(
                name: "Tag",
                table: "Tags",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AvatorUrl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvatorUrl", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AvatorUrl",
                columns: new[] { "Id", "Description", "Name", "Url" },
                values: new object[,]
                {
                    { 1, "xxx", "20200229054606_anihu", "Resource/Avator/20200229054606_anihu.jpg" },
                    { 2, "xxx", "20200229054608_qzack", "Resource/Avator/20200229054608_qzack.jpg" },
                    { 3, "xxx", "20200229054609_sfisa", "Resource/Avator/20200229054609_sfisa.jpg" },
                    { 4, "xxx", "20200229054611_eheop", "Resource/Avator/20200229054611_eheop.jpg" },
                    { 5, "xxx", "20200526220507_ksbbi", "Resource/Avator/20200526220507_ksbbi.jpg" },
                    { 6, "xxx", "20200819204403_tydvl", "Resource/Avator/20200819204403_tydvl.jpg" },
                    { 7, "xxx", "20201114165124_efbb5", "Resource/Avator/20201114165124_efbb5.png" },
                    { 8, "xxx", "20201114175727_8a7c9", "Resource/Avator/20201114175727_8a7c9.jpg" },
                    { 9, "xxx", "20220226004938_f5ce4", "Resource/Avator/20220226004938_f5ce4.png" },
                    { 10, "xxx", "2YSeem4vF6LnmN0", "Resource/Avator/2YSeem4vF6LnmN0.thumb.400_0.jpg" },
                    { 11, "xxx", "EWSoo7dytVNZ6e4", "Resource/Avator/EWSoo7dytVNZ6e4.thumb.400_0.jpg" },
                    { 12, "xxx", "OIP-C", "Resource/Avator/OIP-C.jpg" },
                    { 13, "xxx", "V2SAAv0gim04Gx2", "Resource/Avator/V2SAAv0gim04Gx2.thumb.400_0.jpg" },
                    { 14, "xxx", "YxS007AoTq1LMYn", "Resource/Avator/YxS007AoTq1LMYn.thumb.400_0.jpg" },
                    { 15, "xxx", "d3SQQVvwUqBqDxA", "Resource/Avator/d3SQQVvwUqBqDxA.thumb.400_0.jpg" },
                    { 16, "xxx", "gVSmmA0yFQnevbx", "Resource/Avator/gVSmmA0yFQnevbx.thumb.400_0.jpg" },
                    { 17, "xxx", "muecheack1", "Resource/Avator/muecheack1.jpg" },
                    { 18, "xxx", "猫娘横_头部", "Resource/Avator/猫娘横_头部.png" }
                });

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 1,
                column: "Tag",
                value: "xxx");

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "AIsId", "Tag" },
                values: new object[,]
                {
                    { 2, null, "xxx" },
                    { 3, null, "xxx" },
                    { 4, null, "xxx" },
                    { 5, null, "xxx" }
                });
        }
    }
}
