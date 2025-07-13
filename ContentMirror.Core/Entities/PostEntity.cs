namespace ContentMirror.Core.Entities;

public class PostEntity
{
    public int PostId { get; set; }
    public int PostCategoryId { get; set; } = 13;
    public int PostFeedId { get; set; }
    public required string PostTitle { get; set; }
    public string PostAuthor { get; set; } = "8d22c46d56f8034123f84f5452905cab";
    public required string PostContent { get; set; }
    public required string PostExcerpt { get; set; }
    public required string PostFeaturedImage { get; set; }
    public string PostType { get; set; } = "imported_post";
    public required string PostSource { get; set; }
    public int PostHits { get; set; } = 0;
    public long PostPublishDate { get; set; } = 0;
    public long CreatedAt { get; set; } = 0;
    public long UpdatedAt { get; set; } = 0;
}