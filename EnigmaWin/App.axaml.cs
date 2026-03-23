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
using EnigmaWin.Sources.Data.Db;
using EnigmaWin.Sources.Data.Event;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Data.UserConfiguration;
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
            Services.GetRequiredService<DatabaseInitializer>().Initialize();

            var configContext  = Services.GetRequiredService<IConfigContext>();
            var configRepo     = Services.GetRequiredService<IUserConfigurationRepository>();

            // Load or create the active configuration from the database.
            var activeConfig = configRepo.FetchActiveAsync().GetAwaiter().GetResult();
            if (activeConfig is null)
            {
                activeConfig = configRepo.AddAsync("Default").GetAwaiter().GetResult();
            }
            configContext.ActiveConfig = activeConfig;

            var language = string.IsNullOrEmpty(activeConfig.Language)
                ? DetectSystemLanguage()
                : activeConfig.Language;
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
        services.AddSingleton<IChartSession, ChartSession>();
        services.AddSingleton<IConfigContext, ConfigContext>();
        services.AddSingleton<MainWindowViewModel>();

        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddSingleton<DatabaseInitializer>();
        services.AddSingleton<IHoroscopeRepository, HoroscopeRepository>();
        services.AddSingleton<IEventRepository, EventRepository>();
        services.AddSingleton<IUserConfigurationRepository, UserConfigurationRepository>();

        return services.BuildServiceProvider();
    }
}
