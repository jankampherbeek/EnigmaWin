// SolarInputViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Progressive.Solar.UI;

public sealed class SolarInputViewModel(SolarViewModel inner)
{
    public SolarViewModel Inner { get; } = inner;
}
