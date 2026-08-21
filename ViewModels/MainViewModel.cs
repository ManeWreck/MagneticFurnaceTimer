using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagneticFurnaceTimer.Models;
using MagneticFurnaceTimer.Services;

namespace MagneticFurnaceTimer.ViewModels;

public partial class MainViewModel : ViewModelBase, IDisposable
{
    private readonly ExcelProfileReader _profileReader = new();
    private readonly RunStorage _storage = new();
    private readonly DispatcherTimer _timer;
    private FurnaceProfile? _profile;
    private DateTimeOffset? _startUtc;

    public ObservableCollection<StageRowViewModel> Schedule { get; } = [];

    [ObservableProperty] private DateTimeOffset? _startDate = DateTimeOffset.Now.Date;
    [ObservableProperty] private TimeSpan? _startTime = DateTime.Now.TimeOfDay;
    [ObservableProperty] private string _profileName = "Конфигурация не загружена";
    [ObservableProperty] private string _sourceFileName = "Выберите стандартный Excel-файл печи";
    [ObservableProperty] private string _totalDurationText = "—";
    [ObservableProperty] private string _plannedEndText = "—";
    [ObservableProperty] private string _currentStageText = "Нет активного запуска";
    [ObservableProperty] private string _currentStageDetails = "Загрузите Excel и укажите время запуска";
    [ObservableProperty] private string _stageRemainingText = "--:--:--";
    [ObservableProperty] private string _totalRemainingText = "--:--:--";
    [ObservableProperty] private double _stageProgress;
    [ObservableProperty] private double _totalProgress;
    [ObservableProperty] private string _statusText = "ОЖИДАНИЕ";
    [ObservableProperty] private string _statusBrush = "#334155";
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private bool _canStart;
    [ObservableProperty] private bool _hasProfile;

