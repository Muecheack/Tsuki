namespace cs;

public abstract record Result<T>
{
    public record Ok(T Value) : Result<T>;

    public record Err(string Message) : Result<T>;
}