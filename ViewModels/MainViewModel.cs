using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Norevia.PingTool.Models;
using Norevia.PingTool.Services;
using System.Collections.ObjectModel;
using System.Threading;
using System.Linq;

namespace Norevia.PingTool.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly PingService _pingService;
    private CancellationTokenSource? _cts;
    private readonly ExportService _exportService;
    private readonly HistoryService _historyService;

    public MainViewModel(PingService pingService, ExportService exportService, HistoryService historyService)
    {
        _pingService = pingService;
        _exportService = exportService;
        _historyService = historyService;

        Host = "8.8.8.8";
        Count = 4;
        TimeoutMs = 1000;

        LoadHistory();
    }

    [ObservableProperty]
    private string host = "";

    [ObservableProperty]
    private int count;

    [ObservableProperty]
    private int timeoutMs;

    [ObservableProperty]
    private bool isRunning;

    public ObservableCollection<PingResult> Results { get; } = new();

    // Summary props
    [ObservableProperty] private int sent;
    [ObservableProperty] private int received;
    [ObservableProperty] private double lossPercent;
    [ObservableProperty] private int? minMs;
    [ObservableProperty] private double? avgMs;
    [ObservableProperty] private int? maxMs;

    private void ResetSummary()
    {
        Sent = 0;
        Received = 0;
        LossPercent = 0;
        MinMs = null;
        AvgMs = null;
        MaxMs = null;
    }

    public ObservableCollection<PingHistoryItem> History { get; } = new();

    [ObservableProperty]
    private PingHistoryItem? selectedHistoryItem;

    partial void OnSelectedHistoryItemChanged(PingHistoryItem? value)
    {
        if (value is null) return;

        // When user clicks history item, reuse host
        Host = value.Host;
    }

    private void LoadHistory()
    {
        History.Clear();
        foreach (var item in _historyService.Load())
            History.Add(item);
    }

    private PingSessionSummary GetSummary()
    {
        return new PingSessionSummary
        {
            Sent = Sent,
            Received = Received,
            LossPercent = LossPercent,
            MinMs = MinMs,
            AvgMs = AvgMs,
            MaxMs = MaxMs
        };
    }

    private void RecalcSummary()
    {
        var all = Results.ToList();
        Sent = all.Count;
        Received = all.Count(x => x.Success);

        LossPercent = Sent == 0 ? 0 : (double)(Sent - Received) / Sent * 100.0;

        var times = all.Where(x => x.Success && x.TimeMs.HasValue).Select(x => x.TimeMs!.Value).ToList();
        if (times.Count == 0)
        {
            MinMs = null;
            AvgMs = null;
            MaxMs = null;
            return;
        }

        MinMs = times.Min();
        AvgMs = times.Average();
        MaxMs = times.Max();
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAsync()
    {
        if (IsRunning) return;

        IsRunning = true;
        StartCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();

        Results.Clear();
        ResetSummary();

        _cts = new CancellationTokenSource();

        try
        {
            var list = await _pingService.RunAsync(Host, Count, TimeoutMs, _cts.Token);

            foreach (var r in list)
                Results.Add(r);

            RecalcSummary();
            _historyService.AddAndSave(new PingHistoryItem
            {
                Host = Host,
                Timestamp = DateTime.Now,
                Sent = Sent,
                Received = Received,
                LossPercent = LossPercent,
                AvgMs = AvgMs
            });

            LoadHistory();
        }
        catch (OperationCanceledException)
        {
            // user pressed Stop
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;

            IsRunning = false;
            StartCommand.NotifyCanExecuteChanged();
            StopCommand.NotifyCanExecuteChanged();
            ExportCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    private void Export()
    {
        var list = Results.ToList();
        _exportService.ExportWithDialog(Host, list, GetSummary());
    }

    private bool CanExport() => !IsRunning && Results.Count > 0;

    private bool CanStart() => !IsRunning;

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop()
    {
        _cts?.Cancel();
    }

    private bool CanStop() => IsRunning;
}