using System.Globalization;
using Avalonia;
using MagneticFurnaceTimer.Models;

namespace MagneticFurnaceTimer.Services;

public static class LocalizationService
{
    private static readonly string LanguagePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MagneticFurnaceTimer", "language.txt");

    private static readonly Dictionary<string, Dictionary<string, string>> Languages = new()
    {
        ["ru"] = new()
        {
            ["AppTitle"] = "Контроль магнитной печи", ["HeaderOverline"] = "КОНТРОЛЬ МАГНИТНОЙ ПЕЧИ",
            ["HeaderTitle"] = "Профиль и время окончания", ["CloudButton"] = "Из облака…", ["LocalFileButton"] = "Файл на ПК…",
            ["StartTimeCaption"] = "ВРЕМЯ ЗАПУСКА — ЧАСЫ ЭТОГО ПК", ["DateCaption"] = "ДАТА  ДД.ММ.ГГГГ", ["TimeCaption"] = "ВРЕМЯ  ЧЧ:ММ",
            ["StartButton"] = "Запустить", ["NowButton"] = "Сейчас", ["ResetButton"] = "Сброс", ["TotalDurationCaption"] = "ОБЩАЯ ДЛИТЕЛЬНОСТЬ",
            ["RemoveCaption"] = "ИЗВЛЕЧЬ ИЗ ПЕЧИ", ["LiveTab"] = "Ход процесса", ["StaticTab"] = "Статичное расписание",
            ["StageRemainingCaption"] = "ОСТАЛОСЬ ЭТАПУ", ["ProfileRemainingCaption"] = "ДО ОКОНЧАНИЯ ПРОФИЛЯ",
            ["TemperatureProfileTitle"] = "Предполагаемый температурный профиль", ["TemperatureDisclaimer"] = "Расчёт по уставкам и длительности этапов Excel — не фактическое измерение",
            ["ExpectedTemperatureCaption"] = "РАСЧЁТНАЯ ТЕМПЕРАТУРА", ["SetpointCaption"] = "УСТАВКА ЭТАПА", ["RateCaption"] = "СКОРОСТЬ",
            ["ElapsedCaption"] = "ПРОШЛО / ВСЕГО", ["TransitionsTitle"] = "Переходы между этапами", ["StartCaption"] = "НАЧАЛО", ["EndCaption"] = "КОНЕЦ",
            ["StageCaption"] = "ЭТАП", ["TemperatureCaption"] = "ТЕМПЕРАТУРА", ["DurationCaption"] = "ДЛИТЕЛЬНОСТЬ", ["FinishCaption"] = "ОКОНЧАНИЕ", ["StatusCaption"] = "СТАТУС",
            ["CloudWindowTitle"] = "Профили из SharePoint", ["CloudOverline"] = "ПРОФИЛИ ИЗ SHAREPOINT / ONEDRIVE", ["CloudTitle"] = "Облачная библиотека печи",
            ["CloudSubtitle"] = "Файл загрузится из облака только после выбора", ["OtherFolderButton"] = "Другая папка…", ["RefreshButton"] = "Обновить",
            ["NameSearchCaption"] = "ПОИСК ПО НАЗВАНИЮ ИЛИ ПАПКЕ", ["NameSearchWatermark"] = "Например: D50609 или annealing", ["ModifiedDateCaption"] = "ДАТА ИЗМЕНЕНИЯ",
            ["ClearDateButton"] = "Сбросить дату", ["NameCaption"] = "НАЗВАНИЕ", ["ModifiedCaption"] = "ИЗМЕНЁН", ["FolderCaption"] = "ПАПКА",
            ["CancelButton"] = "Отмена", ["SelectProfileButton"] = "Выбрать профиль",
            ["FurnaceName"] = "Печь #{0}", ["ConfigurationCaption"] = "ПЕЧЬ #{0} · КОНФИГУРАЦИЯ EXCEL", ["Waiting"] = "ОЖИДАНИЕ",
            ["ProfileNotSelected"] = "Профиль не выбран", ["ConfigurationNotLoaded"] = "Конфигурация не загружена", ["ChooseExcel"] = "Выберите стандартный Excel-файл печи",
            ["InputFormat"] = "Формат: ДД.ММ.ГГГГ и ЧЧ:ММ", ["LocalComputerTime"] = "Локальное время этого компьютера", ["EnterDateTime"] = "Введите дату ДД.ММ.ГГГГ и время ЧЧ:ММ",
            ["NoActiveRun"] = "Нет активного запуска", ["RunNotConfirmed"] = "Запуск ещё не подтверждён", ["LoadExcelHint"] = "Загрузите Excel и укажите время запуска",
            ["ConfirmRunHint"] = "Проверьте дату и время, затем нажмите «Запустить»", ["Scheduled"] = "ЗАПЛАНИРОВАНО", ["WaitingForStart"] = "Ожидание запуска",
            ["StartAt"] = "Старт: {0}", ["StartShort"] = "Старт {0}", ["FinishedRemove"] = "ЗАВЕРШЕНО — ИЗВЛЕЧЬ ИЗ ПЕЧИ", ["RemoveNow"] = "ИЗВЛЕЧЬ ИЗ ПЕЧИ",
            ["ProfileFinished"] = "Профиль завершён", ["CalculatedFinish"] = "Расчётное окончание: {0}", ["InProgress"] = "В ПРОЦЕССЕ", ["StageCurrent"] = "Этап {0} · {1}",
            ["EndsAt"] = "Завершится в {0}", ["RemainingShort"] = "Осталось {0}", ["HoldRate"] = "Выдержка", ["Pending"] = "Ожидает", ["Done"] = "Готово", ["NowStatus"] = "Сейчас",
            ["MinutesShort"] = "{0} мин", ["RateUnit"] = "{0} °C/мин", ["CloudFolderNotSelected"] = "Папка ещё не выбрана", ["ChooseCloudFolder"] = "Выберите папку SharePoint, добавленную в OneDrive",
            ["FolderUnavailable"] = "Папка не выбрана или недоступна", ["RefreshingProfiles"] = "Обновляем список облачных профилей…", ["DateHint"] = "Дата изменения: ДД.ММ.ГГГГ",
            ["InvalidDateHint"] = "Неверная дата — используйте ДД.ММ.ГГГГ", ["FixDate"] = "Исправьте дату для фильтрации", ["ProfilesFound"] = "Найдено профилей: {0} из {1}", ["RootFolder"] = "Корневая папка",
            ["ReadExcelError"] = "Не удалось прочитать Excel: {0}", ["SaveRunError"] = "Запуск не сохранён: {0}", ["ClearRunError"] = "Не удалось удалить сохранённый запуск: {0}",
            ["OpenFileTitle"] = "Выберите конфигурацию магнитной печи", ["OpenCloudFolderTitle"] = "Выберите AnnealingResults или другую папку профилей из рабочего OneDrive",
            ["CloudReadError"] = "Не удалось прочитать облачную папку: {0}", ["CloudSaveError"] = "Не удалось сохранить папку: {0}"
        },
        ["ro"] = new()
        {
            ["AppTitle"] = "Controlul cuptorului magnetic", ["HeaderOverline"] = "CONTROLUL CUPTORULUI MAGNETIC",
            ["HeaderTitle"] = "Profil și ora finalizării", ["CloudButton"] = "Din cloud…", ["LocalFileButton"] = "Fișier pe PC…",
            ["StartTimeCaption"] = "ORA PORNIRII — CEASUL ACESTUI PC", ["DateCaption"] = "DATA  ZZ.LL.AAAA", ["TimeCaption"] = "ORA  HH:MM",
            ["StartButton"] = "Pornește", ["NowButton"] = "Acum", ["ResetButton"] = "Resetare", ["TotalDurationCaption"] = "DURATA TOTALĂ",
            ["RemoveCaption"] = "SCOATE DIN CUPTOR", ["LiveTab"] = "Proces în timp real", ["StaticTab"] = "Program static",
            ["StageRemainingCaption"] = "RĂMAS DIN ETAPĂ", ["ProfileRemainingCaption"] = "PÂNĂ LA FINALUL PROFILULUI",
            ["TemperatureProfileTitle"] = "Profil termic estimat", ["TemperatureDisclaimer"] = "Calcul după valorile și duratele din Excel — nu este o măsurare reală",
            ["ExpectedTemperatureCaption"] = "TEMPERATURA ESTIMATĂ", ["SetpointCaption"] = "VALOAREA ETAPEI", ["RateCaption"] = "VITEZA",
            ["ElapsedCaption"] = "TRECUT / TOTAL", ["TransitionsTitle"] = "Tranziții între etape", ["StartCaption"] = "START", ["EndCaption"] = "SFÂRȘIT",
            ["StageCaption"] = "ETAPĂ", ["TemperatureCaption"] = "TEMPERATURĂ", ["DurationCaption"] = "DURATĂ", ["FinishCaption"] = "FINALIZARE", ["StatusCaption"] = "STARE",
            ["CloudWindowTitle"] = "Profiluri din SharePoint", ["CloudOverline"] = "PROFILURI DIN SHAREPOINT / ONEDRIVE", ["CloudTitle"] = "Biblioteca cloud a cuptorului",
            ["CloudSubtitle"] = "Fișierul se descarcă din cloud numai după selectare", ["OtherFolderButton"] = "Alt dosar…", ["RefreshButton"] = "Reîmprospătează",
            ["NameSearchCaption"] = "CĂUTARE DUPĂ NUME SAU DOSAR", ["NameSearchWatermark"] = "De exemplu: D50609 sau annealing", ["ModifiedDateCaption"] = "DATA MODIFICĂRII",
            ["ClearDateButton"] = "Șterge data", ["NameCaption"] = "NUME", ["ModifiedCaption"] = "MODIFICAT", ["FolderCaption"] = "DOSAR",
            ["CancelButton"] = "Anulare", ["SelectProfileButton"] = "Selectează profilul",
            ["FurnaceName"] = "Cuptor #{0}", ["ConfigurationCaption"] = "CUPTOR #{0} · CONFIGURAȚIE EXCEL", ["Waiting"] = "AȘTEPTARE",
            ["ProfileNotSelected"] = "Profil neselectat", ["ConfigurationNotLoaded"] = "Configurația nu este încărcată", ["ChooseExcel"] = "Selectați fișierul Excel standard al cuptorului",
            ["InputFormat"] = "Format: ZZ.LL.AAAA și HH:MM", ["LocalComputerTime"] = "Ora locală a acestui calculator", ["EnterDateTime"] = "Introduceți data ZZ.LL.AAAA și ora HH:MM",
            ["NoActiveRun"] = "Nicio pornire activă", ["RunNotConfirmed"] = "Pornirea nu este confirmată", ["LoadExcelHint"] = "Încărcați Excel și indicați ora pornirii",
            ["ConfirmRunHint"] = "Verificați data și ora, apoi apăsați «Pornește»", ["Scheduled"] = "PROGRAMAT", ["WaitingForStart"] = "Așteptarea pornirii",
            ["StartAt"] = "Start: {0}", ["StartShort"] = "Start {0}", ["FinishedRemove"] = "FINALIZAT — SCOATE DIN CUPTOR", ["RemoveNow"] = "SCOATE DIN CUPTOR",
            ["ProfileFinished"] = "Profil finalizat", ["CalculatedFinish"] = "Finalizare calculată: {0}", ["InProgress"] = "ÎN PROCES", ["StageCurrent"] = "Etapa {0} · {1}",
            ["EndsAt"] = "Se termină la {0}", ["RemainingShort"] = "Rămas {0}", ["HoldRate"] = "Menținere", ["Pending"] = "Așteaptă", ["Done"] = "Gata", ["NowStatus"] = "Acum",
            ["MinutesShort"] = "{0} min", ["RateUnit"] = "{0} °C/min", ["CloudFolderNotSelected"] = "Dosarul nu este selectat", ["ChooseCloudFolder"] = "Selectați dosarul SharePoint adăugat în OneDrive",
            ["FolderUnavailable"] = "Dosarul nu este selectat sau disponibil", ["RefreshingProfiles"] = "Actualizăm lista profilurilor cloud…", ["DateHint"] = "Data modificării: ZZ.LL.AAAA",
            ["InvalidDateHint"] = "Dată incorectă — utilizați ZZ.LL.AAAA", ["FixDate"] = "Corectați data pentru filtrare", ["ProfilesFound"] = "Profiluri găsite: {0} din {1}", ["RootFolder"] = "Dosar rădăcină",
            ["ReadExcelError"] = "Fișierul Excel nu a putut fi citit: {0}", ["SaveRunError"] = "Pornirea nu a fost salvată: {0}", ["ClearRunError"] = "Pornirea salvată nu a putut fi ștearsă: {0}",
            ["OpenFileTitle"] = "Selectați configurația cuptorului magnetic", ["OpenCloudFolderTitle"] = "Selectați AnnealingResults sau alt dosar de profiluri din OneDrive de serviciu",
            ["CloudReadError"] = "Dosarul cloud nu a putut fi citit: {0}", ["CloudSaveError"] = "Dosarul nu a putut fi salvat: {0}"
        },
        ["en"] = new()
        {
            ["AppTitle"] = "Magnetic furnace control", ["HeaderOverline"] = "MAGNETIC FURNACE CONTROL",
            ["HeaderTitle"] = "Profile and finish time", ["CloudButton"] = "From cloud…", ["LocalFileButton"] = "File on PC…",
            ["StartTimeCaption"] = "START TIME — THIS PC CLOCK", ["DateCaption"] = "DATE  DD.MM.YYYY", ["TimeCaption"] = "TIME  HH:MM",
            ["StartButton"] = "Start", ["NowButton"] = "Now", ["ResetButton"] = "Reset", ["TotalDurationCaption"] = "TOTAL DURATION",
            ["RemoveCaption"] = "REMOVE FROM FURNACE", ["LiveTab"] = "Live process", ["StaticTab"] = "Static schedule",
            ["StageRemainingCaption"] = "STAGE REMAINING", ["ProfileRemainingCaption"] = "UNTIL PROFILE FINISH",
            ["TemperatureProfileTitle"] = "Estimated temperature profile", ["TemperatureDisclaimer"] = "Calculated from Excel setpoints and stage durations — not an actual measurement",
            ["ExpectedTemperatureCaption"] = "ESTIMATED TEMPERATURE", ["SetpointCaption"] = "STAGE SETPOINT", ["RateCaption"] = "RATE",
            ["ElapsedCaption"] = "ELAPSED / TOTAL", ["TransitionsTitle"] = "Stage transitions", ["StartCaption"] = "START", ["EndCaption"] = "END",
            ["StageCaption"] = "STAGE", ["TemperatureCaption"] = "TEMPERATURE", ["DurationCaption"] = "DURATION", ["FinishCaption"] = "FINISH", ["StatusCaption"] = "STATUS",
            ["CloudWindowTitle"] = "Profiles from SharePoint", ["CloudOverline"] = "PROFILES FROM SHAREPOINT / ONEDRIVE", ["CloudTitle"] = "Furnace cloud library",
            ["CloudSubtitle"] = "The file is downloaded from the cloud only after selection", ["OtherFolderButton"] = "Other folder…", ["RefreshButton"] = "Refresh",
            ["NameSearchCaption"] = "SEARCH BY NAME OR FOLDER", ["NameSearchWatermark"] = "For example: D50609 or annealing", ["ModifiedDateCaption"] = "MODIFIED DATE",
            ["ClearDateButton"] = "Clear date", ["NameCaption"] = "NAME", ["ModifiedCaption"] = "MODIFIED", ["FolderCaption"] = "FOLDER",
            ["CancelButton"] = "Cancel", ["SelectProfileButton"] = "Select profile",
            ["FurnaceName"] = "Furnace #{0}", ["ConfigurationCaption"] = "FURNACE #{0} · EXCEL CONFIGURATION", ["Waiting"] = "WAITING",
            ["ProfileNotSelected"] = "Profile not selected", ["ConfigurationNotLoaded"] = "Configuration not loaded", ["ChooseExcel"] = "Select the standard furnace Excel file",
            ["InputFormat"] = "Format: DD.MM.YYYY and HH:MM", ["LocalComputerTime"] = "Local time of this computer", ["EnterDateTime"] = "Enter date DD.MM.YYYY and time HH:MM",
            ["NoActiveRun"] = "No active run", ["RunNotConfirmed"] = "Run has not been confirmed", ["LoadExcelHint"] = "Load Excel and enter the start time",
            ["ConfirmRunHint"] = "Check the date and time, then press “Start”", ["Scheduled"] = "SCHEDULED", ["WaitingForStart"] = "Waiting for start",
            ["StartAt"] = "Start: {0}", ["StartShort"] = "Start {0}", ["FinishedRemove"] = "FINISHED — REMOVE FROM FURNACE", ["RemoveNow"] = "REMOVE FROM FURNACE",
            ["ProfileFinished"] = "Profile finished", ["CalculatedFinish"] = "Calculated finish: {0}", ["InProgress"] = "IN PROGRESS", ["StageCurrent"] = "Stage {0} · {1}",
            ["EndsAt"] = "Ends at {0}", ["RemainingShort"] = "Remaining {0}", ["HoldRate"] = "Hold", ["Pending"] = "Pending", ["Done"] = "Done", ["NowStatus"] = "Now",
            ["MinutesShort"] = "{0} min", ["RateUnit"] = "{0} °C/min", ["CloudFolderNotSelected"] = "Folder not selected", ["ChooseCloudFolder"] = "Select the SharePoint folder added to OneDrive",
            ["FolderUnavailable"] = "Folder is not selected or available", ["RefreshingProfiles"] = "Refreshing cloud profile list…", ["DateHint"] = "Modified date: DD.MM.YYYY",
            ["InvalidDateHint"] = "Invalid date — use DD.MM.YYYY", ["FixDate"] = "Correct the date to filter", ["ProfilesFound"] = "Profiles found: {0} of {1}", ["RootFolder"] = "Root folder",
            ["ReadExcelError"] = "Could not read Excel: {0}", ["SaveRunError"] = "Run was not saved: {0}", ["ClearRunError"] = "Could not delete the saved run: {0}",
            ["OpenFileTitle"] = "Select the magnetic furnace configuration", ["OpenCloudFolderTitle"] = "Select AnnealingResults or another profile folder from work OneDrive",
            ["CloudReadError"] = "Could not read the cloud folder: {0}", ["CloudSaveError"] = "Could not save the folder: {0}"
        }
    };

    public static IReadOnlyList<LanguageOption> Options { get; } =
    [
        new("ru", "RU · Русский"), new("ro", "RO · Română"), new("en", "EN · English")
    ];

    public static string CurrentCode { get; private set; } = "ru";

    public static void Initialize()
    {
        var saved = "ru";
        try
        {
            if (File.Exists(LanguagePath)) saved = File.ReadAllText(LanguagePath).Trim().ToLowerInvariant();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) { }
        Apply(saved, false);
    }

    public static void Apply(string code, bool save = true)
    {
        if (!Languages.ContainsKey(code)) code = "ru";
        CurrentCode = code;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(code);

        if (Application.Current is { } application)
            foreach (var pair in Languages[code]) application.Resources[pair.Key] = pair.Value;

        if (!save) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LanguagePath)!);
            File.WriteAllText(LanguagePath, code);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) { }
    }

    public static string Get(string key) => Languages[CurrentCode].TryGetValue(key, out var value) ? value : Languages["en"].GetValueOrDefault(key, key);

    public static string Format(string key, params object[] arguments) => string.Format(Get(key), arguments);
}
