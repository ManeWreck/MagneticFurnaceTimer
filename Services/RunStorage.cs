using System.Text.Json;
using MagneticFurnaceTimer.Models;

namespace MagneticFurnaceTimer.Services;

public sealed class RunStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly string _statePath;

    public RunStorage(string? statePath = null)
    {
        _statePath = statePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MagneticFurnaceTimer",
            "active-run.json");
    }

    public SavedRun? Load()
    {
        if (!File.Exists(_statePath)) return null;
        try
        {
            return JsonSerializer.Deserialize<SavedRun>(File.ReadAllText(_statePath), JsonOptions);
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    public void Save(SavedRun run)
    {
        var directory = Path.GetDirectoryName(_statePath)!;
        Directory.CreateDirectory(directory);
        var temporaryPath = _statePath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(run, JsonOptions));
        File.Move(temporaryPath, _statePath, true);
    }

    public void Clear()
    {
        if (File.Exists(_statePath)) File.Delete(_statePath);
    }
}
