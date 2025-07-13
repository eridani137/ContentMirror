using System.Text.RegularExpressions;
using ContentMirror.Core.Configs;
using ContentMirror.Core.Entities;
using ParserExtension;

namespace ContentMirror.Application;

public static partial class Extensions
{
    public static int GetLastPageNumber(this Regex regex, List<string> hrefs)
    {
        if (hrefs.Count == 0) return 1;
        var pageNumbers = hrefs.Select(href =>
                ExtractPageNumberFromUrl(regex, href))
            .Where(pageNumber => pageNumber > 0)
            .ToList();
        return pageNumbers.Count != 0 ? pageNumbers.Max() : 1;
    }

    public static int ExtractPageNumberFromUrl(this Regex regex, string url)
    {
        if (string.IsNullOrEmpty(url)) return 0;

        var match = regex.Match(url);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var pageNumber)) return pageNumber;

        if (url.EndsWith('/') && !url.Contains("/page/")) return 1;

        return 0;
    }

    public static ImageInfo? GetHighestQualityFromSourceSimple(this ParserWrapper parser, string xpath)
    {
        var srcsets = parser.GetAttributeValues($"{xpath}", "srcset");
        var types = parser.GetAttributeValues($"{xpath}", "type");

        if (srcsets.Count == 0) return null;

        var allImages = new List<ImageInfo>();

        for (var i = 0; i < srcsets.Count; i++)
        {
            var srcset = srcsets[i];
            var type = i < types.Count ? types[i] : "";

            var images = ParseSrcset(srcset);
            foreach (var image in images)
            {
                image.Format = GetImageFormat(type, image.Url);
                allImages.Add(image);
            }
        }

        return allImages.OrderByDescending(img => img.Width).FirstOrDefault();
    }

    private static List<ImageInfo> ParseSrcset(string srcset)
    {
        var images = new List<ImageInfo>();

        if (string.IsNullOrEmpty(srcset)) return images;

        var parts = srcset.Split(',');

        foreach (var part in parts)
        {
            var trimmedPart = part.Trim();

            var match = MyRegex().Match(trimmedPart);
            if (!match.Success) continue;
            var url = match.Groups[1].Value.Trim();
            if (int.TryParse(match.Groups[2].Value, out int width))
            {
                images.Add(new ImageInfo
                {
                    Url = url,
                    Width = width
                });
            }
        }

        return images;
    }

    private static string GetImageFormat(string mimeType, string url)
    {
        if (!string.IsNullOrEmpty(mimeType))
        {
            if (mimeType.Contains("webp")) return "WebP";
            if (mimeType.Contains("jpeg")) return "JPEG";
            if (mimeType.Contains("png")) return "PNG";
            if (mimeType.Contains("avif")) return "AVIF";
        }

        if (url.Contains(".webp")) return "WebP";
        if (url.Contains(".avif")) return "AVIF";
        if (url.Contains(".jpg") || url.Contains(".jpeg")) return "JPEG";
        if (url.Contains(".png")) return "PNG";

        return "Unknown";
    }

    [GeneratedRegex(@"^(.+?)\s+(\d+)w$")]
    private static partial Regex MyRegex();
}

public class ImageInfo
{
    public string Url { get; set; }
    public int Width { get; set; }
    public string Format { get; set; }
}