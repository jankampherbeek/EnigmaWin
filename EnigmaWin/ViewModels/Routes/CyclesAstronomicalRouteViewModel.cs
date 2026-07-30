// CyclesAstronomicalRouteViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Features.Periods.CyclesAstronomical.UI;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.ViewModels.Routes;

public sealed class CyclesAstronomicalRouteViewModel
{
    public AstronomicalCyclesScreenViewModel InputViewModel { get; }
    public CyclesChartViewModel              ChartViewModel { get; }

    public CyclesAstronomicalRouteViewModel(IRosetta rosetta, AstronomicalCyclesModel model)
    {
        InputViewModel = new AstronomicalCyclesScreenViewModel(rosetta, model);
        ChartViewModel = new CyclesChartViewModel(rosetta, model);
    }
}
