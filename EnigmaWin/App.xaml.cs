// App.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Data.Db;
using EnigmaWin.Sources.Data.Event;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Data.UserConfiguration;
using EnigmaWin.Sources.Features.Periods.CyclesAstronomical.UI;
using EnigmaWin.Sources.Features.Periods.CyclesWaves.UI;
using EnigmaWin.Sources.Features.Periods.MonthlyEphemeris;
using EnigmaWin.Sources.Features.Periods.LongTimeEphemeris;
using EnigmaWin.Sources.Features.Periods.Eclipses;
using EnigmaWin.Sources.Features.Synastry;
using EnigmaWin.Sources.Features.Progressive;
using EnigmaWin.Sources.Features.Progressive.Events;
using EnigmaWin.Sources.Features.Progressive.AgePoint;
using EnigmaWin.Sources.Features.Progressive.AgePoint.UI;
using EnigmaWin.Sources.Features.Progressive.Solar.UI;
using EnigmaWin.Sources.Features.Progressive.PrimDir.UI;
using EnigmaWin.Sources.Features.Progressive.PreNatal.UI;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.ZodiacDivisions.UI;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Enneagram.UI;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.VSP.UI;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.FixStars.UI;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Parans.UI;
using EnigmaWin.Sources.Features.Progressive.LogTimeScale;
using EnigmaWin.Sources.Features.Progressive.LogTimeScale.UI;
using EnigmaWin.Sources.Features.Progressive.SymbolicDir.UI;
using EnigmaWin.Sources.Features.Progressive.TransitSecDir.UI;
using EnigmaWin.Sources.Features.Research.ResearchProjects.Persistency;
using EnigmaWin.Sources.Features.Shared;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using EnigmaWin.ViewModels;
using EnigmaWin.Views;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var ephePath = ResolveEphemerisPath();
        try
        {
            SEWrapper.SeInitializer(ephePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Swiss Ephemeris initialization failed for path '{ephePath}'.", ex);
        }

        Services = ConfigureServices();
        Services.GetRequiredService<DatabaseInitializer>().Initialize();

        var configContext = Services.GetRequiredService<IConfigContext>();
        var configRepo    = Services.GetRequiredService<IUserConfigurationRepository>();

        var activeConfig = configRepo.FetchActiveAsync().GetAwaiter().GetResult();
        if (activeConfig is null)
            activeConfig = configRepo.AddAsync("Default").GetAwaiter().GetResult();

        configContext.ActiveConfig = activeConfig;

        var language = string.IsNullOrEmpty(activeConfig.Language)
            ? DetectSystemLanguage()
            : activeConfig.Language;
        var rosetta = Services.GetRequiredService<IRosetta>();
        rosetta.SetLanguage(language);

        var chartSession = Services.GetRequiredService<IChartSession>();
        StartupChartBuilder.Build(chartSession, configContext, rosetta);

        Exit += (_, _) => SEWrapper.CloseEphemeris();

        var mainWindow = new MainWindow
        {
            DataContext = Services.GetRequiredService<MainWindowViewModel>()
        };
        mainWindow.Show();
    }

    private static readonly HashSet<string> SupportedLanguages = ["en", "nl", "de", "fr"];

    private static string DetectSystemLanguage()
    {
        var twoLetter = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return SupportedLanguages.Contains(twoLetter) ? twoLetter : "en";
    }

    private static string ResolveEphemerisPath()
    {
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
        services.AddSingleton<IResearchProjectRepository, ResearchProjectRepository>();
        services.AddSingleton<EventsOrchestrator>();
        services.AddSingleton<IProgressiveSession, ProgressiveSession>();
        services.AddSingleton<ProgressiveOrchestrator>();
        services.AddSingleton<TransitViewModel>();
        services.AddSingleton<SecondaryViewModel>();
        services.AddSingleton<SymbolicViewModel>();
        services.AddSingleton<LogTimeScaleOrchestrator>();
        services.AddSingleton<LogTimeScaleViewModel>();
        services.AddSingleton<AgePointOrchestrator>();
        services.AddSingleton<AgePointViewModel>();
        services.AddSingleton<SolarViewModel>();
        services.AddSingleton<PrimDirViewModel>();
        services.AddSingleton<PreNatalViewModel>();
        services.AddSingleton<ZodiacDivisionsViewModel>();
        services.AddSingleton<EnneagramViewModel>();
        services.AddSingleton<VspViewModel>();
        services.AddSingleton<FixStarsViewModel>();
        services.AddSingleton<EnigmaWin.Sources.Features.Radix.RadixAnalysis.HarmonicOrbs.UI.HarmonicOrbsViewModel>();
        services.AddSingleton<ParansViewModel>();
        services.AddSingleton<AstronomicalCyclesModel>();
        services.AddSingleton<WavesModel>();
        services.AddSingleton<MonthlyEphemerisModel>();
        services.AddSingleton<LongTimeEphemerisModel>();
        services.AddSingleton<EclipsesModel>();
        services.AddSingleton<SynastryModel>();

        return services.BuildServiceProvider();
    }
}
