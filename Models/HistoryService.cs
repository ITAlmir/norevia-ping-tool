using System.IO;
using System.Text.Json;
using Norevia.PingTool.Models;

namespace Norevia.PingTool.Services;

public class HistoryService
{
    private readonly string _filePath;

    public HistoryService()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Norevia", "PingTool");

        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "history.json");
    }

    public List<PingHistoryItem> Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return new List<PingHistoryItem>();

            var json = File.ReadAllText(_filePath);
            var items = JsonSerializer.Deserialize<List<PingHistoryItem>>(json) ?? new List<PingHistoryItem>();

            // newest first
            return items.OrderByDescending(x => x.Timestamp).ToList();
        }
        catch
        {
            // if file is corrupted, just ignore
            return new List<PingHistoryItem>();
        }
    }

    public void AddAndSave(PingHistoryItem item, int keepLast = 10)
    {
        var items = Load();

        items.Insert(0, item);

        // keep only last N
        items = items
            .OrderByDescending(x => x.Timestamp)
            .Take(keepLast)
            .ToList();

        var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}