using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.AppShell.State;

public interface IChartContext
{
    FullChart? CurrentChart { get; set; }
}
