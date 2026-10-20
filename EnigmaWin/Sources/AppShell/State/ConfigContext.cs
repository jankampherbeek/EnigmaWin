// ConfigContext.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.AppShell.State;

public sealed partial class ConfigContext : ObservableObject, IConfigContext
{
    private UserConfiguration _activeConfig = UserConfiguration.Default;

    [ObservableProperty]
    private UserConfiguration? _editingConfig;

    /// <summary>The active configuration, with the factor settings as they apply to its observer position
    /// (in a heliocentric configuration Earth takes the place of the Sun).</summary>
    public UserConfiguration ActiveConfig
    {
        get => _activeConfig;
        set
        {
            if (!SetProperty(ref _activeConfig, value.ForCalculation())) return;
            GlyphSelector.Configure(_activeConfig.GlyphsConfig);
            SignColorSelector.Configure(_activeConfig.DisplayConfig);
        }
    }
}
