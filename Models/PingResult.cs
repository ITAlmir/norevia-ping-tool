namespace Norevia.PingTool.Models;

public class PingResult
{
    public int Seq { get; set; }
    public bool Success { get; set; }
    public int? TimeMs { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? ErrorMessage { get; set; }

    // Helper za DataGrid prikaz
    public string Status => Success ? "OK" : "TIMEOUT/FAIL";
}