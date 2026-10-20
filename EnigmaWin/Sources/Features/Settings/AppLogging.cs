// AppLogging.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.IO;
using System.Linq;
using Serilog;

namespace EnigmaWin.Sources.Features.Settings;

/// <summary>
/// Writes warnings and errors to the log file in the folder from <see cref="FileLocations"/>.
/// The file is replaced at 5 MB; the two previous versions are kept.
/// </summary>
public static class AppLogging
{
    private const long MaxFileSize    = 5 * 1024 * 1024;
    private const int  RetainedFiles  = 3;

    /// <summary>Starts (or restarts) logging to <paramref name="folder"/>. If that folder cannot be used,
    /// the default log folder is used instead. Returns the folder that is actually used.</summary>
    public static string Configure(string folder)
    {
        var used = TryConfigure(folder) ? folder
                 : TryConfigure(FileLocations.DefaultLogFolder) ? FileLocations.DefaultLogFolder
                 : string.Empty;
        if (used != folder)
            Log.Warning("AppLogging: could not use log folder {Folder}; using {Used}", folder, used);
        return used;
    }

    /// <summary>The most recent log file in <paramref name="folder"/>, or null if nothing has been logged yet.
    /// After a size limit is reached the newest file has a sequence number (EnigmaWin_001.log).</summary>
    public static string? LatestLogFile(string folder)
    {
        if (!Directory.Exists(folder)) return null;
        return new DirectoryInfo(folder)
            .GetFiles(Path.GetFileNameWithoutExtension(FileLocations.LogFileName) + "*.log")
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .FirstOrDefault()?.FullName;
    }

    private static bool TryConfigure(string folder)
    {
        try
        {
            Directory.CreateDirectory(folder);
            var logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.File(Path.Combine(folder, FileLocations.LogFileName),
                              fileSizeLimitBytes: MaxFileSize,
                              rollOnFileSizeLimit: true,
                              retainedFileCountLimit: RetainedFiles,
                              shared: true)
                .CreateLogger();
            var previous = Log.Logger;
            Log.Logger = logger;
            (previous as IDisposable)?.Dispose();
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return false;
        }
    }
}
