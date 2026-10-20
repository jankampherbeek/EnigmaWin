// SettingsViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Serilog;

namespace EnigmaWin.Sources.Features.Settings.UI;

/// <summary>ViewModel for the Settings window: machine-specific file locations.</summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly IRosetta      _rosetta;
    private readonly FileLocations _fileLocations;

    public SettingsViewModel(IRosetta rosetta, FileLocations fileLocations)
    {
        _rosetta       = rosetta;
        _fileLocations = fileLocations;
        _researchFolderPath = fileLocations.DefaultResearchFolder ?? string.Empty;
        _hasChosenLogFolder = fileLocations.ChosenLogFolder is not null;
        _logFilePath        = fileLocations.LogFilePath;
    }

    // ── State ────────────────────────────────────────────────────────────────

    [ObservableProperty, NotifyPropertyChangedFor(nameof(ResearchFolderText), nameof(HasResearchFolder))]
    private string _researchFolderPath;

    [ObservableProperty] private string _logFilePath;
    [ObservableProperty] private bool   _hasChosenLogFolder;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorText = string.Empty;

    public bool   HasResearchFolder  => !string.IsNullOrEmpty(ResearchFolderPath);
    public string ResearchFolderText => HasResearchFolder ? ResearchFolderPath : LabelNotSet;
    public bool   HasError           => !string.IsNullOrEmpty(ErrorText);

    /// <summary>Folder to start a folder dialog in.</summary>
    public string? ResearchFolderStart => _fileLocations.DefaultResearchFolder;
    public string  LogFolderStart      => _fileLocations.LogFolder;

    // ── Labels ───────────────────────────────────────────────────────────────

    private string T(string key) => _rosetta.GetText(RbFile.Settings, key);

    public string Title                 => T(SettingsKeys.Title);
    public string LabelSectionFiles     => T(SettingsKeys.SectionFiles);
    public string LabelResearchFolder   => T(SettingsKeys.ResearchFolderLabel);
    public string LabelResearchFolderHelp => T(SettingsKeys.ResearchFolderHelp);
    public string LabelLogFile          => T(SettingsKeys.LogFileLabel);
    public string LabelLogFileHelp      => T(SettingsKeys.LogFileHelp);
    public string LabelLogFolderHelp    => T(SettingsKeys.LogFolderHelp);
    public string LabelNotSet           => T(SettingsKeys.NotSet);
    public string LabelChoose           => T(SettingsKeys.ButtonChoose);
    public string LabelClear            => T(SettingsKeys.ButtonClear);
    public string LabelShowInExplorer   => T(SettingsKeys.ButtonShowInExplorer);
    public string LabelClose            => T(SettingsKeys.ButtonClose);

    // ── Research folder ──────────────────────────────────────────────────────

    /// <summary>Stores the folder just selected in a folder dialog as the default research folder.</summary>
    public void SetResearchFolder(string folder)
    {
        try
        {
            _fileLocations.SetDefaultResearchFolder(folder);
            ResearchFolderPath = folder;
            ErrorText = string.Empty;
        }
        catch (IOException ex)
        {
            Log.Error(ex, "SettingsViewModel: could not store the default research folder");
            ErrorText = T(SettingsKeys.ErrorResearchFolder);
        }
    }

    [RelayCommand]
    private void ClearResearchFolder()
    {
        try { _fileLocations.SetDefaultResearchFolder(null); }
        catch (IOException ex) { Log.Error(ex, "SettingsViewModel: could not clear the default research folder"); }
        ResearchFolderPath = string.Empty;
        ErrorText = string.Empty;
    }

    // ── Log folder ───────────────────────────────────────────────────────────

    /// <summary>Uses the folder just selected in a folder dialog for the log file, from now on.</summary>
    public void SetLogFolder(string folder)
    {
        if (AppLogging.Configure(folder) != folder)
        {
            ErrorText = T(SettingsKeys.ErrorLogFolder);
            return;
        }
        try
        {
            _fileLocations.SetLogFolder(folder);
            ErrorText = string.Empty;
        }
        catch (IOException ex)
        {
            Log.Error(ex, "SettingsViewModel: could not store the log folder");
            ErrorText = T(SettingsKeys.ErrorLogFolder);
        }
        RefreshLogFile();
    }

    [RelayCommand]
    private void ClearLogFolder()
    {
        try { _fileLocations.SetLogFolder(null); }
        catch (IOException ex) { Log.Error(ex, "SettingsViewModel: could not clear the log folder"); }
        AppLogging.Configure(_fileLocations.LogFolder);
        ErrorText = string.Empty;
        RefreshLogFile();
    }

    /// <summary>Shows the log file in Explorer, or its folder if nothing has been logged yet.</summary>
    [RelayCommand]
    private void ShowLogFile()
    {
        var folder = _fileLocations.LogFolder;
        var file   = AppLogging.LatestLogFile(folder);
        if (file is not null)
            Process.Start("explorer.exe", $"/select,\"{file}\"");
        else if (Directory.Exists(folder))
            Process.Start("explorer.exe", $"\"{folder}\"");
    }

    private void RefreshLogFile()
    {
        LogFilePath        = _fileLocations.LogFilePath;
        HasChosenLogFolder = _fileLocations.ChosenLogFolder is not null;
    }
}
