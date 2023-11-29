
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace msmd;

public static class TcpRelayServer
{
    public static async Task StartServer(CancellationToken cancellationToken)
    {
        TcpListener server = null;
        try
        {
            server = new(IPAddress.Any, Config.ListenPort);
            server.Start(); // This merely queues connection requests

            while (!cancellationToken.IsCancellationRequested)
            {
                using var client = await server.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var message = await ReadString(client);
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        var (success, response) = await TryRelay(message);
                        if (!success) response = "ERR: Can't relay to application";
                        await WriteString(client, response);
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

    private static async Task<string> ReadString(TcpClient client)
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

    private static async Task<(bool success, string response)> TryRelay(string message)
    {
        using TcpClient client = new();
        try
        {
            // try to connect to MHH
            await client.ConnectAsync(IPAddress.Loopback, Config.MHHPort).ConfigureAwait(false);
            
            // if it didn't work, try to launch MHH
            if(!client.Connected)
            {
                if (!await Task.Run(Config.Launcher)) return (false, null);
            }

            // try to connect to MHH again
            await client.ConnectAsync(IPAddress.Loopback, Config.MHHPort).ConfigureAwait(false);
            if (!client.Connected) return (false, null);

            // send the message
            await WriteString(client, message);

            // try to get a response
            var response = await ReadString(client);

            //
            return (true, response);
        }
        catch (SocketException ex)
        {
            if (ex.SocketErrorCode == SocketError.ConnectionRefused) return (false, null);
            throw;
        }
    }

    private static async Task WriteString(TcpClient client, string message)
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
}
