namespace cs.Repo;

public class TagsUser
{
    public int Id   { get; set; }
    public int UserId { get; set; }
    public int TagsId { get; set; }
    public virtual User User { get; set; }
    public virtual Tags Tags { get; set; }
}