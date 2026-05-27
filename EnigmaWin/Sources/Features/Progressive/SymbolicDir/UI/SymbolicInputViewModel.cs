// SymbolicInputViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Progressive.SymbolicDir.UI;

public sealed class SymbolicInputViewModel(SymbolicViewModel inner)
{
    public SymbolicViewModel Inner { get; } = inner;
}
