using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MagneticFurnaceTimer.Services;
using MagneticFurnaceTimer.ViewModels;

namespace MagneticFurnaceTimer.Views;

public partial class CloudProfileBrowserWindow : Window
{
    private readonly CloudFolderStorage _folderStorage = new();
    private readonly CloudProfileBrowserViewModel _viewModel = new();

    public CloudProfileBrowserWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        Opened += OnOpened;
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        Opened -= OnOpened;
        var savedFolder = _folderStorage.Load();
        if (!string.IsNullOrWhiteSpace(savedFolder) && Directory.Exists(savedFolder))
        {
            await _viewModel.SetFolderAsync(savedFolder);
            return;
        }

        await ChooseFolderAsync();
    }

    private async void ChooseFolder_Click(object? sender, RoutedEventArgs e) => await ChooseFolderAsync();

    private async Task ChooseFolderAsync()
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = LocalizationService.Get("OpenCloudFolderTitle"),
            AllowMultiple = false,
        });

        var path = folders.FirstOrDefault()?.TryGetLocalPath();
        if (path is null) return;

        try
        {
            _folderStorage.Save(path);
            await _viewModel.SetFolderAsync(path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            _viewModel.StatusText = LocalizationService.Format("CloudSaveError", exception.Message);
        }
    }

    private async void Refresh_Click(object? sender, RoutedEventArgs e) => await _viewModel.RefreshAsync();

    private void ClearDate_Click(object? sender, RoutedEventArgs e) => _viewModel.ClearDate();

    private void Cancel_Click(object? sender, RoutedEventArgs e) => Close(null);

    private void OpenSelected_Click(object? sender, RoutedEventArgs e) => CloseSelected();

    private void Profile_DoubleTapped(object? sender, TappedEventArgs e) => CloseSelected();

    private void CloseSelected()
    {
        if (_viewModel.SelectedProfile is { } selected)
            Close(selected.FullPath);
    }
}
