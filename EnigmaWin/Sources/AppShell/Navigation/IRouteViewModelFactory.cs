namespace EnigmaWin.Sources.AppShell.Navigation;

public interface IRouteViewModelFactory
{
    object? CreateMain(string route, INavigationParameter? parameter);
    object? CreateDetail(string route, INavigationParameter? parameter);
}
