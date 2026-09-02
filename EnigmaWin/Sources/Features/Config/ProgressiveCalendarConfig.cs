// ProgressiveCalendarConfig.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Config;

/// <summary>Configuration for the Progressive Calendar feature: which techniques and factors to
/// include, the radix factors to compare against, which event kinds to include, and orbs.</summary>
/// <remarks>
/// Like <see cref="PrimaryDirectionsConfig"/>'s method/approach/timeKey, these settings are meant
/// to be edited directly in the Progressive Calendar feature's own input screen, not only through
/// the generic Config editor — this struct only supplies the persisted defaults that screen seeds
/// its local editable state from.
/// </remarks>
public readonly record struct ProgressiveCalendarConfig(
    bool UseTransits,
    bool UseSecondaryDirections,
    bool UseSymbolicDirections,
    IReadOnlyList<Factors> TransitFactors,
    IReadOnlyList<Factors> SecondaryDirectionFactors,
    IReadOnlyList<Factors> SymbolicDirectionFactors,
    SymbolicKeys SymbolicKey,
    IReadOnlyList<Factors> RadixFactors,
    IReadOnlyList<Aspects> Aspects,
    double AspectOrb,
    double ParallelOrb,
    double CuspOrb,
    bool UseAspectsToRadix,
    bool UseParallelsToRadix,
    bool UseAspectsProgToProg,
    bool UseParallelsProgToProg,
    bool UseCuspConjunctions,
    bool UseRetrogradeDirectStations,
    bool UseOobEnterExit,
    bool UseDeclinationExtremes)
{
    public static ProgressiveCalendarConfig Default => new(
        UseTransits: true,
        UseSecondaryDirections: true,
        UseSymbolicDirections: false,
        TransitFactors: DefaultTransitFactors,
        SecondaryDirectionFactors: DefaultSecondaryDirectionFactors,
        SymbolicDirectionFactors: DefaultSymbolicDirectionFactors,
        SymbolicKey: SymbolicKeys.OneDegree,
        RadixFactors: DefaultRadixFactors,
        Aspects: DefaultAspects,
        AspectOrb: 1.0,
        ParallelOrb: 1.0,
        CuspOrb: 1.0,
        UseAspectsToRadix: true,
        UseParallelsToRadix: true,
        UseAspectsProgToProg: false,
        UseParallelsProgToProg: false,
        UseCuspConjunctions: true,
        UseRetrogradeDirectStations: true,
        UseOobEnterExit: false,
        UseDeclinationExtremes: false);

    public static IReadOnlyList<Factors> DefaultTransitFactors => TransitsConfig.DefaultFactors;
    public static IReadOnlyList<Factors> DefaultSecondaryDirectionFactors => SecondaryDirectionsConfig.DefaultFactors;
    public static IReadOnlyList<Factors> DefaultSymbolicDirectionFactors => SymbolicDirectionsConfig.DefaultFactors;
    public static IReadOnlyList<Factors> DefaultRadixFactors => TransitsConfig.DefaultFactors;
    public static IReadOnlyList<Aspects> DefaultAspects =>
    [
        Domain.Aspects.Conjunction, Domain.Aspects.Opposition, Domain.Aspects.Trine,
        Domain.Aspects.Square, Domain.Aspects.Sextile
    ];
}
