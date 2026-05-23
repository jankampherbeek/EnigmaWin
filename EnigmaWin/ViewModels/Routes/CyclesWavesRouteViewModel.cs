// CyclesWavesRouteViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Features.Cycles.CyclesWaves.UI;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.ViewModels.Routes;

public sealed class CyclesWavesRouteViewModel
{
    public WavesScreenViewModel InputViewModel { get; }
    public WavesChartViewModel  ChartViewModel { get; }

    public CyclesWavesRouteViewModel(IRosetta rosetta, WavesModel model)
    {
        InputViewModel = new WavesScreenViewModel(rosetta, model);
        ChartViewModel = new WavesChartViewModel(rosetta, model);
    }
}
