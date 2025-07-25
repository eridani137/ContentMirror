namespace ContentMirror.Core.Entities;

public record PreviewNewsEntity
{
    public required string Url { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required DateTime Date { get; set; }
    public required string Image { get; set; }
}