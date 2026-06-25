// ConfigContext.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.AppShell.State;

public sealed partial class ConfigContext : ObservableObject, IConfigContext
{
    [ObservableProperty]
    private UserConfiguration _activeConfig = UserConfiguration.Default;

    [ObservableProperty]
    private UserConfiguration? _editingConfig;

    partial void OnActiveConfigChanged(UserConfiguration value) =>
        GlyphSelector.Configure(value.GlyphsConfig);
}
