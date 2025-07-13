using AutoMapper;
using ContentMirror.Core.Entities;
using Dapper;

namespace ContentMirror.Infrastructure;

public class PostsRepository(ConnectionFactory connectionFactory, IMapper mapper)
{
    public async Task<List<string>> GetPostTitles()
    {
        await using var connection = await connectionFactory.CreateConnection();

        const string sql = "SELECT post_title FROM in_posts ORDER BY created_at DESC";

        var titles = (await connection.QueryAsync<string>(sql)).ToList();

        return titles;
    }

    public async Task AddPost(NewsEntity newsEntity)
    {
        const string sql = """
                                       INSERT INTO in_posts (
                                           post_category_id,
                                           post_feed_id,
                                           post_title,
                                           post_author,
                                           post_content,
                                           post_excerpt,
                                           post_featured_image,
                                           post_type,
                                           post_source,
                                           post_hits,
                                           post_pubdate,
                                           created_at,
                                           updated_at
                                       )
                                       VALUES (
                                           @PostCategoryId,
                                           @PostFeedId,
                                           @PostTitle,
                                           @PostAuthor,
                                           @PostContent,
                                           @PostExcerpt,
                                           @PostFeaturedImage,
                                           @PostType,
                                           @PostSource,
                                           @PostHits,
                                           @PostPublishDate,
                                           @CreatedAt,
                                           @UpdatedAt
                                       );
                                       SELECT LAST_INSERT_ID();
                           """;

        var post = mapper.Map<PostEntity>(newsEntity);

        await using var connection = await connectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync<int>(sql, post);
    }
}