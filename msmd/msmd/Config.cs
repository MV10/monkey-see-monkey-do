
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace msmd;

public static class Config
{
    public static readonly string ConfigFilename = "mhh.conf";
    public static readonly string DebugConfigFilename = "mhh.debug.conf";
    public static readonly string ConfigLocationEnvironmentVariable = "monkey-hi-hat-config";

    public static readonly int ProcessStarttMillisec = 5000;

    // mhh.conf UnsecuredPort (used by MHH itself)
    public static int MHHPort = 0;

    // mhh.conf UnsecuredRelayPort (msmd section)
    public static int ListenPort = 0;

    // mhh.conf RelayIPType (msmd section)
    public static IPAddress[] Localhost;

    static Config()
    {
        var pathname = FindAppConfig();
        if (pathname is null) return;

        AddressFamily ipType = AddressFamily.Unknown;

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
                    if (setting == "relayiptype") ipType = (value == 4) ? AddressFamily.InterNetwork : (value == 6) ? AddressFamily.InterNetworkV6 : AddressFamily.Unspecified;

                    if (MHHPort > 0 && ListenPort > 0 && ipType != AddressFamily.Unknown) break;
                }
            }
        }

        // If IPType wasn't provided, default to Unspecified (IPv4 and IPv6)
        ipType = (ipType == AddressFamily.Unknown) ? AddressFamily.Unspecified : ipType;

        Localhost = Dns.GetHostAddresses("localhost", ipType);
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
