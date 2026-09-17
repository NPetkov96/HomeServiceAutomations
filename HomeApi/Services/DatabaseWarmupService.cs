using DataLayer;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HomeApi.Services;

public sealed class DatabaseWarmupService(
    IServiceProvider services,
    ILogger<DatabaseWarmupService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await using var scope = services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<DataBaseContext>();

            await context.Database.OpenConnectionAsync(cancellationToken);
            await context.Database.ExecuteSqlRawAsync(
                "SELECT 1",
                cancellationToken);
            await context.Database.CloseConnectionAsync();

            logger.LogInformation(
                "Database connection pool warmed in {ElapsedMilliseconds} ms.",
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(
                "Database warm-up failed; the first request will retry initialization.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
