
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;

namespace msmd;

public class TcpRelayServer(LogFileService logger)
{
    public async Task StartServer(CancellationToken cancellationToken)
    {
        TcpListener server = null;
        try
        {
            await logger.WriteLine($"Listening on TCP port {Config.ListenPort}");
            server = new(IPAddress.Any, Config.ListenPort);
            server.Start(); // This merely queues connection requests

            while (!cancellationToken.IsCancellationRequested)
            {
                await logger.WriteLine("Waiting for client connection");
                using var client = await server.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                await logger.WriteLine("Client connected");
                try
                {
                    var message = await ReadString(client);
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        await logger.WriteLine($"Incoming from remote: {message}");
                        var (success, response) = await TryRelay(message);
                        if (!success) response = "ERR: Can't relay to application";
                        await logger.WriteLine($"Response to remote: {response}");
                        await WriteString(client, response);
                    }
                    else
                    {
                        await logger.WriteLine("No message received from remote");
                    }
                }
                catch
                { }
                finally
                {
                    client?.Dispose();
                }
            }
        }
        catch (OperationCanceledException)
        { } // normal, disregard
        finally
        {
            server?.Stop();
        }
    }

    private async Task<string> ReadString(TcpClient client)
    {
        var response = string.Empty;

        try
        {
            var stream = client.GetStream();

            var maxBuff = Math.Max(client.SendBufferSize, client.ReceiveBufferSize);
            var messageBuffer = new byte[maxBuff];
            var bytes = await stream.ReadAsync(messageBuffer, 0, maxBuff).ConfigureAwait(false);
            await stream.FlushAsync().ConfigureAwait(false);

            response = Encoding.ASCII.GetString(messageBuffer, 0, bytes);
        }
        catch (EndOfStreamException)
        { } // normal, disregard
        catch 
        { }

        return response;
    }

    private async Task<(bool success, string response)> TryRelay(string message)
    {
        await logger.WriteLine("Trying to relay to Monkey Hi Hat");
        using TcpClient client = new();
        // try to connect to MHH; ConnectionRefused means nothing is listening
        try
        {
            await client.ConnectAsync(Config.Localhost, Config.MHHPort).ConfigureAwait(false);
        }
        catch(SocketException ex)
        {
            if (ex.SocketErrorCode != SocketError.ConnectionRefused)
            {
                await logger.WriteLine($"Exception {ex}\n{ex.Message}");
                throw;
            }
        }

        // if it didn't work, try to launch MHH
        if (!client.Connected)
        {
            await logger.WriteLine("Trying to start Monkey Hi Hat");
            if (!await LaunchMonkeyHiHat()) return (false, null);

            // try to connect to MHH again
            await logger.WriteLine("Trying to relay to Monkey Hi Hat again");
            try
            {
                await client.ConnectAsync(Config.Localhost, Config.MHHPort).ConfigureAwait(false);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode != SocketError.ConnectionRefused)
                {
                    await logger.WriteLine($"Exception {ex}\n{ex.Message}");
                    throw;
                }
            }
            if (!client.Connected) return (false, null);
        }

        // send the message
        await logger.WriteLine("Connected, relaying message");
        await WriteString(client, message);

        // try to get a response
        var response = await ReadString(client);
        return (true, response);
    }

    private async Task WriteString(TcpClient client, string message)
    {
        try
        {
            var stream = client.GetStream();

            var messageBuffer = Encoding.ASCII.GetBytes(message);
            var minBuff = Math.Min(client.SendBufferSize, client.ReceiveBufferSize);
            if (messageBuffer.Length > minBuff) throw new ArgumentException($"Message exceeds {minBuff} byte buffer size");

            await stream.WriteAsync(messageBuffer, 0, messageBuffer.Length).ConfigureAwait(false);
            await stream.FlushAsync().ConfigureAwait(false);
        }
        catch
        { }
    }

    private async Task<bool> LaunchMonkeyHiHat()
    {
        bool success = false;

        try
        {
            if (OperatingSystem.IsWindows())
            {
                if (Environment.UserInteractive)
                {
                    success = Process.Start("mhh.exe") is not null;
                }
                else
                {
                    var pathname = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mhh.exe");
                    success = ProcessExtensions.StartProcessAsCurrentUser(pathname, workDir: AppDomain.CurrentDomain.BaseDirectory);
                }
            }

            if (OperatingSystem.IsLinux())
            {
                success = Process.Start("mhh") is not null;
            }
        }
        catch(Exception ex)
        {
            await logger.WriteLine($"{ex}: {ex.Message}");
        }

        if (success)
        {
            await Task.Delay(Config.ProcessStarttMillisec);
        }
        else
        {
            await logger.WriteLine("Failed to start app");
        }

        return success;
    }
}
