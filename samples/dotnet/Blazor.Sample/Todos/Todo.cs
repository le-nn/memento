namespace Blazor.Sample.Todos;

/// <summary>
/// Represents Todo entity. 
/// </summary>
public record Todo {
    public required Guid TodoId { get; init; }

    public required string Text { get; init; }

    public required bool IsCompleted { get; init; }

    public required DateTime CreatedAt { get; init; }

    public DateTime? CompletedAt { get; init; }

    public static Todo CreateNew(Guid id, string text) {
        return new Todo {
            TodoId = id,
            Text = text,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public override string ToString() {
        return $"{this.Text}";
    }

    public Todo Complete() => this with {
        CompletedAt = DateTime.UtcNow,
        IsCompleted = true,
    };
}
