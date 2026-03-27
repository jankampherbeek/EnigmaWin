// IConfigContext.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.AppShell.State;

public interface IConfigContext
{
    UserConfiguration ActiveConfig { get; set; }

    /// <summary>The configuration currently selected for editing in the config list. Null when none selected.</summary>
    UserConfiguration? EditingConfig { get; set; }
}
