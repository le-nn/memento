namespace Memento.Blazor.Devtools;

public record ChromiumDevToolOption {
    public string Name { get; init; } = "Memento Developer Tool";
    public uint MaximumHistoryLength { get; init; } = 50;
    public TimeSpan Latency { get; init; } = TimeSpan.FromMilliseconds(800);
    public bool StackTraceEnabled { get; init; }
}