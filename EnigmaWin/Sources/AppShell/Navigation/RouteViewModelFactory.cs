using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Data.UserConfiguration;
using EnigmaWin.Sources.Features.Config.UI;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using EnigmaWin.ViewModels.Routes;
using System;
using System.Collections.Generic;

namespace EnigmaWin.Sources.AppShell.Navigation;

public sealed class RouteViewModelFactory : IRouteViewModelFactory
{
    private readonly INavigationService _navigationService;
    private readonly Dictionary<string, Func<INavigationParameter?, object?>> _mainMap;
    private readonly Dictionary<string, Func<INavigationParameter?, object?>> _detailMap;

    private readonly IRosetta _rosetta;

    public RouteViewModelFactory(
        INavigationService navigationService,
        IChartSession chartSession,
        IConfigContext configContext,
        IUserConfigurationRepository configRepository,
        IRosetta rosetta)
    {
        _navigationService = navigationService;
        _rosetta = rosetta;

        _mainMap = new Dictionary<string, Func<INavigationParameter?, object?>>
        {
            [AppRoutes.MainRadixHome]     = _ => new RadixWorkspaceRouteViewModel(),
            [AppRoutes.MainConfigHome]    = _ => new ConfigListViewModel(configRepository, navigationService, configContext, rosetta),
            [AppRoutes.MainResearchHome]  = _ => new ResearchWorkspaceRouteViewModel()
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
            [AppRoutes.ConfigSectionProgSolar]      = _ => new ConfigSolarReturnSectionViewModel(configRepository, navigationService, configContext, rosetta)
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
