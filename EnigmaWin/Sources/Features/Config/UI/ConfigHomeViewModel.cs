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

    /// <summary>The configuration currently being viewed — the editing config if set, otherwise the active config.</summary>
    public UserConfiguration DisplayConfig => _configContext.EditingConfig ?? _configContext.ActiveConfig;

    public string ConfigName => DisplayConfig.Name;
    public bool IsActive => DisplayConfig.IsActive;

    public string Summary
    {
        get
        {
            var calc = DisplayConfig.CalculationConfig;
            return $"House: {calc.HouseSystem} | Ayanamsha: {calc.Ayanamsha} | Observer: {calc.ObserverPosition}";
        }
    }

    public string ModeLabel => Mode switch
    {
        ConfigHomeMode.New  => "Mode: New configuration",
        ConfigHomeMode.Edit => "Mode: Edit configuration",
        _                   => "Mode: Configuration overview"
    };

    private void OnConfigContextPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(IConfigContext.ActiveConfig) or nameof(IConfigContext.EditingConfig))
        {
            OnPropertyChanged(nameof(DisplayConfig));
            OnPropertyChanged(nameof(ConfigName));
            OnPropertyChanged(nameof(IsActive));
            OnPropertyChanged(nameof(Summary));
        }
    }
}
