using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.AppShell.State;

public interface IConfigContext
{
    ConfigData ActiveConfig { get; set; }
}
