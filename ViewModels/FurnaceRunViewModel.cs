using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagneticFurnaceTimer.Models;
using MagneticFurnaceTimer.Services;

namespace MagneticFurnaceTimer.ViewModels;

public partial class FurnaceRunViewModel : ViewModelBase
{
    private static readonly CultureInfo InputCulture = CultureInfo.GetCultureInfo("ru-RU");
    private readonly ExcelProfileReader _profileReader = new();
    private readonly RunStorage _storage;
    private FurnaceProfile? _profile;
    private DateTimeOffset? _startUtc;

    public int FurnaceNumber { get; }
    public string DisplayName => LocalizationService.Format("FurnaceName", FurnaceNumber);
    public string ConfigurationCaption => LocalizationService.Format("ConfigurationCaption", FurnaceNumber);
    public ObservableCollection<StageRowViewModel> Schedule { get; } = [];

    [ObservableProperty] private string _selectorSummary = LocalizationService.Get("ProfileNotSelected");
    [ObservableProperty] private string _startDateText = DateTime.Now.ToString("dd.MM.yyyy");
    [ObservableProperty] private string _startTimeText = DateTime.Now.ToString("HH:mm");
    [ObservableProperty] private string _startInputHint = LocalizationService.Get("InputFormat");
    [ObservableProperty] private bool _hasStartInputError;
    [ObservableProperty] private string _profileName = LocalizationService.Get("ConfigurationNotLoaded");
    [ObservableProperty] private string _sourceFileName = LocalizationService.Get("ChooseExcel");
    [ObservableProperty] private string _totalDurationText = "—";
    [ObservableProperty] private string _plannedEndText = "—";
    [ObservableProperty] private string _currentStageText = LocalizationService.Get("NoActiveRun");
    [ObservableProperty] private string _currentStageDetails = LocalizationService.Get("LoadExcelHint");
    [ObservableProperty] private string _stageRemainingText = "--:--:--";
    [ObservableProperty] private string _totalRemainingText = "--:--:--";
    [ObservableProperty] private double _stageProgress;
    [ObservableProperty] private double _totalProgress;
    [ObservableProperty] private string _statusText = LocalizationService.Get("Waiting");
    [ObservableProperty] private string _statusBrush = "#64748B";
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private bool _canStart;
    [ObservableProperty] private bool _hasProfile;
    [ObservableProperty] private IReadOnlyList<TemperaturePoint> _temperaturePoints = [];
    [ObservableProperty] private double _currentMinute;
    [ObservableProperty] private double _profileTotalMinutes;
    [ObservableProperty] private double _expectedTemperature = double.NaN;
    [ObservableProperty] private string _expectedTemperatureText = "—";
    [ObservableProperty] private string _setTemperatureText = "—";
    [ObservableProperty] private string _currentRateText = "—";
    [ObservableProperty] private string _elapsedProfileText = "00:00 / 00:00";

