// AgePointInputViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Progressive.AgePoint.UI;

public sealed class AgePointInputViewModel(AgePointViewModel inner)
{
    public AgePointViewModel Inner { get; } = inner;
}
