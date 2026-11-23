using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Interfaces;

namespace PetroVision.BLL.Services;

// TCP reachability check for every supported scheme; for plain RTSP it also sends an
// OPTIONS request and expects an "RTSP/1.0 200" answer, so a random open port does not pass.
public class StreamProbe : IStreamProbe
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private static readonly Dictionary<string, int> DefaultPorts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["rtsp"] = 554,
        ["rtsps"] = 322,
        ["rtmp"] = 1935,
        ["http"] = 80,
        ["https"] = 443
    };

    public async Task<ConnectionTestResultDto> TestAsync(string streamUrl, CancellationToken ct = default)
    {
        if (!Uri.TryCreate(streamUrl.Trim(), UriKind.Absolute, out var uri) || string.IsNullOrEmpty(uri.Host))
            return Fail("Invalid stream URL. Example: rtsp://192.168.1.20:554/stream1");

        if (!DefaultPorts.TryGetValue(uri.Scheme, out var defaultPort))
            return Fail($"Unsupported scheme \"{uri.Scheme}\". Use rtsp, rtsps, rtmp, http or https.");

        var port = uri.IsDefaultPort || uri.Port <= 0 ? defaultPort : uri.Port;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(Timeout);

        var watch = Stopwatch.StartNew();
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(uri.Host, port, cts.Token);

            var message = "Connection test successful.";

            if (uri.Scheme.Equals("rtsp", StringComparison.OrdinalIgnoreCase))
            {
                var status = await RtspOptionsAsync(client.GetStream(), uri, port, cts.Token);

                if (status is null || !status.StartsWith("RTSP/1.0", StringComparison.Ordinal))
                    return Fail("Host is reachable but did not answer as an RTSP server.");

                // 401: it is an RTSP camera, it just wants credentials for OPTIONS (we never send them).
                if (status.StartsWith("RTSP/1.0 401", StringComparison.Ordinal))
                    message = "RTSP camera reachable (it requires authentication).";
                else if (!status.StartsWith("RTSP/1.0 200", StringComparison.Ordinal))
                    return Fail($"RTSP server answered: {status}");
            }

            watch.Stop();
            return new ConnectionTestResultDto
            {
                Success = true,
                Message = message,
                LatencyMs = watch.ElapsedMilliseconds
            };
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return Fail($"Connection timed out after {Timeout.TotalSeconds:0} seconds.");
        }
        catch (SocketException ex)
        {
            return Fail($"Connection failed: {ex.SocketErrorCode}.");
        }
        catch (IOException)
        {
            return Fail("Connection was closed by the camera.");
        }
    }

    private static async Task<string?> RtspOptionsAsync(NetworkStream stream, Uri uri, int port, CancellationToken ct)
    {
        // Rebuilt without user:password so credentials are never sent in clear text.
        var target = $"rtsp://{uri.Host}:{port}{uri.AbsolutePath}";
        var request = $"OPTIONS {target} RTSP/1.0\r\nCSeq: 1\r\nUser-Agent: PetroVision\r\n\r\n";
        await stream.WriteAsync(Encoding.ASCII.GetBytes(request), ct);

        var buffer = new byte[512];
        var read = await stream.ReadAsync(buffer, ct);
        if (read == 0) return null;

        var text = Encoding.ASCII.GetString(buffer, 0, read);
        var lineEnd = text.IndexOf('\r');
        return lineEnd > 0 ? text[..lineEnd] : text;
    }

    private static ConnectionTestResultDto Fail(string message) => new() { Success = false, Message = message };
}
