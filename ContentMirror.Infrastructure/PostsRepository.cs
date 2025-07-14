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
    
    public async Task DeletePost(int id, int postFeedId)
    {
        const string sql = """
                               DELETE FROM in_posts
                               WHERE post_id = @PostId AND post_feed_id = @PostFeedId
                           """;

        await using var connection = await connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            PostId = id,
            PostFeedId = postFeedId
        });
    }
    
    public async Task<List<PostEntity>> GetExpiredPostsByFeedId(int feedId, long expireBeforeTimestamp)
    {
        const string sql = """
                               SELECT *
                               FROM in_posts
                               WHERE post_feed_id = @FeedId
                                 AND created_at <= @ExpireBefore
                           """;

        await using var connection = await connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<PostEntity>(sql, new
        {
            FeedId = feedId,
            ExpireBefore = expireBeforeTimestamp
        });

        return result.ToList();
    }
    
    public async Task<List<PostEntity>> GetPostsByFeedId(int postFeedId)
    {
        const string sql = """
                               SELECT
                                   post_id AS PostId,
                                   post_category_id AS PostCategoryId,
                                   post_feed_id AS PostFeedId,
                                   post_title AS PostTitle,
                                   post_author AS PostAuthor,
                                   post_content AS PostContent,
                                   post_excerpt AS PostExcerpt,
                                   post_featured_image AS PostFeaturedImage,
                                   post_type AS PostType,
                                   post_source AS PostSource,
                                   post_hits AS PostHits,
                                   post_pubdate AS PostPublishDate,
                                   created_at AS CreatedAt,
                                   updated_at AS UpdatedAt
                               FROM in_posts
                               WHERE post_feed_id = @PostFeedId
                           """;

        await using var connection = await connectionFactory.CreateConnection();

        var posts = await connection.QueryAsync<PostEntity>(sql, new { PostFeedId = postFeedId });

        return posts.ToList();
    }
}