namespace ContentMirror.Core.Entities;

public record PreviewNewsEntity
{
    public required string Url { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
}