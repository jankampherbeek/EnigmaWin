// IRouteViewModelFactory.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.AppShell.Navigation;

public interface IRouteViewModelFactory
{
    object? CreateMain(string route, INavigationParameter? parameter);
    object? CreateDetail(string route, INavigationParameter? parameter);
}
