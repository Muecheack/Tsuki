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
            // Name 是几乎所有查询的入口（登录、注册去重、按名字找 AI）。
            // SQL Server 的默认排序规则不区分大小写，而 SQLite 的 = 走 BINARY 是区分的 ——
            // 不加 NOCASE，"Alice" 和 "alice" 就会被当成两个不同的用户。
            // 注：NOCASE 只折叠 ASCII 大小写，中文本来就没有大小写，不受影响。
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .UseCollation("NOCASE");
        });

        // ── 头像库（内置字典表：初始化一次，不受外部改变）──
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
                    { Id = 18, Name = "neko_head", Url = "Resource/Avatar/neko_head.png", Description = "灰发蓝瞳可爱猫娘少女" }
            );
        });

        // ── 标签库（内置字典表：初始化一次，不受外部改变）──
        // 标签与用户多对多关联（User.TagsAI）
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
                new Tags { Id = 14, TagName = "优柔寡断" },
                new Tags { Id = 15, TagName = "清纯" },
                new Tags { Id = 16, TagName = "慵懒" },
                new Tags { Id = 17, TagName = "偶像" },
                new Tags { Id = 18, TagName = "天才" },
                new Tags { Id = 19, TagName = "中二病" }
            );
        });


        // ── 会话 / 参与者 / 消息（显式 ToTable 固定表名，与迁移一致）──
        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("Session");
        });

        modelBuilder.Entity<SessionParticipant>(entity =>
        {
            entity.ToTable("SessionParticipant");
        });

        modelBuilder.Entity<Messages>(entity =>
        {
            entity.ToTable("Messages");
        });

        // ── AI 标签关联（显式实体表）──
        // 一对 (UserId, TagsId) 全局唯一：显式化后主键是代理键 Id,
        // 唯一性得靠这个索引兜住（等价于旧隐式表的复合主键）
        modelBuilder.Entity<TagsUser>(entity =>
        {
            entity.ToTable("TagsUser");
            entity.HasIndex(e => new { e.UserId, e.TagsId }).IsUnique();
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}