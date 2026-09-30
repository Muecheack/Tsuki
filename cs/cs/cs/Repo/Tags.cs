namespace cs.Repo;

// 标签库
public class Tags
{
    public int Id { get; set; }
    public string TagName { get; set; }
    public virtual List<TagsUser> User { get; set; } = new();
}