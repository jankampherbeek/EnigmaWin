// EnneagramDetailsLine.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Enneagram;

public record EnneagramDetailsLine(Factors Factor, int PositionIndex, bool InSigns, double[] TypeFactors);
