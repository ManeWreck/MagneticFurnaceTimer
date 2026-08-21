using System.Text.Json;

namespace MagneticFurnaceTimer.Services;

public sealed class CloudFolderStorage
{
    private readonly string _settingsPath;

    public CloudFolderStorage(string? settingsPath = null)
    {
        _settingsPath = settingsPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MagneticFurnaceTimer",
            "settings.json");
    }

    public string? Load()
    {
        try
        {
            if (!File.Exists(_settingsPath)) return null;
            var settings = JsonSerializer.Deserialize<CloudSettings>(File.ReadAllText(_settingsPath));
            return settings?.CloudProfilesFolder;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    public void Save(string folder)
    {
        var directory = Path.GetDirectoryName(_settingsPath)!;
        Directory.CreateDirectory(directory);
        File.WriteAllText(
            _settingsPath,
            JsonSerializer.Serialize(new CloudSettings(Path.GetFullPath(folder)), new JsonSerializerOptions { WriteIndented = true }));
    }

    private sealed record CloudSettings(string CloudProfilesFolder);
}
