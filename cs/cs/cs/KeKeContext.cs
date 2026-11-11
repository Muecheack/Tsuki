using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace cs.Repo;

public partial class KeKeContext : DbContext
{
    public KeKeContext()
    {
    }

    public KeKeContext(DbContextOptions<KeKeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<AvatarUrl> AvatarUrls { get; set; }
    public virtual DbSet<Tags> Tags { get; set; }
    public virtual DbSet<Session> Sessions { get; set; }
    public virtual DbSet<SessionParticipant> SessionParticipants { get; set; }
    public virtual DbSet<Messages> Messages { get; set; }
    public virtual DbSet<TagsUser> TagsUser { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("User_pk");

            entity.ToTable("User");

            entity.Property(e => e.CallName)
                .HasMaxLength(50)
                .HasColumnName("Call_Name");
            entity.Property(e => e.Gender).HasMaxLength(50);
            // Name 查询要不分大小写，SQLite 得显式加 NOCASE（中文不受影响）。
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .UseCollation("NOCASE");
        });

        // 头像库（内置数据）
        modelBuilder.Entity<AvatarUrl>(entity =>
        {
            entity.ToTable("AvatarUrl");

            entity.HasData(
                new AvatarUrl
                {
                    Id = 1, Name = "20200229054606_anihu", Url = "Resource/Avatar/20200229054606_anihu.jpg",
                    Description = "米黄色头发蓝瞳开心小萝莉双手头顶比耶正对镜头"
                },
                new AvatarUrl
                {
                    Id = 2, Name = "20200229054608_qzack", Url = "Resource/Avatar/20200229054608_qzack.jpg",
                    Description = "米黄色头发蓝瞳呆滞小萝莉双手揣兜正对镜头"
                },
                new AvatarUrl
                {
                    Id = 3, Name = "20200229054609_sfisa", Url = "Resource/Avatar/20200229054609_sfisa.jpg",
                    Description = "米黄色低马尾橙瞳好奇小萝莉侧脸看猫猫"
                },
                new AvatarUrl
                {
                    Id = 4, Name = "20200229054611_eheop", Url = "Resource/Avatar/20200229054611_eheop.jpg",
                    Description = "白发散发制服蓝瞳蠢萌小萝莉双手举抱枕在头顶正对镜头"
                },
                new AvatarUrl
                {
                    Id = 5, Name = "20200526220507_ksbbi", Url = "Resource/Avatar/20200526220507_ksbbi.jpg",
                    Description = "浅蓝发红羊角蓝瞳呆萌少女抱着抱枕正对镜头"
                },
                new AvatarUrl
                {
                    Id = 6, Name = "20200819204403_tydvl", Url = "Resource/Avatar/20200819204403_tydvl.jpg",
                    Description = "橙发卷发橙瞳温柔大姐姐单手捂嘴正对镜头"
                },
                new AvatarUrl
                {
                    Id = 7, Name = "20201114165124_efbb5", Url = "Resource/Avatar/20201114165124_efbb5.png",
                    Description = "黑发披肩发型棕瞳黑衣常服开朗少女双手举蛋糕在头顶正对镜头"
                },
                new AvatarUrl
                {
                    Id = 8, Name = "20201114175727_8a7c9", Url = "Resource/Avatar/20201114175727_8a7c9.jpg",
                    Description = "黄发卷毛散发蓝瞳可爱小萝莉正对镜头"
                },
                new AvatarUrl
                {
                    Id = 9, Name = "20220226004938_f5ce4", Url = "Resource/Avatar/20220226004938_f5ce4.png",
                    Description = "灰发双马尾黑瞳黑色华服忧郁小萝莉正对镜头"
                },
                new AvatarUrl
                {
                    Id = 10, Name = "2YSeem4vF6LnmN0", Url = "Resource/Avatar/2YSeem4vF6LnmN0.thumb.400_0.jpg",
                    Description = "红发黄瞳开朗少女手持酒杯正对镜头"
                },
                new AvatarUrl
                {
                    Id = 11, Name = "EWSoo7dytVNZ6e4", Url = "Resource/Avatar/EWSoo7dytVNZ6e4.thumb.400_0.jpg",
                    Description = "蓝发短发蓝瞳深蓝制服酷酷少女手持相机正对镜头"
                },
                new AvatarUrl
                {
                    Id = 12, Name = "OIP-C", Url = "Resource/Avatar/OIP-C.jpg", Description = "蓝发双马尾蓝瞳白色裙子阳关少女背手斜眼注释镜头"
                },
                new AvatarUrl
                {
                    Id = 13, Name = "V2SAAv0gim04Gx2", Url = "Resource/Avatar/V2SAAv0gim04Gx2.thumb.400_0.jpg",
                    Description = "棕发灰瞳制服少女眨瞳正对镜头"
                },
                new AvatarUrl
                {
                    Id = 14, Name = "YxS007AoTq1LMYn", Url = "Resource/Avatar/YxS007AoTq1LMYn.thumb.400_0.jpg",
                    Description = "蓝发碧瞳体操服慵懒萝莉扎头发正对镜头"
                },
                new AvatarUrl
                {
                    Id = 15, Name = "d3SQQVvwUqBqDxA", Url = "Resource/Avatar/d3SQQVvwUqBqDxA.thumb.400_0.jpg",
                    Description = "白发卷毛散发白色制服粉瞳好奇少女正对镜头"
                },
                new AvatarUrl
                {
                    Id = 16, Name = "gVSmmA0yFQnevbx", Url = "Resource/Avatar/gVSmmA0yFQnevbx.thumb.400_0.jpg",
                    Description = "粉发粉瞳可爱少女双手比心正对镜头"
                },
                new AvatarUrl
                {
                    Id = 17, Name = "muecheack1", Url = "Resource/Avatar/muecheack1.jpg",
                    Description = "白发蓝瞳慵懒萝莉抱着粉色抱枕困对镜头"
                },
                new AvatarUrl
                    { Id = 18, Name = "neko_head", Url = "Resource/Avatar/neko_head.png", Description = "灰发蓝瞳可爱猫娘少女" },
                new AvatarUrl
                {
                    Id = 19, Name = "20200508145507_jlkkh", Url = "Resource/Avatar/20200508145507_jlkkh.jpg",
                    Description = "棕发少年, 澄澈青眼, 微微侧头, 唇轻启, 神色懵懂, 双臂交叠, 神情柔和又茫然"
                },
                new AvatarUrl
                {
                    Id = 20, Name = "20200509112659_xchjs", Url = "Resource/Avatar/20200509112659_xchjs.jpg",
                    Description = "深紫色黑发少年, 浅灰眼眸, 侧脸微转, 唇线紧抿, 神色冷淡, 神情带着疏离感"
                },
                new AvatarUrl
                {
                    Id = 21, Name = "20200510132433_oqlyd", Url = "Resource/Avatar/20200510132433_oqlyd.jpg",
                    Description = "浅灰长发少女, 清透蓝眼眸, 抬手轻捻发丝, 神色柔和, 唇微张, 安静温婉"
                },
                new AvatarUrl
                {
                    Id = 22, Name = "20200601214042_ufuwv", Url = "Resource/Avatar/20200601214042_ufuwv.jpg",
                    Description = "白发少年, 澄澈蓝眼睛, 轻托小猫, 脸颊泛红, 眉眼柔和, 带着腼腆欢喜"
                },
                new AvatarUrl
                {
                    Id = 23, Name = "20201219221757_b4b47", Url = "Resource/Avatar/20201219221757_b4b47.jpg",
                    Description = "白发猫耳少年, 浅蓝眼眸, 手握杯子, 戴口罩, 眼神清冷, 神情淡漠"
                },
                new AvatarUrl
                {
                    Id = 24, Name = "20220212141358_c94d6", Url = "Resource/Avatar/20220212141358_c94d6.jpeg",
                    Description = "灰发少女, 紫灰眼眸, 抬手轻掩脸颊, 脸颊泛红, 眼神羞怯, 软萌温顺"
                },
                new AvatarUrl
                {
                    Id = 25, Name = "20230123231249_9d8ac.thumb.400_0",
                    Url = "Resource/Avatar/20230123231249_9d8ac.thumb.400_0.jpeg",
                    Description = "浅蓝长发少女, 澄澈蓝眸, 微微侧首, 唇轻启, 神色懵懂, 恬静可爱"
                },
                new AvatarUrl
                {
                    Id = 26, Name = "19724921082172288", Url = "Resource/Avatar/19724921082172288.png",
                    Description = "浅蓝短发少女, 橙红眼眸, 微微抬脸, 唇角轻抿, 神情温柔安静"
                },
                new AvatarUrl
                {
                    Id = 27, Name = "acgszz", Url = "Resource/Avatar/acgszz.png",
                    Description = "青蓝长卷发少女, 紫眸, 一手抿眼, 怀抱小熊玩偶, 单眼轻眨, 娇俏慵懒"
                },
                new AvatarUrl
                {
                    Id = 28, Name = "green_h_neko", Url = "Resource/Avatar/green_h_neko.png",
                    Description = "薄荷绿长发猫耳少女, 浅绿眼眸, 侧头眨眼, 脸颊泛红, 娇俏灵动"
                },
                new AvatarUrl
                {
                    Id = 29, Name = "huaban-6459710836", Url = "Resource/Avatar/huaban-6459710836.png",
                    Description = "粉发猫耳少女, 淡紫眼眸, 抬手轻抵唇边, 脸颊泛红, 羞怯温柔"
                },
                new AvatarUrl
                {
                    Id = 30, Name = "huaban-6459712303", Url = "Resource/Avatar/huaban-6459712303.png",
                    Description = "金发少女, 橙红眼眸, 比出剪刀手, 单眼眨动, 笑容明媚活泼"
                },
                new AvatarUrl
                {
                    Id = 31, Name = "R-C", Url = "Resource/Avatar/R-C.jpg",
                    Description = "浅银长发高马尾少女, 深蓝眼眸, 手轻攥衣领, 小嘴微张, 神情是软乎乎的, 充满好奇"
                },
                new AvatarUrl
                {
                    Id = 32, Name = "scs", Url = "Resource/Avatar/scs.png",
                    Description = "粉渐变长发少女, 青绿色眼眸, 裹紧衣袖, 浅浅含笑, 神情温顺软萌"
                }
            );
        });

