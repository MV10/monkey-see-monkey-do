
using Microsoft.Extensions.Hosting;

namespace msmd;

public class RelayLauncherService(LogFileService logger, TcpRelayServer server) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await logger.WriteLine("Relay service starting");

        try
        {
            await server.StartServer(stoppingToken);
        }
        catch (TaskCanceledException)
        { } // normal, app is exiting

        await logger.WriteLine("Relay service exiting");
    }
}
