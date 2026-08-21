using System.Globalization;
using MagneticFurnaceTimer.Models;

namespace MagneticFurnaceTimer.Services;

public sealed class CloudProfileCatalog
{
    private static readonly string[] SupportedExtensions = [".xlsx", ".xlsm"];

    public IReadOnlyList<CloudProfileItem> Scan(string rootFolder)
    {
        if (string.IsNullOrWhiteSpace(rootFolder) || !Directory.Exists(rootFolder))
            return [];

        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            MatchCasing = MatchCasing.CaseInsensitive,
            ReturnSpecialDirectories = false,
        };

        var root = Path.GetFullPath(rootFolder);
        var items = new List<CloudProfileItem>();

        foreach (var path in Directory.EnumerateFiles(root, "*", options))
        {
            if (!SupportedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
                continue;

            try
            {
                var info = new FileInfo(path);
                var relativeFolder = Path.GetDirectoryName(Path.GetRelativePath(root, path)) ?? string.Empty;
                if (relativeFolder == ".") relativeFolder = string.Empty;
                items.Add(new CloudProfileItem(
                    info.FullName,
                    Path.GetFileNameWithoutExtension(info.Name),
                    relativeFolder,
                    info.LastWriteTime));
            }
            catch (IOException)
            {
                // A cloud item may be changing while OneDrive refreshes the folder. Skip it for this scan.
            }
            catch (UnauthorizedAccessException)
            {
                // SharePoint permissions can differ between nested folders. Show the files that are accessible.
            }
        }

        return items
            .OrderByDescending(item => item.LastModifiedLocal)
            .ThenBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public static IReadOnlyList<CloudProfileItem> Filter(
        IEnumerable<CloudProfileItem> items,
        string? nameQuery,
        string? dateText,
        out bool dateIsValid)
    {
        var query = nameQuery?.Trim() ?? string.Empty;
        var dateFilter = ParseDate(dateText, out dateIsValid);

        return items.Where(item =>
                (query.Length == 0 ||
                 item.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                 item.RelativeFolder.Contains(query, StringComparison.CurrentCultureIgnoreCase)) &&
                (dateFilter is null || item.LastModifiedLocal.Date == dateFilter.Value.Date))
            .ToArray();
    }

    private static DateTime? ParseDate(string? value, out bool isValid)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            isValid = true;
            return null;
        }

        isValid = DateTime.TryParseExact(
            value.Trim(),
            ["dd.MM.yyyy", "d.M.yyyy"],
            CultureInfo.GetCultureInfo("ru-RU"),
            DateTimeStyles.AllowWhiteSpaces,
            out var date);
        return isValid ? date : null;
    }
}
