using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace cs.Repo.Migrations
{
    /// <inheritdoc />
    public partial class SeedAvatorAndTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AIName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AIAvatorUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
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
                name: "AvatorUrl",
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
                    table.PrimaryKey("PK_AvatorUrl", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
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
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AIsId = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AIsId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_AIs_AIsId",
                        column: x => x.AIsId,
                        principalTable: "AIs",
                        principalColumn: "Id");
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

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "AIsId", "Tag" },
                values: new object[,]
                {
                    { 1, null, "xxx" },
                    { 2, null, "xxx" },
                    { 3, null, "xxx" },
                    { 4, null, "xxx" },
                    { 5, null, "xxx" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIMessages_AIsId",
                table: "AIMessages",
                column: "AIsId");

            migrationBuilder.CreateIndex(
                name: "IX_AIs_UserId",
                table: "AIs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_AIsId",
                table: "Tags",
                column: "AIsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMessages_UserId",
                table: "UserMessages",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIMessages");

            migrationBuilder.DropTable(
                name: "AvatorUrl");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "UserMessages");

            migrationBuilder.DropTable(
                name: "AIs");
        }
    }
}
