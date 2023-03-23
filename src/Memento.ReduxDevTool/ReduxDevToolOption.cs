namespace Memento.ReduxDevTool;

/// <summary>
/// Represents the configuration options for the Redux Developer Tool middleware.
/// </summary>
public record ReduxDevToolOption {
    /// <summary>
    /// Gets or initializes the name displayed in the developer tool.
    /// </summary>
    public string Name { get; init; } = "Memento Developer Tool";

    /// <summary>
    /// Gets or initializes the maximum number of history entries stored by the developer tool.
    /// </summary>
    public uint MaximumHistoryLength { get; init; } = 50;

    /// <summary>
    /// Gets or initializes the latency for the developer tool.
    /// </summary>
    public TimeSpan Latency { get; init; } = TimeSpan.FromMilliseconds(800);

    /// <summary>
    /// Gets or initializes a value indicating whether stack traces are enabled in the developer tool.
    /// </summary>
    public bool StackTraceEnabled { get; init; } = true;

    /// <summary>
    /// Gets or initializes the limit of stack trace lines displayed in the developer tool.
    /// </summary>
    public int StackTraceLinesLimit { get; init; } = 30;

    /// <summary>
    /// Gets or initializes a value indicating whether the developer tool should be opened by default.
    /// </summary>
    public bool OpenDevTool { get; init; } = false;
}