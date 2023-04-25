namespace Shared;

public record Poslog
{
    public int StoreId { get; init; }
    public DateTime DateTime { get; init; }
    public string Content { get; init; }
}