namespace Norevia.PingTool.Models;

public class PingHistoryItem
{
    public string Host { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public int Sent { get; set; }
    public int Received { get; set; }
    public double LossPercent { get; set; }

    public double? AvgMs { get; set; }

    // nice for list display
    public string Display =>
        $"{Timestamp:yyyy-MM-dd HH:mm}  |  {Host}  |  Avg: {(AvgMs.HasValue ? AvgMs.Value.ToString("F1") : "-")} ms  |  Loss: {LossPercent:F1}%";
}