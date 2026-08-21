namespace MagneticFurnaceTimer.Models;

public sealed record FurnaceProfile(
    string Name,
    string SourceFile,
    IReadOnlyList<FurnaceStage> Stages)
{
    public double TotalMinutes => Stages.Sum(stage => stage.DurationMinutes);
}
