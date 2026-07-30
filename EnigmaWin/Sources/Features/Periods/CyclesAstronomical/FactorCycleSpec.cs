// FactorCycleSpec.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Periods.CyclesAstronomical;

/// <summary>Calculation parameters for a factor used in period/cycle calculations.</summary>
/// <param name="Interval">Step size in days between calculated positions.</param>
/// <param name="MaxPeriod">Maximum allowed span between start and end date, in days.</param>
public record FactorCycleSpec(double Interval, int MaxPeriod);

/// <summary>Extension methods providing cycle specs for Factors.</summary>
public static class FactorCycleSpecExtensions
{
    /// <summary>Returns the cycle spec for this factor, or null if the factor is not supported.</summary>
    public static FactorCycleSpec? CycleSpec(this Factors factor) => factor switch
    {
        Factors.Moon =>
            new FactorCycleSpec(0.1, 2_000),
        Factors.Sun or Factors.Earth or Factors.Mercury or Factors.Venus or Factors.Mars
            or Factors.Pallas or Factors.Juno or Factors.Vesta or Factors.ApogeeMean =>
            new FactorCycleSpec(1, 20_000),
        Factors.Jupiter or Factors.Saturn or Factors.Ceres =>
            new FactorCycleSpec(5, 200_000),
        Factors.Uranus or Factors.Neptune or Factors.Pluto =>
            new FactorCycleSpec(10, 750_000),
        Factors.NorthNodeMean or Factors.SouthNodeMean or Factors.Chiron
            or Factors.Pholus or Factors.Nessus or Factors.Huya =>
            new FactorCycleSpec(10, 200_000),
        Factors.Priapus or Factors.PriapusKoch or Factors.PriapusDuval or Factors.PriapusInterpolated or Factors.Dragon or Factors.Beast
            or Factors.Diamond or Factors.Ascendant or Factors.Mc or Factors.EastPoint
            or Factors.Vertex or Factors.ZeroAries or Factors.FortunaSect or Factors.FortunaNoSect =>
            null,
        _ =>
            new FactorCycleSpec(10, 200_000)
    };
}
