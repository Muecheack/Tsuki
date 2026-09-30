namespace cs.Repo;

public class Messages
{
    public int Id  { get; set; }
    public int SessionId  { get; set; }
    public int UserId  { get; set; }
    /// <summary>
    /// text | file
    /// </summary>
    public string? Type  { get; set; }
    public string? Content { get; set; }
    public  DateTime Date  { get; set; }
    public virtual User User { get; set; } = null!;
    public virtual Session Session { get; set; } = null!;
}