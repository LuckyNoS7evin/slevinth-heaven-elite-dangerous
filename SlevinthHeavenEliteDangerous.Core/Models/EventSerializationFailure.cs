namespace SlevinthHeavenEliteDangerous.Core.Models;

public class EventSerializationFailure
{
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    public string EventName { get; set; } = string.Empty;

    public string? ClrTypeName { get; set; }

    public string Error { get; set; } = string.Empty;

    public string? ExceptionType { get; set; }

    public string? SourceContext { get; set; }

    public string? RawJson { get; set; }
}
