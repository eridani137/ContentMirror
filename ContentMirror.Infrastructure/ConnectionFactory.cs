using ContentMirror.Core.Configs;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace ContentMirror.Infrastructure;

public class ConnectionFactory(IOptions<SiteConfig> siteConfig)
{
    public async Task<MySqlConnection> CreateConnection()
    {
        var connection = new MySqlConnection(siteConfig.Value.ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
}