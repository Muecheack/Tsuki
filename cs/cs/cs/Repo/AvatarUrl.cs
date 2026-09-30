namespace cs.Repo;

// 头像库
public class AvatarUrl
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public string Description { get; set; }
    public virtual List<User>? Users { get; set; } = new();
}