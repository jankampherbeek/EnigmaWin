using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace EnigmaWin.Sources.AppShell.Navigation;

public sealed partial class NavigationService : ObservableObject, INavigationService
{
    private readonly Stack<NavigationEntry> _mainHistory = new();
    private readonly Stack<NavigationEntry> _detailHistory = new();

    [ObservableProperty]
    private string _currentMainRoute = AppRoutes.None;

    [ObservableProperty]
    private string _currentDetailRoute = AppRoutes.None;

    [ObservableProperty]
    private INavigationParameter? _currentMainParameter;

    [ObservableProperty]
    private INavigationParameter? _currentDetailParameter;

    public bool CanGoBackMain => _mainHistory.Count > 0;
    public bool CanGoBackDetail => _detailHistory.Count > 0;

    public void NavigateMain(string route, INavigationParameter? parameter = null)
    {
        if (route == CurrentMainRoute && Equals(parameter, CurrentMainParameter))
        {
            return;
        }

        if (CurrentMainRoute != AppRoutes.None || CurrentMainParameter != null || _mainHistory.Count > 0)
        {
            _mainHistory.Push(new NavigationEntry(CurrentMainRoute, CurrentMainParameter));
        }

        CurrentMainRoute = route;
        CurrentMainParameter = parameter;
        OnPropertyChanged(nameof(CanGoBackMain));
    }

    public void NavigateDetail(string route, INavigationParameter? parameter = null)
    {
        if (route == CurrentDetailRoute && Equals(parameter, CurrentDetailParameter))
        {
            return;
        }

        if (CurrentDetailRoute != AppRoutes.None || CurrentDetailParameter != null || _detailHistory.Count > 0)
        {
            _detailHistory.Push(new NavigationEntry(CurrentDetailRoute, CurrentDetailParameter));
        }

        CurrentDetailRoute = route;
        CurrentDetailParameter = parameter;
        OnPropertyChanged(nameof(CanGoBackDetail));
    }

    public void GoBackMain()
    {
        if (_mainHistory.Count == 0)
        {
            return;
        }

        var entry = _mainHistory.Pop();
        CurrentMainRoute = entry.Route;
        CurrentMainParameter = entry.Parameter;
        OnPropertyChanged(nameof(CanGoBackMain));
    }

    public void GoBackDetail()
    {
        if (_detailHistory.Count == 0)
        {
            return;
        }

        var entry = _detailHistory.Pop();
        CurrentDetailRoute = entry.Route;
        CurrentDetailParameter = entry.Parameter;
        OnPropertyChanged(nameof(CanGoBackDetail));
    }

    private readonly record struct NavigationEntry(string Route, INavigationParameter? Parameter);
}
