namespace MagneticFurnaceTimer.Models;

public sealed record FurnaceStage(
    int Step,
    string Label,
    double? SetTemperatureC,
    double? RateCPerMinute,
    double DurationMinutes);
