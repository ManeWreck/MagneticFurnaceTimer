using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MagneticFurnaceTimer.Models;
using MagneticFurnaceTimer.Services;

namespace MagneticFurnaceTimer.ViewModels;

public partial class CloudProfileBrowserViewModel : ViewModelBase
{
    private readonly CloudProfileCatalog _catalog = new();
    private IReadOnlyList<CloudProfileItem> _allProfiles = [];

    public ObservableCollection<CloudProfileItem> Profiles { get; } = [];

    [ObservableProperty] private string _rootFolder = "Папка ещё не выбрана";
    [ObservableProperty] private string _nameQuery = string.Empty;
    [ObservableProperty] private string _dateText = string.Empty;
    [ObservableProperty] private string _dateHint = "Дата изменения: ДД.ММ.ГГГГ";
    [ObservableProperty] private bool _hasInvalidDate;
    [ObservableProperty] private string _statusText = "Выберите папку SharePoint, добавленную в OneDrive";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _hasFolder;
    [ObservableProperty] private CloudProfileItem? _selectedProfile;
    [ObservableProperty] private bool _hasSelection;

    public async Task SetFolderAsync(string folder)
    {
        RootFolder = Path.GetFullPath(folder);
        HasFolder = Directory.Exists(RootFolder);
        await RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        if (!Directory.Exists(RootFolder))
        {
            _allProfiles = [];
            HasFolder = false;
            ApplyFilter();
            StatusText = "Папка не выбрана или недоступна";
            return;
        }

        IsBusy = true;
        StatusText = "Обновляем список облачных профилей…";
        try
        {
            _allProfiles = await Task.Run(() => _catalog.Scan(RootFolder));
            HasFolder = true;
            ApplyFilter();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            _allProfiles = [];
            ApplyFilter();
            StatusText = $"Не удалось прочитать облачную папку: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnNameQueryChanged(string value) => ApplyFilter();
    partial void OnDateTextChanged(string value) => ApplyFilter();
    partial void OnSelectedProfileChanged(CloudProfileItem? value) => HasSelection = value is not null;

    public void ClearDate() => DateText = string.Empty;

    private void ApplyFilter()
    {
        var filtered = CloudProfileCatalog.Filter(_allProfiles, NameQuery, DateText, out var dateValid);
        HasInvalidDate = !dateValid;
        DateHint = dateValid ? "Дата изменения: ДД.ММ.ГГГГ" : "Неверная дата — используйте ДД.ММ.ГГГГ";

        Profiles.Clear();
        foreach (var item in filtered) Profiles.Add(item);

        SelectedProfile = Profiles.FirstOrDefault();
        StatusText = HasInvalidDate
            ? "Исправьте дату для фильтрации"
            : $"Найдено профилей: {Profiles.Count} из {_allProfiles.Count}";
    }
}
