using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MagneticFurnaceTimer.ViewModels;

namespace MagneticFurnaceTimer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Closed += (_, _) => (DataContext as IDisposable)?.Dispose();
    }

    private async void OpenCloud_Click(object? sender, RoutedEventArgs e)
    {
        var browser = new CloudProfileBrowserWindow();
        var path = await browser.ShowDialog<string?>(this);
        if (path is not null && DataContext is MainViewModel viewModel)
        {
            viewModel.LoadProfile(path);
        }
    }

    private async void OpenExcel_Click(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите конфигурацию магнитной печи",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Excel Workbook")
                {
                    Patterns = ["*.xlsx", "*.xlsm"],
                    MimeTypes = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"],
                },
            ],
        });

        var path = files.FirstOrDefault()?.TryGetLocalPath();
        if (path is not null && DataContext is MainViewModel viewModel)
        {
            viewModel.LoadProfile(path);
        }
    }
}
