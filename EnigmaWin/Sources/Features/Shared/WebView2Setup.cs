// WebView2Setup.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace EnigmaWin.Sources.Features.Shared;

public static class WebView2Setup
{
    private static Task<CoreWebView2Environment>? _environmentTask;

    /// <summary>Initializes the WebView2 control. Returns false (after showing an error
    /// dialog) if initialization fails, so callers can skip setting Source.</summary>
    public static async Task<bool> EnsureInitializedAsync(WebView2 webView, IRosetta? rosetta = null)
    {
        try
        {
            var environment = await GetEnvironmentAsync();
            await webView.EnsureCoreWebView2Async(environment);
            return true;
        }
        catch (Exception)
        {
            var message = rosetta?.GetText(RbFile.Localizable, "shared.webview2.initerror")
                          ?? "The PDF viewer could not be initialized.";
            var title = rosetta?.GetText(RbFile.Localizable, "error") ?? "Error";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
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
        return CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
    }
}
