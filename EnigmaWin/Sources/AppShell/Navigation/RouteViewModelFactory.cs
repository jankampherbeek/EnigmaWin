// RouteViewModelFactory.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Data.UserConfiguration;
using EnigmaWin.Sources.Features.Config.UI;
using EnigmaWin.Sources.Features.Cycles.CyclesAstronomical.UI;
using EnigmaWin.Sources.Features.Cycles.CyclesWaves.UI;
using EnigmaWin.Sources.Features.Cycles.Ephemeris;
using EnigmaWin.Sources.Features.Cycles.Ephemeris.UI;
using EnigmaWin.Sources.Features.Progressive;
using EnigmaWin.Sources.Features.Progressive.Events;
using EnigmaWin.Sources.Features.Progressive.Events.UI;
using EnigmaWin.Sources.Features.Progressive.AgePoint.UI;
using EnigmaWin.Sources.Features.Progressive.Solar.UI;
using EnigmaWin.Sources.Features.Progressive.PrimDir.UI;
using EnigmaWin.Sources.Features.Progressive.PreNatal.UI;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.ZodiacDivisions.UI;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Enneagram.UI;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.VSP.UI;
using EnigmaWin.Sources.Features.Progressive.LogTimeScale.UI;
using EnigmaWin.Sources.Features.Progressive.SymbolicDir.UI;
using EnigmaWin.Sources.Features.Progressive.TransitSecDir.UI;
using EnigmaWin.Sources.Features.Research.ResearchProjects.Persistency;
using EnigmaWin.Sources.Features.Research.UI;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using EnigmaWin.Sources.Features.Calculators.UI;
using EnigmaWin.ViewModels.Routes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace EnigmaWin.Sources.AppShell.Navigation;

public sealed class RouteViewModelFactory : IRouteViewModelFactory
{
    private readonly INavigationService _navigationService;
    private readonly Dictionary<string, Func<INavigationParameter?, object?>> _mainMap;
    private readonly Dictionary<string, Func<INavigationParameter?, object?>> _detailMap;

    private readonly IRosetta _rosetta;
    private readonly EventsOrchestrator _eventsOrchestrator;
    private readonly IProgressiveSession _progressiveSession;
    private readonly IHoroscopeRepository _horoscopeRepository;
    private readonly ProgressiveOrchestrator _progressiveOrchestrator;
    private readonly IConfigContext _configContext;
    private readonly IServiceProvider _services;

