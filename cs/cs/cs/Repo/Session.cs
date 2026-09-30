namespace cs.Repo;

public class Session
{
    public int Id   { get; set; }
    public virtual List<SessionParticipant> Participants { get; set; } = new();
    public virtual List<Messages> MessagesList { get; set; } = new();
}