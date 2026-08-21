namespace MagneticFurnaceTimer.Models;

public sealed record CloudProfileItem(
    string FullPath,
    string Name,
    string RelativeFolder,
    DateTime LastModifiedLocal)
{
    public string ModifiedText => LastModifiedLocal.ToString("dd.MM.yyyy  HH:mm");
    public string LocationText => string.IsNullOrWhiteSpace(RelativeFolder) ? "Корневая папка" : RelativeFolder;
}
