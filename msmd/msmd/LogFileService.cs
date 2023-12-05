
namespace msmd;

// A terribly inefficient approach, but log output is extremely limited.

public class LogFileService
{
    private readonly string LogFilePathname;

    public LogFileService()
    {
        LogFilePathname = Path.Combine(Directory.GetCurrentDirectory(), "msmd.log");
        try
        {
            if (File.Exists(LogFilePathname)) File.Delete(LogFilePathname);
        }
        catch { }
    }

    public async Task WriteLine(string message)
    {
        var msg = $"[{DateTime.Now}] {message}";

        try
        {
            using var writer = File.AppendText(LogFilePathname);
            await writer.WriteLineAsync(msg);
        }
        catch { }

        if (Environment.UserInteractive) Console.WriteLine(msg);
    }
}
