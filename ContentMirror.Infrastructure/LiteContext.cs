using LiteDB;

namespace ContentMirror.Infrastructure;

public class LiteContext
{
    public ILiteCollection<NewsEntry> RemovedNews { get; }
    public LiteContext()
    {
        var path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.db"));
        var db = new LiteDatabase(path);
        RemovedNews = db.GetCollection<NewsEntry>("removed_news");
    }
}

public class NewsEntry
{
    public required ObjectId Id { get; set; }
    public required string Url { get; set; }
}