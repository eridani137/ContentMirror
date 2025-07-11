namespace ContentMirror.Application.Services;

public class GatewayHost(ILogger<GatewayHost> logger, IHostApplicationLifetime lifetime) : IHostedService
{
    private Task _worker = null!;
    private readonly TimeSpan _delay = TimeSpan.FromHours(1);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _worker = Worker();
            logger.LogInformation("Сервис запущен");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка запуска сервиса");
        }
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.WhenAny(_worker, Task.Delay(Timeout.Infinite, cancellationToken));
            logger.LogInformation("Сервис остановлен");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка остановки сервиса");
        }
        finally
        {
            _worker.Dispose();
        }
    }

    private async Task Worker()
    {
        await Task.Delay(3000, lifetime.ApplicationStopping);
        
        while (!lifetime.ApplicationStopping.IsCancellationRequested)
        {
            try
            {
                
                await Task.Delay(_delay, lifetime.ApplicationStopping);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка в цикле обработки");
            }
        }
    }
}