using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Win32;
using Norevia.PingTool.Models;

namespace Norevia.PingTool.Services;

public class ExportService
{
    public void ExportWithDialog(
        string host,
        IReadOnlyList<PingResult> results,
        PingSessionSummary summary)
    {
        if (results == null || results.Count == 0)
            throw new InvalidOperationException("Nothing to export.");

        var safeHost = MakeSafeFileName(host);
        var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);

        var dialog = new SaveFileDialog
        {
            Title = "Export results",
            FileName = $"ping_{safeHost}_{ts}",
            Filter = "CSV (*.csv)|*.csv|Text (*.txt)|*.txt",
            DefaultExt = ".csv",
            AddExtension = true
        };

        if (dialog.ShowDialog() != true)
            return;

        var ext = Path.GetExtension(dialog.FileName).ToLowerInvariant();
        var content = ext == ".txt"
            ? BuildTxt(host, results, summary)
            : BuildCsv(host, results, summary);

        File.WriteAllText(dialog.FileName, content, Encoding.UTF8);
    }

    private static string BuildCsv(string host, IReadOnlyList<PingResult> results, PingSessionSummary summary)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Host,{Escape(host)}");
        sb.AppendLine($"ExportedAt,{DateTime.Now:O}");
        sb.AppendLine($"Sent,{summary.Sent},Received,{summary.Received},LossPercent,{summary.LossPercent:F1}");
        sb.AppendLine($"MinMs,{summary.MinMs},AvgMs,{(summary.AvgMs.HasValue ? summary.AvgMs.Value.ToString("F1") : "")},MaxMs,{summary.MaxMs}");
        sb.AppendLine();

        sb.AppendLine("Seq,Status,TimeMs,Timestamp,Error");
        foreach (var r in results)
        {
            sb.AppendLine($"{r.Seq},{Escape(r.Status)},{r.TimeMs},{r.Timestamp:O},{Escape(r.ErrorMessage)}");
        }

        return sb.ToString();
    }

    private static string BuildTxt(string host, IReadOnlyList<PingResult> results, PingSessionSummary summary)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Norevia Ping Tool - Export");
        sb.AppendLine($"Host: {host}");
        sb.AppendLine($"Exported: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        sb.AppendLine($"Sent: {summary.Sent}");
        sb.AppendLine($"Received: {summary.Received}");
        sb.AppendLine($"Loss: {summary.LossPercent:F1}%");
        sb.AppendLine($"Min/Avg/Max: {summary.MinMs} / {(summary.AvgMs.HasValue ? summary.AvgMs.Value.ToString("F1") : "-")} / {summary.MaxMs} ms");
        sb.AppendLine();

        sb.AppendLine("Results:");
        foreach (var r in results)
        {
            var line = r.Success
                ? $"#{r.Seq} OK  {r.TimeMs} ms  @ {r.Timestamp:HH:mm:ss}"
                : $"#{r.Seq} FAIL  @ {r.Timestamp:HH:mm:ss}  ({r.ErrorMessage})";
            sb.AppendLine(line);
        }

        return sb.ToString();
    }

    private static string Escape(string? s)
    {
        s ??= "";
        if (s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r'))
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }

    private static string MakeSafeFileName(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "host";
        foreach (var c in Path.GetInvalidFileNameChars())
            s = s.Replace(c, '_');
        return s.Trim();
    }
}