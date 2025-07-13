namespace ContentMirror.Core.Entities;

public class PostEntity
{
    public int PostId { get; init; }
    public int PostCategoryId { get; init; } = 13;
    public int PostFeedId { get; init; }
    public required string PostTitle { get; init; }
    public string PostAuthor { get; init; } = "8d22c46d56f8034123f84f5452905cab";
    public required string PostContent { get; init; }
    public required string PostExcerpt { get; init; }
    public required string PostFeaturedImage { get; init; }
    public string PostType { get; init; } = "imported_post";
    public required string PostSource { get; init; }
    public int PostHits { get; init; }
    public long PostPublishDate { get; init; }
    public long CreatedAt { get; init; }
    public long UpdatedAt { get; init; }
}