
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace msmd;

internal class Program
{
    static async Task Main(string[] args)
    {
        if (Config.MHHPort == 0 || Config.ListenPort == 0)
        {
            Console.WriteLine("The monkey-hi-hat configuration file was not found or does not define UnsecuredPort and/or UnsecuredRelayPort.");
            Thread.Sleep(250); // slow-ass console
            Environment.Exit(-1);
        }

        // Despite the service/systemd statements, the program can be run
        // interactively on both Windows and Linux.

        var host = Host.CreateDefaultBuilder(args)
            .UseSystemd()
            .UseWindowsService(opt =>
            {
                opt.ServiceName = "Monkey Hi Hat TCP Relay (msmd)";
            })
            .ConfigureServices(svc =>
            {
                svc.AddHostedService<RelayLauncherService>();
            })
            .Build();

        await host.RunAsync();
    }
}
