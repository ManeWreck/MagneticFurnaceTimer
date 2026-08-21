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

    [ObservableProperty] private string _rootFolder = LocalizationService.Get("CloudFolderNotSelected");
    [ObservableProperty] private string _nameQuery = string.Empty;
    [ObservableProperty] private string _dateText = string.Empty;
    [ObservableProperty] private string _dateHint = LocalizationService.Get("DateHint");
    [ObservableProperty] private bool _hasInvalidDate;
    [ObservableProperty] private string _statusText = LocalizationService.Get("ChooseCloudFolder");
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
            StatusText = LocalizationService.Get("FolderUnavailable");
            return;
        }

        IsBusy = true;
        StatusText = LocalizationService.Get("RefreshingProfiles");
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
            StatusText = LocalizationService.Format("CloudReadError", exception.Message);
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
        DateHint = dateValid ? LocalizationService.Get("DateHint") : LocalizationService.Get("InvalidDateHint");

        Profiles.Clear();
        foreach (var item in filtered) Profiles.Add(item);

        SelectedProfile = Profiles.FirstOrDefault();
        StatusText = HasInvalidDate
            ? LocalizationService.Get("FixDate")
            : LocalizationService.Format("ProfilesFound", Profiles.Count, _allProfiles.Count);
    }
}
