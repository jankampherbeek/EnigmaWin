// ParansResults.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.FixStars;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Parans;

public enum ParanType { Rising, Setting, Culmination, AntiCulmination }

public static class ParanTypeExtensions
{
    public static int Rsmi(this ParanType t) => t switch
    {
        ParanType.Rising          => 1 | 256 | 512,
        ParanType.Setting         => 2 | 256 | 512,
        ParanType.Culmination     => 4,
        ParanType.AntiCulmination => 8,
        _                         => 0
    };

    public static ParanType[] All =>
        [ParanType.Rising, ParanType.Setting, ParanType.Culmination, ParanType.AntiCulmination];
}

public enum ParanBodyKind { Factor, Star }

public sealed record ParanTimesForBody(
    ParanBodyKind Kind,
    Factors? Factor,
    StarDefinitions? Star,
    double? Rising,
    double? Setting,
    double? Culmination,
    double? AntiCulmination)
{
    public double? TimeFor(ParanType t) => t switch
    {
        ParanType.Rising          => Rising,
        ParanType.Setting         => Setting,
        ParanType.Culmination     => Culmination,
        ParanType.AntiCulmination => AntiCulmination,
        _                         => null
    };

    public string DisplayName =>
        Kind == ParanBodyKind.Factor ? (Factor?.ToString() ?? "") : (Star?.Name ?? "");
}

public sealed record ParanMatch(
    ParanTimesForBody Body1,
    ParanType Type1,
    double Time1,
    ParanTimesForBody Body2,
    ParanType Type2,
    double Time2,
    double OrbMinutes);

public sealed record ParansResult(
    IReadOnlyList<ParanTimesForBody> AllTimes,
    IReadOnlyList<ParanMatch> Matches);
