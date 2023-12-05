
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace msmd;

internal class Program
{
    static async Task Main(string[] args)
    {
        if (Config.MHHPort == 0 || Config.ListenPort == 0 || Config.Localhost is null || Config.Localhost.Length == 0)
        {
            Console.WriteLine("The monkey-hi-hat configuration file was not found or does not define UnsecuredPort and/or UnsecuredRelayPort.");
            Thread.Sleep(250); // slow-ass console
            Environment.Exit(-1);
        }

        // Despite the service/systemd statements, the program can be run
        // interactively on both Windows and Linux.

        // Due to the simplicity of the service, logging is to a local file
        // (msmd.log) in the application directory.

        var host = Host.CreateDefaultBuilder(args)
            .UseSystemd()
            .UseWindowsService(opt =>
            {
                opt.ServiceName = "Monkey Hi Hat TCP Relay (msmd)";
            })
            .ConfigureLogging(cfg =>
            {
                cfg.ClearProviders();
            })
            .ConfigureServices(svc =>
            {
                svc.AddSingleton<LogFileService>();
                svc.AddSingleton<TcpRelayServer>();
                svc.AddHostedService<RelayLauncherService>();
            })
            .Build();

        await host.RunAsync();
    }
}
