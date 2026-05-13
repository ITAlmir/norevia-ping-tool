using System.Net.NetworkInformation;
using Norevia.PingTool.Models;

namespace Norevia.PingTool.Services;

public class PingService
{
    public async Task<List<PingResult>> RunAsync(
        string host,
        int count,
        int timeoutMs,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host is required.", nameof(host));

        if (count <= 0) count = 4;
        if (timeoutMs <= 0) timeoutMs = 1000;

        var results = new List<PingResult>(capacity: count);

        using var ping = new Ping();

        for (int i = 1; i <= count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var r = new PingResult
            {
                Seq = i,
                Timestamp = DateTime.Now
            };

            try
            {
                // SendPingAsync ima overload sa CancellationToken u novijim .NET verzijama,
                // ali da bude kompatibilno i jednostavno, mi kontrolišemo cancellation vani.
                var reply = await ping.SendPingAsync(host, timeoutMs);

                if (reply.Status == IPStatus.Success)
                {
                    r.Success = true;
                    r.TimeMs = (int)reply.RoundtripTime;
                }
                else
                {
                    r.Success = false;
                    r.TimeMs = null;
                    r.ErrorMessage = reply.Status.ToString();
                }
            }
            catch (PingException ex)
            {
                r.Success = false;
                r.TimeMs = null;
                r.ErrorMessage = ex.InnerException?.Message ?? ex.Message;
            }
            catch (Exception ex)
            {
                r.Success = false;
                r.TimeMs = null;
                r.ErrorMessage = ex.Message;
            }

            results.Add(r);

            // mala pauza da UI diše + da ne spamuje mrežu
            if (i < count)
                await Task.Delay(250, cancellationToken);
        }

        return results;
    }
}