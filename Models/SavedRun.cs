namespace MagneticFurnaceTimer.Models;

public sealed record SavedRun(
    FurnaceProfile Profile,
    DateTimeOffset StartUtc,
    DateTimeOffset SavedAtUtc);
