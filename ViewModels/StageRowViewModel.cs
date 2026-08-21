using CommunityToolkit.Mvvm.ComponentModel;
using MagneticFurnaceTimer.Models;

namespace MagneticFurnaceTimer.ViewModels;

public partial class StageRowViewModel : ObservableObject
{
    public required FurnaceStage Stage { get; init; }
    public required DateTimeOffset StartUtc { get; init; }
    public required DateTimeOffset EndUtc { get; init; }

    public string StepText => Stage.Step.ToString();
    public string Label => Stage.Label;
    public string TemperatureText => Stage.SetTemperatureC is { } value ? $"{value:0.#} °C" : "—";
    public string RateText => Stage.RateCPerMinute is { } value ? $"{value:0.##} °C/мин" : "—";
    public string DurationText => $"{Stage.DurationMinutes:0.#} мин";
    public string StartText => StartUtc.ToLocalTime().ToString("dd.MM.yyyy  HH:mm");
    public string EndText => EndUtc.ToLocalTime().ToString("dd.MM.yyyy  HH:mm");

    [ObservableProperty]
    private string _statusText = "Ожидает";

    [ObservableProperty]
    private string _statusBrush = "#273449";
}