    public MainViewModel()
    {
        RestoreSavedRun();
        _timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, (_, _) => RefreshClock());
        _timer.Start();
        RefreshClock();
    }

    public void LoadProfile(string path)
    {
        try
        {
            var profile = _profileReader.Read(path);
            _storage.Clear();
            _profile = profile;
            _startUtc = null;
            ApplyProfile(profile);
            ErrorMessage = string.Empty;
            HasError = false;
            RebuildSchedule(GetSelectedStartUtc());
            RefreshClock();
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Не удалось прочитать Excel: {exception.Message}";
            HasError = true;
        }
    }

    [RelayCommand(CanExecute = nameof(CanStartRun))]
    private void StartRun()
    {
        if (_profile is null) return;
        var selectedStartUtc = GetSelectedStartUtc();
        try
        {
            _storage.Save(new SavedRun(_profile, selectedStartUtc, DateTimeOffset.UtcNow));
            _startUtc = selectedStartUtc;
            HasError = false;
            ErrorMessage = string.Empty;
            RebuildSchedule(selectedStartUtc);
            RefreshClock();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            ErrorMessage = $"Запуск не сохранён: {exception.Message}";
            HasError = true;
        }
    }

    [RelayCommand]
    private void ResetRun()
    {
        try
        {
            _storage.Clear();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            ErrorMessage = $"Не удалось удалить сохранённый запуск: {exception.Message}";
            HasError = true;
            return;
        }
        _startUtc = null;
        StartDate = DateTimeOffset.Now.Date;
        StartTime = DateTime.Now.TimeOfDay;
        if (_profile is not null) RebuildSchedule(GetSelectedStartUtc());
        RefreshClock();
    }

    partial void OnStartDateChanged(DateTimeOffset? value) => UpdatePreview();
    partial void OnStartTimeChanged(TimeSpan? value) => UpdatePreview();
    partial void OnCanStartChanged(bool value) => StartRunCommand.NotifyCanExecuteChanged();

    private bool CanStartRun() => CanStart;

    private void UpdatePreview()
    {
        CanStart = _profile is not null && StartDate is not null && StartTime is not null;
        if (_profile is not null && _startUtc is null && StartDate is not null && StartTime is not null)
        {
            var start = GetSelectedStartUtc();
            RebuildSchedule(start);
            PlannedEndText = start.AddMinutes(_profile.TotalMinutes).ToLocalTime().ToString("dd.MM.yyyy  HH:mm:ss");
        }
    }

    private void RestoreSavedRun()
    {
        var saved = _storage.Load();
        if (saved is null || saved.Profile.Stages.Count == 0) return;

        _profile = saved.Profile;
        _startUtc = saved.StartUtc;
        StartDate = saved.StartUtc.ToLocalTime().Date;
        StartTime = saved.StartUtc.ToLocalTime().TimeOfDay;
        ApplyProfile(saved.Profile);
        RebuildSchedule(saved.StartUtc);
    }

    private void ApplyProfile(FurnaceProfile profile)
    {
        ProfileName = profile.Name;
        SourceFileName = Path.GetFileName(profile.SourceFile);
        TotalDurationText = FormatDuration(TimeSpan.FromMinutes(profile.TotalMinutes));
        HasProfile = true;
        CanStart = StartDate is not null && StartTime is not null;
    }

    private DateTimeOffset GetSelectedStartUtc()
    {
        var date = StartDate?.LocalDateTime.Date ?? DateTime.Today;
        var time = StartTime ?? TimeSpan.Zero;
        var local = DateTime.SpecifyKind(date.Add(time), DateTimeKind.Local);
        return new DateTimeOffset(local).ToUniversalTime();
    }

    private void RebuildSchedule(DateTimeOffset startUtc)
    {
        Schedule.Clear();
        if (_profile is null) return;

        var cursor = startUtc;
        foreach (var stage in _profile.Stages)
        {
            var end = cursor.AddMinutes(stage.DurationMinutes);
            Schedule.Add(new StageRowViewModel { Stage = stage, StartUtc = cursor, EndUtc = end });
            cursor = end;
        }
        PlannedEndText = cursor.ToLocalTime().ToString("dd.MM.yyyy  HH:mm:ss");
    }

    private void RefreshClock()
    {
        if (_profile is null || _startUtc is null)
        {
            StatusText = "ОЖИДАНИЕ";
            StatusBrush = "#334155";
            CurrentStageText = _profile is null ? "Нет активного запуска" : "Запуск ещё не подтверждён";
            CurrentStageDetails = _profile is null ? "Загрузите Excel и укажите время запуска" : "Проверьте дату и время, затем нажмите «Запустить»";
            StageRemainingText = "--:--:--";
            TotalRemainingText = "--:--:--";
            StageProgress = 0;
            TotalProgress = 0;
            UpdateRowStatuses(DateTimeOffset.MinValue, false);
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var finish = _startUtc.Value.AddMinutes(_profile.TotalMinutes);
        PlannedEndText = finish.ToLocalTime().ToString("dd.MM.yyyy  HH:mm:ss");

        if (now < _startUtc.Value)
        {
            var untilStart = _startUtc.Value - now;
            StatusText = "ЗАПЛАНИРОВАНО";
            StatusBrush = "#2563EB";
            CurrentStageText = "Ожидание запуска";
            CurrentStageDetails = $"Старт: {_startUtc.Value.ToLocalTime():dd.MM.yyyy  HH:mm:ss}";
            StageRemainingText = FormatClock(untilStart);
            TotalRemainingText = FormatClock(finish - now);
            StageProgress = 0;
            TotalProgress = 0;
            UpdateRowStatuses(now, true);
            return;
        }

        if (now >= finish)
        {
            StatusText = "ЗАВЕРШЕНО — ИЗВЛЕЧЬ ИЗ ПЕЧИ";
            StatusBrush = "#DC2626";
            CurrentStageText = "Профиль завершён";
            CurrentStageDetails = $"Расчётное окончание: {finish.ToLocalTime():dd.MM.yyyy  HH:mm:ss}";
            StageRemainingText = "00:00:00";
            TotalRemainingText = "00:00:00";
            StageProgress = 100;
            TotalProgress = 100;
            UpdateRowStatuses(now, true);
            return;
        }

        var current = Schedule.First(row => now >= row.StartUtc && now < row.EndUtc);
        var stageDuration = current.EndUtc - current.StartUtc;
        var stageElapsed = now - current.StartUtc;
        var elapsed = now - _startUtc.Value;

        StatusText = "В ПРОЦЕССЕ";
        StatusBrush = "#16A34A";
        CurrentStageText = $"Этап {current.StepText} · {current.Label}";
        CurrentStageDetails = $"Температура: {current.TemperatureText}   •   Завершится: {current.EndUtc.ToLocalTime():HH:mm:ss}";
        StageRemainingText = FormatClock(current.EndUtc - now);
        TotalRemainingText = FormatClock(finish - now);
        StageProgress = stageDuration.TotalSeconds <= 0 ? 100 : Math.Clamp(stageElapsed.TotalSeconds / stageDuration.TotalSeconds * 100, 0, 100);
        TotalProgress = Math.Clamp(elapsed.TotalSeconds / (finish - _startUtc.Value).TotalSeconds * 100, 0, 100);
        UpdateRowStatuses(now, true);
    }

    private void UpdateRowStatuses(DateTimeOffset now, bool activeRun)
    {
        foreach (var row in Schedule)
        {
            if (!activeRun || now < row.StartUtc)
            {
                row.StatusText = "Ожидает";
                row.StatusBrush = "#273449";
            }
            else if (now >= row.EndUtc)
            {
                row.StatusText = "Готово";
                row.StatusBrush = "#14532D";
            }
            else
            {
                row.StatusText = "Сейчас";
                row.StatusBrush = "#1D4ED8";
            }
        }
    }

    private static string FormatClock(TimeSpan value)
    {
        if (value < TimeSpan.Zero) value = TimeSpan.Zero;
        var totalHours = (int)value.TotalHours;
        return $"{totalHours:00}:{value.Minutes:00}:{value.Seconds:00}";
    }

    private static string FormatDuration(TimeSpan value)
        => value.TotalDays >= 1
            ? $"{(int)value.TotalDays} д {value.Hours} ч {value.Minutes} мин"
            : $"{(int)value.TotalHours} ч {value.Minutes} мин";

    public void Dispose() => _timer.Stop();
}
