
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace msmd;

// TODO logging, finish systemd and Linux support
// https://devblogs.microsoft.com/dotnet/net-core-and-systemd/

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

        Config.Launcher = OperatingSystem.IsWindows() 
            ? WindowsLauncher 
            : LinuxLauncher;

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

    private static async Task<bool> WindowsLauncher()
    {
        if(Environment.UserInteractive)
        {

        }
        else
        {
            // https://stackoverflow.com/questions/4278373/
        }
        return false;
    }

    private static async Task<bool> LinuxLauncher()
    {
        return false;
    }
}
