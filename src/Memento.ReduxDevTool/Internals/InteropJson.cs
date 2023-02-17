using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Memento.ReduxDevTool.Internal;

public record StoreAction {
    [JsonPropertyName("action")]
    public required ActionItem Action { get; init; }

    [JsonPropertyName("timestamp")]
    public required long Timestamp { get; init; }

    [JsonPropertyName("stack")]
    public string? Stack { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }
}

public record ActionItemFromDevtool(
    string Type,
    JsonElement? Payload,
    string? Source
);

public record ActionItem(
    [property:JsonPropertyName("type")]
    string? Type,
    [property:JsonPropertyName("payload")]
    object? Payload,
    [property:JsonPropertyName("declaredType")]
    string? DeclaredType,
    [property:JsonPropertyName("storeName")]
    string StoreName
);

public record HistoryStateContextJson {
    [JsonPropertyName("actionsById")]
    public required Dictionary<int, StoreAction> ActionsById { get; init; }

    [JsonPropertyName("computedStates")]
    public required ImmutableArray<ComputedState> ComputedStates { get; init; }

    [JsonPropertyName("currentStateIndex")]
    public required int CurrentStateIndex { get; init; }

    [JsonPropertyName("nextActionId")]
    public required int NextActionId { get; init; }

    [JsonPropertyName("skippedActionIds")]
    public required ImmutableArray<int> SkippedActionIds { get; init; }

    [JsonPropertyName("stagedActionIds")]
    public required ImmutableArray<int> StagedActionIds { get; init; }

    [JsonPropertyName("isLocked")]
    public bool IsLocked { get; init; }

    [JsonPropertyName("isPaused")]
    public bool IsPaused { get; init; }
}

public record ComputedState(
    [property:JsonPropertyName("state")]
    object State
);