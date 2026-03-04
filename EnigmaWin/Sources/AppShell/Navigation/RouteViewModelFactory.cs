using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.Config.UI;
using EnigmaWin.ViewModels.Routes;
using System;
using System.Collections.Generic;

namespace EnigmaWin.Sources.AppShell.Navigation;

public sealed class RouteViewModelFactory : IRouteViewModelFactory
{
    private readonly INavigationService _navigationService;
    private readonly Dictionary<string, Func<INavigationParameter?, object?>> _mainMap;
    private readonly Dictionary<string, Func<INavigationParameter?, object?>> _detailMap;

    public RouteViewModelFactory(
        INavigationService navigationService,
        IChartContext chartContext,
        IConfigContext configContext)
    {
        _navigationService = navigationService;

        _mainMap = new Dictionary<string, Func<INavigationParameter?, object?>>
        {
            [AppRoutes.MainRadixHome] = _ => new RadixWorkspaceRouteViewModel(),
            [AppRoutes.MainConfigHome] = _ => new ConfigWorkspaceRouteViewModel(),
            [AppRoutes.MainResearchHome] = _ => new ResearchWorkspaceRouteViewModel()
        };

        _detailMap = new Dictionary<string, Func<INavigationParameter?, object?>>
        {
            [AppRoutes.RadixInput] = parameter =>
            {
                var sessionId = parameter is RadixInputNavigationParameter p ? p.SessionId : Guid.Empty;
                return new RadixInputRouteViewModel(sessionId);
            },
            [AppRoutes.RadixPositions] = _ => new RadixPositionsRouteViewModel(chartContext),
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
                return new ConfigEditorViewModel(configContext, _navigationService, mode);
            }
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