    public RouteViewModelFactory(
        INavigationService navigationService,
        IChartSession chartSession,
        IConfigContext configContext,
        IUserConfigurationRepository configRepository,
        IResearchProjectRepository researchProjectRepository,
        IRosetta rosetta,
        AstronomicalCyclesModel astronomicalCyclesModel,
        WavesModel wavesModel,
        EphemerisModel ephemerisModel,
        EventsOrchestrator eventsOrchestrator,
        IProgressiveSession progressiveSession,
        IHoroscopeRepository horoscopeRepository,
        ProgressiveOrchestrator progressiveOrchestrator,
        IServiceProvider services)
    {
        _navigationService       = navigationService;
        _rosetta                 = rosetta;
        _eventsOrchestrator      = eventsOrchestrator;
        _progressiveSession      = progressiveSession;
        _horoscopeRepository     = horoscopeRepository;
        _progressiveOrchestrator = progressiveOrchestrator;
        _configContext           = configContext;
        _services                = services;

        _mainMap = new Dictionary<string, Func<INavigationParameter?, object?>>
        {
            [AppRoutes.MainRadixHome]     = _ => new RadixWorkspaceRouteViewModel(),
            [AppRoutes.RadixChart]        = _ => new RadixChartRouteViewModel(),
            [AppRoutes.MainConfigHome]    = _ => new ConfigListViewModel(configRepository, navigationService, configContext, rosetta),
            [AppRoutes.MainResearchHome]  = _ => new ResearchWorkspaceRouteViewModel(),
            [AppRoutes.ResearchProjects]      = _ => new ResearchProjectsRouteViewModel(),
            [AppRoutes.ResearchProjectInput]  = _ => new ResearchProjectInputRouteViewModel(),
            [AppRoutes.ResearchProjectConfig] = parameter =>
            {
                var draft = parameter is ResearchProjectConfigNavigationParameter p
                    ? p.Draft
                    : new ResearchProjectDraft("", "", default, 1, "");
                return new ResearchProjectConfigRouteViewModel(draft);
            },
            [AppRoutes.ResearchProjectListAll] = _ =>
                new ResearchProjectAllListRouteViewModel(),
            [AppRoutes.ResearchProjectListSearch] = _ =>
                new ResearchProjectSearchListRouteViewModel(),
            [AppRoutes.ResearchProjectOpen] = parameter =>
            {
                var id = parameter is ResearchProjectOpenNavigationParameter p ? p.ProjectId : 0;
                return new ResearchProjectOpenRouteViewModel(id, researchProjectRepository);
            },
            [AppRoutes.ResearchResult] = parameter =>
            {
                if (parameter is ResearchResultNavigationParameter p)
                    return new ResearchResultRouteViewModel(p.Result, p.Project, p.CgMultiplier);
                return null;
            },
            [AppRoutes.MainCyclesHome]         = _ => new CyclesWorkspaceRouteViewModel(rosetta),
            [AppRoutes.MainCalculatorsHome]   = _ => new CalculatorsWorkspaceRouteViewModel(rosetta),
            [AppRoutes.MainProgressiveHome]   = _ => new ProgressiveWorkspaceRouteViewModel(rosetta),
            [AppRoutes.ProgressiveTransit]   = _ => _services.GetRequiredService<TransitViewModel>(),
            [AppRoutes.ProgressiveSecondary] = _ => _services.GetRequiredService<SecondaryViewModel>(),
            [AppRoutes.ProgressiveSymbolic]      = _ => _services.GetRequiredService<SymbolicViewModel>(),
            [AppRoutes.ProgressiveLogTimeScale]  = _ => _services.GetRequiredService<LogTimeScaleViewModel>(),
            [AppRoutes.ProgressiveAgePoint]      = _ => _services.GetRequiredService<AgePointViewModel>(),
            [AppRoutes.ProgressiveSolar]         = _ => _services.GetRequiredService<SolarViewModel>(),
            [AppRoutes.ProgressivePrimDir]       = _ => _services.GetRequiredService<PrimDirViewModel>(),
            [AppRoutes.ProgressivePreNatal]      = _ => _services.GetRequiredService<PreNatalViewModel>().Inner,
            [AppRoutes.CyclesAstronomical]        = _ => new CyclesChartViewModel(rosetta, astronomicalCyclesModel),
            [AppRoutes.CyclesWaves]               = _ => new WavesChartViewModel(rosetta, wavesModel),
            [AppRoutes.EphemerisInput]            = _ => new EphemerisInputViewModel(rosetta, ephemerisModel, configContext),
            [AppRoutes.RadixZodiacDivisions]      = _ => _services.GetRequiredService<ZodiacDivisionsViewModel>(),
            [AppRoutes.RadixEnneagram]            = _ => _services.GetRequiredService<EnneagramViewModel>(),
            [AppRoutes.RadixVsp]                  = _ => _services.GetRequiredService<VspViewModel>(),
        };

        _detailMap = new Dictionary<string, Func<INavigationParameter?, object?>>
        {
            [AppRoutes.RadixInput] = parameter =>
            {
                var sessionId = parameter is RadixInputNavigationParameter p ? p.SessionId : Guid.Empty;
                return new RadixInputRouteViewModel(sessionId);
            },
            [AppRoutes.RadixPositions] = _ => new RadixPositionsRouteViewModel(chartSession),
            [AppRoutes.RadixOverview]  = _ => new RadixOverviewRouteViewModel(),
            [AppRoutes.RadixAnalysis]  = _ => new RadixAnalysisRouteViewModel(),
            [AppRoutes.RadixAspects]    = _ => new RadixAspectsRouteViewModel(),
            [AppRoutes.RadixMidpoints]  = _ => new RadixMidpointsRouteViewModel(),
            [AppRoutes.RadixHarmonics]         = _ => new RadixHarmonicsRouteViewModel(),
            [AppRoutes.RadixDeclinations]      = _ => new RadixDeclinationsRouteViewModel(),
            [AppRoutes.RadixZodiacDivisionsInput] = _ => new ZodiacDivisionsInputViewModel(_services.GetRequiredService<ZodiacDivisionsViewModel>()),
            [AppRoutes.RadixEnneagramOptions]     = _ => new EnneagramOptionsViewModel(_services.GetRequiredService<EnneagramViewModel>()),
            [AppRoutes.RadixVspDetail]            = _ => new VspDetailViewModel(_services.GetRequiredService<VspViewModel>()),
            [AppRoutes.RadixSearch]    = _ => new RadixSearchRouteViewModel(),
            [AppRoutes.RadixEdit]      = _ => new RadixEditRouteViewModel(),
            [AppRoutes.ConfigHome] = parameter =>
            {
                var mode = parameter is ConfigHomeNavigationParameter p
                    ? p.Mode
                    : ConfigHomeMode.Overview;
                return new ConfigHomeViewModel(configContext, mode);
            },
            [AppRoutes.ConfigEditor] = parameter =>
            {
                var mode = parameter is ConfigEditorNavigationParameter p
                    ? p.Mode
                    : ConfigEditorMode.Edit;
                return new ConfigEditorViewModel(configContext, _navigationService, _rosetta, mode);
            },
            [AppRoutes.ConfigEdit] = _ => new ConfigEditViewModel(configRepository, navigationService, configContext, rosetta),
            [AppRoutes.ConfigSectionCalc]         = _ => new ConfigCalcSectionViewModel(configRepository, navigationService, configContext, rosetta),
            [AppRoutes.ConfigSectionDisplay]      = _ => new ConfigDisplaySectionViewModel(configRepository, navigationService, configContext, rosetta),
            [AppRoutes.ConfigSectionGlyphs]       = _ => new ConfigGlyphsSectionViewModel(configRepository, navigationService, configContext, rosetta),
            [AppRoutes.ConfigSectionFactors]      = _ => new ConfigFactorSectionViewModel(configRepository, navigationService, configContext, rosetta),
            [AppRoutes.ConfigSectionAspects]      = _ => new ConfigAspectSectionViewModel(configRepository, navigationService, configContext, rosetta),
            [AppRoutes.ConfigSectionOrbs]         = _ => new ConfigOrbSectionViewModel(configRepository, navigationService, configContext, rosetta),
            [AppRoutes.ConfigSectionProgressions] = _ => new ConfigProgressionsSectionViewModel(navigationService, configContext, rosetta),
            [AppRoutes.ConfigSectionProgPrimary]    = _ => new ConfigPrimaryDirectionsSectionViewModel(configRepository, navigationService, configContext, rosetta),
            [AppRoutes.ConfigSectionProgTransits]   = _ => new ConfigTransitsSectionViewModel(configRepository, navigationService, configContext, rosetta),
            [AppRoutes.ConfigSectionProgSecondary]  = _ => new ConfigSecondaryDirectionsSectionViewModel(configRepository, navigationService, configContext, rosetta),
            [AppRoutes.ConfigSectionProgSymbolic]   = _ => new ConfigSymbolicDirectionsSectionViewModel(configRepository, navigationService, configContext, rosetta),
            [AppRoutes.ConfigSectionProgSolar]      = _ => new ConfigSolarReturnSectionViewModel(configRepository, navigationService, configContext, rosetta),
            [AppRoutes.CyclesAstronomicalInput]     = _ => new AstronomicalCyclesScreenViewModel(rosetta, astronomicalCyclesModel),
            [AppRoutes.CyclesWavesInput]            = _ => new WavesScreenViewModel(rosetta, wavesModel),
            [AppRoutes.Ephemeris]                   = _ => new EphemerisResultViewModel(rosetta, ephemerisModel),
            [AppRoutes.CalculatorsJulianDay]        = _ => new JulianDayRouteViewModel(rosetta),
            [AppRoutes.CalculatorsObliquity]        = _ => new ObliquityRouteViewModel(rosetta),
            [AppRoutes.ProgressiveTransitInput]   = _ => new TransitInputViewModel(_services.GetRequiredService<TransitViewModel>()),
            [AppRoutes.ProgressiveSecondaryInput] = _ => new SecondaryInputViewModel(_services.GetRequiredService<SecondaryViewModel>()),
            [AppRoutes.ProgressiveSymbolicInput]      = _ => new SymbolicInputViewModel(_services.GetRequiredService<SymbolicViewModel>()),
            [AppRoutes.ProgressiveLogTimeScaleInput] = _ => new LogTimeScaleInputViewModel(_services.GetRequiredService<LogTimeScaleViewModel>()),
            [AppRoutes.ProgressiveAgePointInput]     = _ => new AgePointInputViewModel(_services.GetRequiredService<AgePointViewModel>()),
            [AppRoutes.ProgressiveSolarInput]        = _ => new SolarInputViewModel(_services.GetRequiredService<SolarViewModel>()),
            [AppRoutes.ProgressivePrimDirInput]      = _ => new PrimDirInputViewModel(_services.GetRequiredService<PrimDirViewModel>()),
            [AppRoutes.ProgressivePreNatalInput]     = _ => _services.GetRequiredService<PreNatalViewModel>(),
            [AppRoutes.ProgressiveEventsOverview] = _ => new EventsOverviewViewModel(
                _eventsOrchestrator, _horoscopeRepository, chartSession, _progressiveSession, navigationService, rosetta),
            [AppRoutes.ProgressiveEventInput] = parameter =>
            {
                var id = parameter is ProgressiveEventNavigationParameter p ? p.HoroscopeId : Guid.Empty;
                return new EventInputViewModel(_eventsOrchestrator, navigationService, rosetta, id);
            },
            [AppRoutes.ProgressiveEventEdit] = parameter =>
            {
                if (parameter is ProgressiveEventEditNavigationParameter ep)
                    return new EventEditViewModel(_eventsOrchestrator, navigationService, rosetta, ep.Event);
                return null;
            },
        };
    }

    public object? CreateMain(string route, INavigationParameter? parameter)
    {
        return _mainMap.TryGetValue(route, out var factory)
            ? factory(parameter)
            : null;
    }

    public object? CreateDetail(string route, INavigationParameter? parameter)
    {
        return _detailMap.TryGetValue(route, out var factory)
            ? factory(parameter)
            : null;
    }
}
