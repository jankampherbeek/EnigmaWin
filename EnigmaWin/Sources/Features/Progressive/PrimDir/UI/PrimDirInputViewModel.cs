// PrimDirInputViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Progressive.PrimDir.UI;

public sealed class PrimDirInputViewModel(PrimDirViewModel inner)
{
    public PrimDirViewModel Inner { get; } = inner;
}
