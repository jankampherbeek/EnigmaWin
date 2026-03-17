using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Markup.Xaml;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;
using EnigmaWin.ViewModels;
using EnigmaWin.Views;

namespace EnigmaWin;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
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
            Services = ConfigureServices();
            var configContext = Services.GetRequiredService<IConfigContext>();
            var savedLanguage = configContext.ActiveConfig.Language;
            var language = string.IsNullOrEmpty(savedLanguage) ? DetectSystemLanguage() : savedLanguage;
            Services.GetRequiredService<IRosetta>().SetLanguage(language);

            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.Exit += (_, _) => SEWrapper.CloseEphemeris();
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static readonly HashSet<string> SupportedLanguages = ["en", "nl", "de", "fr"];

    private static string DetectSystemLanguage()
    {
        var twoLetter = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return SupportedLanguages.Contains(twoLetter) ? twoLetter : "en";
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

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IRosetta, Rosetta>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IRouteViewModelFactory, RouteViewModelFactory>();
        services.AddSingleton<IChartContext, ChartContext>();
        services.AddSingleton<IConfigContext, ConfigContext>();
        services.AddSingleton<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }
}
