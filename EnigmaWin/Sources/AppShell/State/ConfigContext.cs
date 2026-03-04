using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.AppShell.State;

public sealed partial class ConfigContext : ObservableObject, IConfigContext
{
    [ObservableProperty]
    private ConfigData _activeConfig = ConfigData.Default;
}
