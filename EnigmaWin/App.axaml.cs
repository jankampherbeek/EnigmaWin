using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System;
using System.IO;
using System.Linq;
using Avalonia.Markup.Xaml;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Localization;
using EnigmaWin.ViewModels;
using EnigmaWin.Views;

namespace EnigmaWin;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Rosetta.SetLanguage("en");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var ephePath = ResolveEphemerisPath();
        try
        {
            SEWrapper.SeInitializer(ephePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Swiss Ephemeris initialization failed for path '{ephePath}'.", ex);
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.Exit += (_, _) => SEWrapper.CloseEphemeris();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static string ResolveEphemerisPath()
    {
        // Relative to runtime base dir (bin/...): resolve to <EnigmaWin>/se
        var hardcodedPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "se"));

        if (!Directory.Exists(hardcodedPath))
        {
            throw new InvalidOperationException(
                $"Swiss Ephemeris folder not found at hardcoded path: '{hardcodedPath}'.");
        }

        var hasKnownFile =
            File.Exists(Path.Combine(hardcodedPath, "de431.eph")) ||
            File.Exists(Path.Combine(hardcodedPath, "seasnam.txt")) ||
            Directory.EnumerateFiles(hardcodedPath, "*.se1").Any();

        if (!hasKnownFile)
        {
            throw new InvalidOperationException(
                $"Swiss Ephemeris files not found in hardcoded folder: '{hardcodedPath}'.");
        }

        return hardcodedPath;
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
