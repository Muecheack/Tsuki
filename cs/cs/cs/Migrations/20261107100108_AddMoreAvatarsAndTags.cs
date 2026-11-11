using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace cs.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreAvatarsAndTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Messages",
                newName: "SendAt");

            migrationBuilder.InsertData(
                table: "AvatarUrl",
                columns: new[] { "Id", "Description", "Name", "Url" },
                values: new object[,]
                {
                    { 19, "棕发少年, 澄澈青眼, 微微侧头, 唇轻启, 神色懵懂, 双臂交叠, 神情柔和又茫然", "20200508145507_jlkkh", "Resource/Avatar/20200508145507_jlkkh.jpg" },
                    { 20, "深紫色黑发少年, 浅灰眼眸, 侧脸微转, 唇线紧抿, 神色冷淡, 神情带着疏离感", "20200509112659_xchjs", "Resource/Avatar/20200509112659_xchjs.jpg" },
                    { 21, "浅灰长发少女, 清透蓝眼眸, 抬手轻捻发丝, 神色柔和, 唇微张, 安静温婉", "20200510132433_oqlyd", "Resource/Avatar/20200510132433_oqlyd.jpg" },
                    { 22, "白发少年, 澄澈蓝眼睛, 轻托小猫, 脸颊泛红, 眉眼柔和, 带着腼腆欢喜", "20200601214042_ufuwv", "Resource/Avatar/20200601214042_ufuwv.jpg" },
                    { 23, "白发猫耳少年, 浅蓝眼眸, 手握杯子, 戴口罩, 眼神清冷, 神情淡漠", "20201219221757_b4b47", "Resource/Avatar/20201219221757_b4b47.jpg" },
                    { 24, "灰发少女, 紫灰眼眸, 抬手轻掩脸颊, 脸颊泛红, 眼神羞怯, 软萌温顺", "20220212141358_c94d6", "Resource/Avatar/20220212141358_c94d6.jpeg" },
                    { 25, "浅蓝长发少女, 澄澈蓝眸, 微微侧首, 唇轻启, 神色懵懂, 恬静可爱", "20230123231249_9d8ac.thumb.400_0", "Resource/Avatar/20230123231249_9d8ac.thumb.400_0.jpeg" },
                    { 26, "浅蓝短发少女, 橙红眼眸, 微微抬脸, 唇角轻抿, 神情温柔安静", "19724921082172288", "Resource/Avatar/19724921082172288.png" },
                    { 27, "青蓝长卷发少女, 紫眸, 一手抿眼, 怀抱小熊玩偶, 单眼轻眨, 娇俏慵懒", "acgszz", "Resource/Avatar/acgszz.png" },
                    { 28, "薄荷绿长发猫耳少女, 浅绿眼眸, 侧头眨眼, 脸颊泛红, 娇俏灵动", "green_h_neko", "Resource/Avatar/green_h_neko.png" },
                    { 29, "粉发猫耳少女, 淡紫眼眸, 抬手轻抵唇边, 脸颊泛红, 羞怯温柔", "huaban-6459710836", "Resource/Avatar/huaban-6459710836.png" },
                    { 30, "金发少女, 橙红眼眸, 比出剪刀手, 单眼眨动, 笑容明媚活泼", "huaban-6459712303", "Resource/Avatar/huaban-6459712303.png" },
                    { 31, "浅银长发高马尾少女, 深蓝眼眸, 手轻攥衣领, 小嘴微张, 神情是软乎乎的, 充满好奇", "R-C", "Resource/Avatar/R-C.jpg" },
                    { 32, "粉渐变长发少女, 青绿色眼眸, 裹紧衣袖, 浅浅含笑, 神情温顺软萌", "scs", "Resource/Avatar/scs.png" }
                });

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 14,
                column: "TagName",
                value: "选择困难症");

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "TagName" },
                values: new object[,]
                {
                    { 20, "毒舌" },
                    { 21, "冷漠" },
                    { 22, "热情" },
                    { 23, "病娇" },
                    { 24, "方言味重" },
                    { 25, "洋气" },
                    { 26, "沉稳" },
                    { 27, "稚气" },
                    { 28, "猫系人格" },
                    { 29, "吃货" },
                    { 30, "夜猫子" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AvatarUrl",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "AvatarUrl",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "AvatarUrl",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "AvatarUrl",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "AvatarUrl",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "AvatarUrl",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "AvatarUrl",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "AvatarUrl",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "AvatarUrl",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "AvatarUrl",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "AvatarUrl",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "AvatarUrl",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "AvatarUrl",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "AvatarUrl",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.RenameColumn(
                name: "SendAt",
                table: "Messages",
                newName: "Date");

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 14,
                column: "TagName",
                value: "优柔寡断");
        }
    }
}
