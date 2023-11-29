
using Microsoft.Extensions.Hosting;

namespace msmd;

public class RelayLauncherService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await TcpRelayServer.StartServer(stoppingToken);
        }
        catch (TaskCanceledException)
        { } // normal, app is exiting
    }
}
