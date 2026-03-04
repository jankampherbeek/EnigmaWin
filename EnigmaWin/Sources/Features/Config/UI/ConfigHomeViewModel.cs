using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.Config;
using System.ComponentModel;

namespace EnigmaWin.Sources.Features.Config.UI;

public sealed class ConfigHomeViewModel : ObservableObject
{
    private readonly IConfigContext _configContext;
    public ConfigHomeMode Mode { get; }

    public ConfigHomeViewModel(IConfigContext configContext, ConfigHomeMode mode)
    {
        _configContext = configContext;
        Mode = mode;
        if (_configContext is INotifyPropertyChanged notifyConfig)
        {
            notifyConfig.PropertyChanged += OnConfigContextPropertyChanged;
        }
    }

    public ConfigData ActiveConfig => _configContext.ActiveConfig;

    public string Summary =>
        $"Observer: {ActiveConfig.ObserverPosition} | House: {ActiveConfig.HouseSystem} | Ayanamsha: {ActiveConfig.Ayanamsha}";

    public string ModeLabel => Mode switch
    {
        ConfigHomeMode.New => "Mode: New configuration",
        ConfigHomeMode.Edit => "Mode: Edit configuration",
        _ => "Mode: Configuration overview"
    };

    private void OnConfigContextPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IConfigContext.ActiveConfig))
        {
            OnPropertyChanged(nameof(ActiveConfig));
            OnPropertyChanged(nameof(Summary));
        }
    }
}
