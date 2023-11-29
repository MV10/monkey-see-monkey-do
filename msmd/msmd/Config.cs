
using System.Diagnostics;

namespace msmd;

public static class Config
{
    public static readonly string ConfigFilename = "mhh.conf";
    public static readonly string DebugConfigFilename = "mhh.debug.conf";
    public static readonly string ConfigLocationEnvironmentVariable = "monkey-hi-hat-config";

    public static readonly int ProcessStarttMillisec = 5000;

    // mhh.conf UnsecuredRelayPort
    public static int ListenPort = 0;

    // mhh.conf UnsecuredPort
    public static int MHHPort = 0;

    // callback used when TryConnect fails
    public static Func<Task<bool>> Launcher;

    static Config()
    {
        var pathname = FindAppConfig();
        if (pathname is null) return;

        foreach (var rawline in File.ReadAllLines(pathname))
        {
            var line = rawline.Trim();
            if (!line.StartsWith("#") && line.Contains("="))
            {
                var s1 = line.Split("=");  // setting=value
                var s2 = s1[1].Split("#"); // ditch any comment after the value
                var setting = s1[0].Trim().ToLowerInvariant();
                if (Int32.TryParse(s2[0].Trim(), out int value) && value > 0 && value < 65536)
                {
                    if (setting == "unsecuredport") MHHPort = value;
                    if (setting == "unsecuredrelayport") ListenPort = value;

                    if (MHHPort > 0 && ListenPort > 0) break;
                }
            }
        }
    }

    // Based on the Program.cs method in Monkey Hi Hat by the same name
    private static string FindAppConfig()
    {
        var filename = Debugger.IsAttached ? DebugConfigFilename : ConfigFilename;

        // Path search sequence:
        // 1. Environment variable (must be complete pathname)
        // 2. App directory (preferred location)
        // 3. ConfigFiles subdirectory (might be an invalid default config; ie. invalid pathspecs)

        var pathname = Environment.GetEnvironmentVariable(ConfigLocationEnvironmentVariable);
        if (!string.IsNullOrEmpty(pathname))
        {
            pathname = Path.GetFullPath(pathname);
            if (!File.Exists(pathname) && Directory.Exists(pathname)) pathname = Path.Combine(pathname, filename);
            if (File.Exists(pathname))
            {
                Console.WriteLine($"Loading configuration via \"{ConfigLocationEnvironmentVariable}\" environment variable:\n  {pathname}");
                return pathname;
            }
        }

        pathname = Path.GetFullPath(Path.Combine($".{Path.DirectorySeparatorChar}", filename));
        if (File.Exists(pathname))
        {
            Console.WriteLine($"Loading configuration from application directory:\n  {pathname}");
            return pathname;
        }

        pathname = Path.GetFullPath(Path.Combine($".{Path.DirectorySeparatorChar}ConfigFiles", filename));
        if (File.Exists(pathname))
        {
            Console.WriteLine($"Loading configuration from ConfigFiles sub-directory:\n  {pathname}");
            return pathname;
        }

        return null;
    }


}
