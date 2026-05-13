using System.Windows;
using Norevia.PingTool.Services;
using Norevia.PingTool.ViewModels;

namespace Norevia.PingTool;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        try
        {
            InitializeComponent();

            DataContext = new MainViewModel(
                new PingService(),
                new ExportService(),
                new HistoryService()
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "XAML error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }
}