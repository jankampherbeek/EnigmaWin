using System.ComponentModel;

namespace EnigmaWin.Sources.AppShell.Navigation;

public interface INavigationService : INotifyPropertyChanged
{
    string CurrentMainRoute { get; }
    string CurrentDetailRoute { get; }
    INavigationParameter? CurrentMainParameter { get; }
    INavigationParameter? CurrentDetailParameter { get; }

    bool CanGoBackMain { get; }
    bool CanGoBackDetail { get; }

    void NavigateMain(string route, INavigationParameter? parameter = null);
    void NavigateDetail(string route, INavigationParameter? parameter = null);
    void GoBackMain();
    void GoBackDetail();
}
