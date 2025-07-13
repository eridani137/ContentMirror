using Dapper;

namespace ContentMirror.Infrastructure;

public class PostsRepository(ConnectionFactory connectionFactory)
{
    public async Task<List<string>> GetPostTitles()
    {
        await using var connection = await connectionFactory.CreateConnection();
        
        const string sql = "SELECT post_title FROM in_posts ORDER BY created_at DESC";
        
        var titles = (await connection.QueryAsync<string>(sql)).ToList();
        
        return titles;
    }
}