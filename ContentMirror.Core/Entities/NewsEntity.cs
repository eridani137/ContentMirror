namespace ContentMirror.Core.Entities;

public record NewsEntity
{
    public required PreviewNewsEntity Preview { get; set; }
}