using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.AppShell.State;

public interface IConfigContext
{
    UserConfiguration ActiveConfig { get; set; }

    /// <summary>The configuration currently selected for editing in the config list. Null when none selected.</summary>
    UserConfiguration? EditingConfig { get; set; }
}
