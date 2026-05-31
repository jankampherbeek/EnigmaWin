// JulianDayRouteViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Features.Calculators.UI;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.ViewModels.Routes;

public sealed class JulianDayRouteViewModel : JulianDayViewModel
{
    public JulianDayRouteViewModel(IRosetta rosetta) : base(rosetta) { }
}
