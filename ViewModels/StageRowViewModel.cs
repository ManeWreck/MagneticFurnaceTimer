using CommunityToolkit.Mvvm.ComponentModel;
using MagneticFurnaceTimer.Models;
using MagneticFurnaceTimer.Services;

namespace MagneticFurnaceTimer.ViewModels;

public partial class StageRowViewModel : ObservableObject
{
    public required FurnaceStage Stage { get; init; }
    public required DateTimeOffset StartUtc { get; init; }
    public required DateTimeOffset EndUtc { get; init; }

    public string StepText => Stage.Step.ToString();
    public string Label => Stage.Label;
    public string TemperatureText => Stage.SetTemperatureC is { } value ? $"{value:0.#} °C" : "—";
    public string RateText => Stage.RateCPerMinute is { } value ? LocalizationService.Format("RateUnit", $"{value:0.##}") : "—";
    public string DurationText => LocalizationService.Format("MinutesShort", $"{Stage.DurationMinutes:0.#}");
    public string StartText => StartUtc.ToLocalTime().ToString("dd.MM.yyyy  HH:mm");
    public string EndText => EndUtc.ToLocalTime().ToString("dd.MM.yyyy  HH:mm");

    [ObservableProperty]
    private string _statusText = LocalizationService.Get("Pending");

    [ObservableProperty]
    private string _statusBrush = "#273449";

    public void ApplyLanguage()
    {
        OnPropertyChanged(nameof(RateText));
        OnPropertyChanged(nameof(DurationText));
    }
}
