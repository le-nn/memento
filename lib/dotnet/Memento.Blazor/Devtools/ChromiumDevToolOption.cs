namespace Memento.Blazor.Devtools;

public record ChromiumDevToolOption {
    public string Name { get; init; } = "Memento Developer Tool";
    public int MaximumHistoryLength { get; init; } = 5;
    public TimeSpan Latency { get; init; } = TimeSpan.FromMilliseconds(800);
    public bool StackTraceEnabled { get; init; }
}