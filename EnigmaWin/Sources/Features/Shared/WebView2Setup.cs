// WebView2Setup.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace EnigmaWin.Sources.Features.Shared;

public static class WebView2Setup
{
    private static Task<CoreWebView2Environment>? _environmentTask;

    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "EnigmaWin", "diagnostics.log");

    private static void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            File.AppendAllText(LogPath, $"{DateTime.Now:O} {message}{Environment.NewLine}");
        }
        catch
        {
            // logging must never throw
        }
    }

    public static async Task EnsureInitializedAsync(WebView2 webView)
    {
        Log("EnsureInitializedAsync: entered");
        try
        {
            var environment = await GetEnvironmentAsync();
            Log("EnsureInitializedAsync: environment ready");
            var initTask = webView.EnsureCoreWebView2Async(environment);
            var winner = await Task.WhenAny(initTask, Task.Delay(TimeSpan.FromSeconds(8)));

            if (winner != initTask)
            {
                Log("EnsureInitializedAsync: TIMEOUT waiting for EnsureCoreWebView2Async");
                MessageBox.Show(
                    "WebView2-initialisatie duurde langer dan 8 seconden en is blijven hangen " +
                    "(geen exception, geen resultaat). Dit wijst op een geblokkeerd of niet-gestart " +
                    "msedgewebview2.exe-proces, bijvoorbeeld door antivirus- of beveiligingsbeleid.",
                    "WebView2 timeout",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await initTask;
            Log($"EnsureInitializedAsync: completed, CoreWebView2 is {(webView.CoreWebView2 != null ? "set" : "NULL")}");
        }
        catch (Exception ex)
        {
            Log($"EnsureInitializedAsync: EXCEPTION {ex}");
            MessageBox.Show(
                $"WebView2 kon niet worden geïnitialiseerd:\n\n{ex}",
                "WebView2 initialisatiefout",
                MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private static Task<CoreWebView2Environment> GetEnvironmentAsync() =>
        _environmentTask ??= CreateEnvironmentAsync();

    private static Task<CoreWebView2Environment> CreateEnvironmentAsync()
    {
        var userDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EnigmaWin", "WebView2");
        Directory.CreateDirectory(userDataFolder);
        Log($"CreateEnvironmentAsync: userDataFolder = {userDataFolder}");
        return CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
    }
}
