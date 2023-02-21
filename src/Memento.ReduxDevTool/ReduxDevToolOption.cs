namespace Memento.ReduxDevTool;

public record ReduxDevToolOption {
    public string Name { get; init; } = "Memento Developer Tool";

    public uint MaximumHistoryLength { get; init; } = 50;

    public TimeSpan Latency { get; init; } = TimeSpan.FromMilliseconds(800);

    public bool StackTraceEnabled { get; init; } = true;

    public int StackTraceLinesLimit { get; init; } = 30;

    public bool OpenDevTool { get; init; } = false;
}