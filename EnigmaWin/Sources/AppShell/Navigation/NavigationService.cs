// NavigationService.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

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

    // Sentinel property names that fire once after route+parameter are both committed.
    public const string MainNavigatedProperty   = "MainNavigated";
    public const string DetailNavigatedProperty = "DetailNavigated";

    public void NavigateMain(string route, INavigationParameter? parameter = null)
    {
        if (route == CurrentMainRoute && Equals(parameter, CurrentMainParameter))
            return;

        if (CurrentMainRoute != AppRoutes.None || CurrentMainParameter != null || _mainHistory.Count > 0)
            _mainHistory.Push(new NavigationEntry(CurrentMainRoute, CurrentMainParameter));

        // Set backing fields directly to avoid two separate PropertyChanged events.
        _currentMainRoute     = route;
        _currentMainParameter = parameter;
        OnPropertyChanged(MainNavigatedProperty);
        OnPropertyChanged(nameof(CanGoBackMain));
    }

    public void NavigateDetail(string route, INavigationParameter? parameter = null)
    {
        if (route == CurrentDetailRoute && Equals(parameter, CurrentDetailParameter))
            return;

        if (CurrentDetailRoute != AppRoutes.None || CurrentDetailParameter != null || _detailHistory.Count > 0)
            _detailHistory.Push(new NavigationEntry(CurrentDetailRoute, CurrentDetailParameter));

        _currentDetailRoute     = route;
        _currentDetailParameter = parameter;
        OnPropertyChanged(DetailNavigatedProperty);
        OnPropertyChanged(nameof(CanGoBackDetail));
    }

    public void GoBackMain()
    {
        if (_mainHistory.Count == 0) return;

        var entry = _mainHistory.Pop();
        _currentMainRoute     = entry.Route;
        _currentMainParameter = entry.Parameter;
        OnPropertyChanged(MainNavigatedProperty);
        OnPropertyChanged(nameof(CanGoBackMain));
    }

    public void GoBackDetail()
    {
        if (_detailHistory.Count == 0) return;

        var entry = _detailHistory.Pop();
        _currentDetailRoute     = entry.Route;
        _currentDetailParameter = entry.Parameter;
        OnPropertyChanged(DetailNavigatedProperty);
        OnPropertyChanged(nameof(CanGoBackDetail));
    }

    private readonly record struct NavigationEntry(string Route, INavigationParameter? Parameter);
}
