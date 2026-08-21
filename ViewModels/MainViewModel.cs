using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using MagneticFurnaceTimer.Models;
using MagneticFurnaceTimer.Services;

namespace MagneticFurnaceTimer.ViewModels;

public partial class MainViewModel : ViewModelBase, IDisposable
{
    private readonly DispatcherTimer _timer;

    public ObservableCollection<FurnaceRunViewModel> Furnaces { get; } = [];
    public IReadOnlyList<LanguageOption> Languages => LocalizationService.Options;

    [ObservableProperty] private FurnaceRunViewModel _selectedFurnace = null!;
    [ObservableProperty] private LanguageOption _selectedLanguage = LocalizationService.Options.First(option => option.Code == LocalizationService.CurrentCode);

    public MainViewModel()
    {
        foreach (var number in new[] { 4, 5, 6, 7 })
            Furnaces.Add(new FurnaceRunViewModel(number));

        SelectedFurnace = Furnaces[0];
        _timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, (_, _) => RefreshAll());
        _timer.Start();
        RefreshAll();
    }

    public void LoadProfile(string path) => SelectedFurnace.LoadProfile(path);

    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        LocalizationService.Apply(value.Code);
        foreach (var furnace in Furnaces) furnace.ApplyLanguage();
    }

    private void RefreshAll()
    {
        foreach (var furnace in Furnaces)
            furnace.RefreshClock();
    }

    public void Dispose() => _timer.Stop();
}