        // 标签库和 AI 的标签关联
        modelBuilder.Entity<Tags>(entity =>
        {
            entity.ToTable("Tags");

            entity.HasData(
                new Tags { Id = 1, TagName = "哥哥" },
                new Tags { Id = 2, TagName = "弟弟" },
                new Tags { Id = 3, TagName = "妹妹", },
                new Tags { Id = 4, TagName = "萝莉", },
                new Tags { Id = 5, TagName = "姐姐", },
                new Tags { Id = 6, TagName = "腐女", },
                new Tags { Id = 7, TagName = "娇柔", },
                new Tags { Id = 8, TagName = "百合", },
                new Tags { Id = 9, TagName = "傲娇", },
                new Tags { Id = 10, TagName = "温柔", },
                new Tags { Id = 11, TagName = "体贴", },
                new Tags { Id = 12, TagName = "自恋", },
                new Tags { Id = 13, TagName = "迟钝", },
                new Tags { Id = 14, TagName = "选择困难症" },
                new Tags { Id = 15, TagName = "清纯" },
                new Tags { Id = 16, TagName = "慵懒" },
                new Tags { Id = 17, TagName = "偶像" },
                new Tags { Id = 18, TagName = "天才" },
                new Tags { Id = 19, TagName = "中二病" },
                new Tags { Id = 20, TagName = "毒舌" },
                new Tags { Id = 21, TagName = "冷漠" },
                new Tags { Id = 22, TagName = "热情" },
                new Tags { Id = 23, TagName = "病娇" },
                new Tags { Id = 24, TagName = "方言味重" },
                new Tags { Id = 25, TagName = "洋气" },
                new Tags { Id = 26, TagName = "沉稳" },
                new Tags { Id = 27, TagName = "稚气" },
                new Tags { Id = 28, TagName = "猫系人格" },
                new Tags { Id = 29, TagName = "吃货" },
                new Tags { Id = 30, TagName = "夜猫子" }
            );
        });


        // 会话、参与者、消息
        modelBuilder.Entity<Session>(entity => { entity.ToTable("Session"); });

        modelBuilder.Entity<SessionParticipant>(entity => { entity.ToTable("SessionParticipant"); });

        modelBuilder.Entity<Messages>(entity => { entity.ToTable("Messages"); });

        // 一对 (UserId, TagsId) 唯一，代理主键不够，靠唯一索引兜底。
        modelBuilder.Entity<TagsUser>(entity =>
        {
            entity.ToTable("TagsUser");
            entity.HasIndex(e => new { e.UserId, e.TagsId }).IsUnique();
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
