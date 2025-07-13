namespace ContentMirror.Core.Entities;

public record NewsEntity
{
    public required PreviewNewsEntity Preview { get; init; }
    public required string Article { get; init; }
    public required int FeedId { get; init; }
}