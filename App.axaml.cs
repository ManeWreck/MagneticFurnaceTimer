using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MagneticFurnaceTimer.ViewModels;
using MagneticFurnaceTimer.Views;
using MagneticFurnaceTimer.Services;

namespace MagneticFurnaceTimer;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        LocalizationService.Initialize();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewModel = new MainViewModel();
            var excelArgument = desktop.Args?.FirstOrDefault(argument =>
                File.Exists(argument) &&
                (Path.GetExtension(argument).Equals(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                 Path.GetExtension(argument).Equals(".xlsm", StringComparison.OrdinalIgnoreCase)));
            if (excelArgument is not null)
            {
                viewModel.LoadProfile(excelArgument);
            }

            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
