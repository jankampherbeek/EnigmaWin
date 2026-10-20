// FileLocations.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.IO;
using System.Text.Json;

namespace EnigmaWin.Sources.Features.Settings;

/// <summary>
/// Machine-specific file locations: the default folder for research projects and the folder for the log file.
/// Stored in a settings file in the local application data folder rather than in the configuration,
/// because a folder only exists on this computer.
/// </summary>
public sealed class FileLocations
{
    public const string LogFileName = "EnigmaWin.log";

    private static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EnigmaWin");

    /// <summary>File locations of this computer.</summary>
    public static FileLocations Current { get; } = new(Path.Combine(AppDataFolder, "settings.json"));

    /// <summary>Folder for the log file when no folder has been chosen.</summary>
    public static string DefaultLogFolder => Path.Combine(AppDataFolder, "Logs");

    private readonly string _settingsPath;

    public FileLocations(string settingsPath) => _settingsPath = settingsPath;

    private sealed record StoredLocations(string? DefaultResearchFolder, string? LogFolder);

    /// <summary>Default base folder for new research projects, or null if none is set or the folder no longer exists.</summary>
    public string? DefaultResearchFolder => ExistingFolder(Load().DefaultResearchFolder);

    /// <summary>The chosen folder for the log file, or null if none is set or the folder no longer exists.</summary>
    public string? ChosenLogFolder => ExistingFolder(Load().LogFolder);

    /// <summary>Folder that is used for the log file: the chosen folder, otherwise <see cref="DefaultLogFolder"/>.</summary>
    public string LogFolder => ChosenLogFolder ?? DefaultLogFolder;

    public string LogFilePath => Path.Combine(LogFolder, LogFileName);

    /// <summary>Stores the default research folder; null removes the setting.</summary>
    /// <exception cref="IOException">The settings could not be written.</exception>
    public void SetDefaultResearchFolder(string? folder) =>
        Save(Load() with { DefaultResearchFolder = folder });

    /// <summary>Stores the folder for the log file; null returns to <see cref="DefaultLogFolder"/>.</summary>
    /// <exception cref="IOException">The settings could not be written.</exception>
    public void SetLogFolder(string? folder) =>
        Save(Load() with { LogFolder = folder });

    private StoredLocations Load()
    {
        try
        {
            if (!File.Exists(_settingsPath)) return new StoredLocations(null, null);
            return JsonSerializer.Deserialize<StoredLocations>(File.ReadAllText(_settingsPath))
                   ?? new StoredLocations(null, null);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Serilog.Log.Warning(ex, "FileLocations: could not read {Path}", _settingsPath);
            return new StoredLocations(null, null);
        }
    }

    private void Save(StoredLocations locations)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
            File.WriteAllText(_settingsPath,
                JsonSerializer.Serialize(locations, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new IOException($"Could not write {_settingsPath}", ex);
        }
    }

    private static string? ExistingFolder(string? folder) =>
        !string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder) ? folder : null;
}
