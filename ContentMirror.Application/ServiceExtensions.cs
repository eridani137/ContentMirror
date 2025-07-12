using System.Reflection;
using ContentMirror.Application.Parsers;
using ContentMirror.Application.Parsers.Abstractions;
using ContentMirror.Core.Configs;

namespace ContentMirror.Application;

public static class ServiceExtensions
{
    public static IServiceCollection AddParsers(this IServiceCollection services)
    {
        services.AddSiteParsers();
        services.AddSingleton<ParsersFactory>();

        return services;
    }

    private static IServiceCollection AddSiteParsers(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        var parserTypes = assembly.GetTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false } &&
                typeof(ISiteParser).IsAssignableFrom(t));
        
        foreach (var type in parserTypes)
        {
            services.AddSingleton(typeof(ISiteParser), type);
        }
        
        return services;
    }
}