    public FurnaceRunViewModel(int furnaceNumber, RunStorage? storage = null)
    {
        FurnaceNumber = furnaceNumber;
        _storage = storage ?? RunStorage.ForFurnace(furnaceNumber);
        RestoreSavedRun();
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
            UpdatePreview();
            RefreshClock();
        }
        catch (Exception exception)
        {
            ErrorMessage = LocalizationService.Format("ReadExcelError", exception.Message);
            HasError = true;
        }
    }

    public void ApplyLanguage()
    {
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(ConfigurationCaption));
        if (_profile is null)
        {
            ProfileName = LocalizationService.Get("ConfigurationNotLoaded");
            SourceFileName = LocalizationService.Get("ChooseExcel");
            StartInputHint = LocalizationService.Get("InputFormat");
        }
        foreach (var row in Schedule) row.ApplyLanguage();
        RefreshClock();
    }

    [RelayCommand(CanExecute = nameof(CanStartRun))]
    private void StartRun()
    {
        if (_profile is null || !TryGetSelectedStartUtc(out var selectedStartUtc)) return;
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
            ErrorMessage = LocalizationService.Format("SaveRunError", exception.Message);
            HasError = true;
        }
    }

    [RelayCommand]
    private void SetNow()
    {
        var now = DateTime.Now;
        StartDateText = now.ToString("dd.MM.yyyy");
        StartTimeText = now.ToString("HH:mm");
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
            ErrorMessage = LocalizationService.Format("ClearRunError", exception.Message);
            HasError = true;
            return;
        }

        _startUtc = null;
        SetNow();
        UpdatePreview();
        RefreshClock();
    }

    partial void OnStartDateTextChanged(string value) => UpdatePreview();
    partial void OnStartTimeTextChanged(string value) => UpdatePreview();
    partial void OnCanStartChanged(bool value) => StartRunCommand.NotifyCanExecuteChanged();

    private bool CanStartRun() => CanStart;

    private void UpdatePreview()
    {
        var inputValid = TryGetSelectedStartUtc(out var start);
        HasStartInputError = !inputValid;
        StartInputHint = inputValid ? LocalizationService.Get("LocalComputerTime") : LocalizationService.Get("EnterDateTime");
        CanStart = _profile is not null && inputValid;

        if (_profile is not null && _startUtc is null && inputValid)
        {
            RebuildSchedule(start);
            PlannedEndText = start.AddMinutes(_profile.TotalMinutes).ToLocalTime().ToString("dd.MM.yyyy  HH:mm:ss");
        }
    }

    private void RestoreSavedRun()
    {
        var saved = _storage.Load();
        if (saved is null && FurnaceNumber == 4)
        {
            saved = new RunStorage().Load();
            if (saved is not null)
            {
                try { _storage.Save(saved); }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) { }
            }
        }

        if (saved is null || saved.Profile.Stages.Count == 0) return;

        _profile = saved.Profile;
        _startUtc = saved.StartUtc;
        var localStart = saved.StartUtc.ToLocalTime();
        StartDateText = localStart.ToString("dd.MM.yyyy");
        StartTimeText = localStart.ToString("HH:mm");
        ApplyProfile(saved.Profile);
        RebuildSchedule(saved.StartUtc);
    }

    private void ApplyProfile(FurnaceProfile profile)
    {
        ProfileName = profile.Name;
        SourceFileName = Path.GetFileName(profile.SourceFile);
        TotalDurationText = FormatDuration(TimeSpan.FromMinutes(profile.TotalMinutes));
        ProfileTotalMinutes = profile.TotalMinutes;
        TemperaturePoints = TemperatureTimeline.BuildPoints(profile);
        HasProfile = true;
        CanStart = TryGetSelectedStartUtc(out _);
    }

    private bool TryGetSelectedStartUtc(out DateTimeOffset startUtc)
    {
        var dateValid = DateTime.TryParseExact(
            StartDateText.Trim(), ["dd.MM.yyyy", "d.M.yyyy"], InputCulture,
            DateTimeStyles.AllowWhiteSpaces, out var date);
        var timeValid = TimeSpan.TryParseExact(
            StartTimeText.Trim(), ["hh\\:mm", "h\\:mm"], InputCulture, out var time);

        if (!dateValid || !timeValid || time >= TimeSpan.FromDays(1))
        {
            startUtc = default;
            return false;
        }

        var local = DateTime.SpecifyKind(date.Date.Add(time), DateTimeKind.Local);
        startUtc = new DateTimeOffset(local).ToUniversalTime();
        return true;
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

    public void RefreshClock()
    {
        if (_profile is null || _startUtc is null)
        {
            StatusText = LocalizationService.Get("Waiting");
            StatusBrush = "#64748B";
            CurrentStageText = _profile is null ? LocalizationService.Get("NoActiveRun") : LocalizationService.Get("RunNotConfirmed");
            CurrentStageDetails = _profile is null ? LocalizationService.Get("LoadExcelHint") : LocalizationService.Get("ConfirmRunHint");
            StageRemainingText = "--:--:--";
            TotalRemainingText = "--:--:--";
            StageProgress = 0;
            TotalProgress = 0;
            CurrentMinute = 0;
            UpdateTemperatureMetrics(0, null);
            UpdateRowStatuses(DateTimeOffset.MinValue, false);
            SelectorSummary = _profile is null ? LocalizationService.Get("ProfileNotSelected") : ProfileName;
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var finish = _startUtc.Value.AddMinutes(_profile.TotalMinutes);
        PlannedEndText = finish.ToLocalTime().ToString("dd.MM.yyyy  HH:mm:ss");

        if (now < _startUtc.Value)
        {
            var untilStart = _startUtc.Value - now;
            StatusText = LocalizationService.Get("Scheduled");
            StatusBrush = "#3B82F6";
            CurrentStageText = LocalizationService.Get("WaitingForStart");
            CurrentStageDetails = LocalizationService.Format("StartAt", _startUtc.Value.ToLocalTime().ToString("dd.MM.yyyy  HH:mm:ss"));
            StageRemainingText = FormatClock(untilStart);
            TotalRemainingText = FormatClock(finish - now);
            StageProgress = 0;
            TotalProgress = 0;
            CurrentMinute = 0;
            UpdateTemperatureMetrics(0, null);
            UpdateRowStatuses(now, true);
            SelectorSummary = LocalizationService.Format("StartShort", _startUtc.Value.ToLocalTime().ToString("HH:mm"));
            return;
        }

        if (now >= finish)
        {
            StatusText = LocalizationService.Get("FinishedRemove");
            StatusBrush = "#EF4444";
            CurrentStageText = LocalizationService.Get("ProfileFinished");
            CurrentStageDetails = LocalizationService.Format("CalculatedFinish", finish.ToLocalTime().ToString("dd.MM.yyyy  HH:mm:ss"));
            StageRemainingText = "00:00:00";
            TotalRemainingText = "00:00:00";
            StageProgress = 100;
            TotalProgress = 100;
            CurrentMinute = _profile.TotalMinutes;
            UpdateTemperatureMetrics(CurrentMinute, _profile.Stages.LastOrDefault());
            UpdateRowStatuses(now, true);
            SelectorSummary = LocalizationService.Get("RemoveNow");
            return;
        }

        var current = Schedule.First(row => now >= row.StartUtc && now < row.EndUtc);
        var stageDuration = current.EndUtc - current.StartUtc;
        var stageElapsed = now - current.StartUtc;
        var elapsed = now - _startUtc.Value;

        StatusText = LocalizationService.Get("InProgress");
        StatusBrush = "#22C55E";
        CurrentStageText = LocalizationService.Format("StageCurrent", current.StepText, current.Label);
        CurrentStageDetails = LocalizationService.Format("EndsAt", current.EndUtc.ToLocalTime().ToString("HH:mm:ss"));
        StageRemainingText = FormatClock(current.EndUtc - now);
        TotalRemainingText = FormatClock(finish - now);
        StageProgress = stageDuration.TotalSeconds <= 0 ? 100 : Math.Clamp(stageElapsed.TotalSeconds / stageDuration.TotalSeconds * 100, 0, 100);
        TotalProgress = Math.Clamp(elapsed.TotalSeconds / (finish - _startUtc.Value).TotalSeconds * 100, 0, 100);
        CurrentMinute = Math.Clamp(elapsed.TotalMinutes, 0, _profile.TotalMinutes);
        UpdateTemperatureMetrics(CurrentMinute, current.Stage);
        UpdateRowStatuses(now, true);
        SelectorSummary = LocalizationService.Format("RemainingShort", FormatShort(finish - now));
    }

    private void UpdateTemperatureMetrics(double elapsedMinutes, FurnaceStage? currentStage)
    {
        if (_profile is null)
        {
            ExpectedTemperature = double.NaN;
            ExpectedTemperatureText = "—";
            SetTemperatureText = "—";
            CurrentRateText = "—";
            ElapsedProfileText = "00:00 / 00:00";
            return;
        }

        var expected = TemperatureTimeline.GetExpectedTemperature(_profile, elapsedMinutes);
        ExpectedTemperature = expected ?? double.NaN;
        ExpectedTemperatureText = expected is null ? "—" : $"≈ {expected:0.0} °C";
        SetTemperatureText = currentStage?.SetTemperatureC is { } setpoint ? $"{setpoint:0.#} °C" : "—";
        CurrentRateText = currentStage?.RateCPerMinute is { } rate ? LocalizationService.Format("RateUnit", $"{rate:0.##}") : LocalizationService.Get("HoldRate");
        ElapsedProfileText = $"{FormatShort(TimeSpan.FromMinutes(elapsedMinutes))} / {FormatShort(TimeSpan.FromMinutes(_profile.TotalMinutes))}";
    }

    private void UpdateRowStatuses(DateTimeOffset now, bool activeRun)
    {
        foreach (var row in Schedule)
        {
            if (!activeRun || now < row.StartUtc)
            {
                row.StatusText = LocalizationService.Get("Pending");
                row.StatusBrush = "#273449";
            }
            else if (now >= row.EndUtc)
            {
                row.StatusText = LocalizationService.Get("Done");
                row.StatusBrush = "#14532D";
            }
            else
            {
                row.StatusText = LocalizationService.Get("NowStatus");
                row.StatusBrush = "#1D4ED8";
            }
        }
    }

    private static string FormatClock(TimeSpan value)
    {
        if (value < TimeSpan.Zero) value = TimeSpan.Zero;
        return $"{(int)value.TotalHours:00}:{value.Minutes:00}:{value.Seconds:00}";
    }

    private static string FormatShort(TimeSpan value) => $"{(int)value.TotalHours:00}:{value.Minutes:00}";

    private static string FormatDuration(TimeSpan value)
        => value.TotalDays >= 1
            ? $"{(int)value.TotalDays} д {value.Hours} ч {value.Minutes} мин"
            : $"{(int)value.TotalHours} ч {value.Minutes} мин";
}
