namespace Norevia.PingTool.Models;

public class PingSessionSummary
{
    public int Sent { get; set; }
    public int Received { get; set; }
    public double LossPercent { get; set; }

    public int? MinMs { get; set; }
    public double? AvgMs { get; set; }
    public int? MaxMs { get; set; }
}