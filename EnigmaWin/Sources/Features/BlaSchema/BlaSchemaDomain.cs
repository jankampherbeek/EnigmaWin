// BlaSchemaDomain.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.BlaSchema;

/// <summary>Correction method for the "corrected" Black Moon (Apogee) and Priapus points shown alongside the mean position in the BLA schema.</summary>
public enum BlaApogeeCorrectionType
{
    Koch,
    Duval,
    Interpolated
}

/// <summary>Extension methods for <see cref="BlaApogeeCorrectionType"/>.</summary>
public static class BlaApogeeCorrectionTypeExtensions
{
    public static Factors ApogeeFactor(this BlaApogeeCorrectionType correctionType) => correctionType switch
    {
        BlaApogeeCorrectionType.Koch => Factors.ApogeeKoch,
        BlaApogeeCorrectionType.Duval => Factors.ApogeeDuval,
        BlaApogeeCorrectionType.Interpolated => Factors.ApogeeInterpolated,
        _ => Factors.ApogeeKoch
    };

    public static Factors PriapusFactor(this BlaApogeeCorrectionType correctionType) => correctionType switch
    {
        BlaApogeeCorrectionType.Koch => Factors.PriapusKoch,
        BlaApogeeCorrectionType.Duval => Factors.PriapusDuval,
        BlaApogeeCorrectionType.Interpolated => Factors.PriapusInterpolated,
        _ => Factors.PriapusKoch
    };
}

/// <summary>Ecliptical longitudes for points and house cusps, as used by the BLA schema calculations.</summary>
/// <param name="Points">Chart points and their ecliptical longitude</param>
/// <param name="Cusps">House cusps (1..12) and their ecliptical longitude</param>
public sealed record BlaChartLongitudes(Dictionary<Factors, double> Points, Dictionary<int, double> Cusps);

/// <summary>Details for a chart point that are relevant for the BLA schema calculations.</summary>
public sealed record BlaPointDetails(
    Factors Point,
    double Longitude,
    int Sign,
    int Decanate,
    int House,
    int MainRuledSign,
    int SubRuledSign,
    List<int> MainRuledHouses,
    List<int> SubRuledHouses);

/// <summary>Details for a house that are relevant for the BLA schema calculations.</summary>
public sealed record BlaHouseDetails(
    int HouseNr,
    int SignOnCusp,
    int Decanate,
    double Longitude,
    Factors MainRuler,
    Factors SubRuler,
    List<Factors> PointsInHouse);

/// <summary>Counts for a sign, house and cusp in a BLA schema.</summary>
public sealed record BlaSignHouseCountLine(int Sign, int House, int Sum, int HCusp, int Total);

/// <summary>Full specs of a BLA data line for dispositors (specs for a ruler pair).</summary>
public sealed record BlaDispositorLine(
    Factors MainRuler,
    Factors SubRuler,
    int MainRulerSignCount,
    int SubRulerSignCount,
    int SumRulerSignCount,
    int IndirectRulerSignCount,
    int TotalRulerSignCount,
    int DirectRulerHouseCount,
    int IndirectRulerHouseCount,
    int SumRulerHouseCount,
    int DirectRulerDecanateCount,
    int Total);

/// <summary>Several details for the BLA Schema that are combined in one visual rectangle.</summary>
public sealed record BlaDetailsData(
    int SisterSignAsc,
    List<int> ClampedHouses,
    List<int> InterceptedSigns,
    List<int> GroundNote,
    List<int> LordAscInHouses,
    int MoonInHouse);

/// <summary>Cyclic connections in a BLA schema. Each connection consists of two houses (1..12). The ruler of the first house is located in the second house.</summary>
public sealed record BlaCyclesData(
    List<(int, int)> Cardinal,
    List<(int, int)> Fixed,
    List<(int, int)> Mutable,
    List<(int, int)> Fire,
    List<(int, int)> Earth,
    List<(int, int)> Air,
    List<(int, int)> Water);

/// <summary>Factor pair in analog house and sign.</summary>
public sealed record FactorPairAnalogHouseSign(Factors PointInSign, int Sign, Factors PointInHouse, int House);

/// <summary>Reception of 2 points, either in signs or in houses.</summary>
public sealed record Reception(Factors Point1, int SignOrHouse1, Factors Point2, int SignOrHouse2);

/// <summary>Combination of ruler and subruler.</summary>
public sealed record RulerPair(int SignIndex, Factors MainRuler, Factors SubRuler);

/// <summary>Ruler and the signs it rules.</summary>
public sealed record RulerAndSigns(Factors Ruler, int MainSign, int SubSign);

/// <summary>Chaldean-order ruler for one of the 7 BLA decanates.</summary>
public sealed record DecanateRuler(Factors Ruler, int Decan);

/// <summary>
/// Domain for BLA schema ("Invisible Luminaries Astrology") calculations, ported from
/// Enigma.Core/Slices/BlaSchema/BlaDomain.cs.
/// </summary>
public static class BlaSchemaDomain
{
    /// <summary>The fixed BLA point set requested from AstronCalcOrchestrator (Ascendant/Mc come from house positions instead).</summary>
    public static List<Factors> BuildRequestedFactors(bool useTrueNode, bool useChiron, bool useCeres, BlaApogeeCorrectionType correctionType)
    {
        var factors = new List<Factors>
        {
            Factors.Sun, Factors.Moon, Factors.Mercury, Factors.Venus, Factors.Mars,
            Factors.Jupiter, Factors.Saturn, Factors.Uranus, Factors.Neptune, Factors.Pluto,
            useTrueNode ? Factors.NorthNodeTrue : Factors.NorthNodeMean,
            useTrueNode ? Factors.SouthNodeTrue : Factors.SouthNodeMean,
            Factors.PersephoneCarteret, Factors.VulcanusCarteret,
            Factors.ApogeeMean, correctionType.ApogeeFactor(),
            Factors.BlackSun, Factors.Diamond,
            Factors.Priapus, correctionType.PriapusFactor(),
            Factors.Dragon, Factors.Beast,
            Factors.FortunaNoSect
        };
        if (useChiron) factors.Add(Factors.Chiron);
        if (useCeres) factors.Add(Factors.Ceres);
        return factors;
    }

    /// <summary>The canonical BLA point order (requested factors, plus Ascendant/Mc at the end) used to keep
    /// every list built from a Dictionary&lt;Factors,...&gt; in a stable, reproducible order.</summary>
    public static List<Factors> BuildCanonicalPointOrder(bool useTrueNode, bool useChiron, bool useCeres, BlaApogeeCorrectionType correctionType)
    {
        var factors = BuildRequestedFactors(useTrueNode, useChiron, useCeres, correctionType);
        factors.Add(Factors.Ascendant);
        factors.Add(Factors.Mc);
        return factors;
    }

    /// <summary>True for the two angle-type factors that can appear in a BLA chart's point set
    /// (Ascendant/Mc are not placed in a house — they define house cusps themselves).</summary>
    public static bool IsAngle(Factors factor) => factor is Factors.Ascendant or Factors.Mc;

    /// <summary>Main/sub ruler for each of the 12 signs.</summary>
    public static List<RulerPair> RulerPairs()
    {
        return
        [
            new RulerPair(1, Factors.Mars, Factors.Pluto),
            new RulerPair(2, Factors.Venus, Factors.PersephoneCarteret),
            new RulerPair(3, Factors.Mercury, Factors.VulcanusCarteret),
            new RulerPair(4, Factors.Moon, Factors.Priapus),
            new RulerPair(5, Factors.Sun, Factors.ApogeeMean),
            new RulerPair(6, Factors.VulcanusCarteret, Factors.Mercury),
            new RulerPair(7, Factors.PersephoneCarteret, Factors.Venus),
            new RulerPair(8, Factors.Pluto, Factors.Mars),
            new RulerPair(9, Factors.Jupiter, Factors.Neptune),
            new RulerPair(10, Factors.ApogeeMean, Factors.Sun),
            new RulerPair(11, Factors.Priapus, Factors.Moon),
            new RulerPair(12, Factors.Neptune, Factors.Jupiter)
        ];
    }

    /// <summary>Inverse view of <see cref="RulerPairs"/>: each of the 12 sign-rulers with its main and sub ruled sign.</summary>
    public static List<RulerAndSigns> AllRulerAndSigns()
    {
        return
        [
            new RulerAndSigns(Factors.Sun, 5, 10),
            new RulerAndSigns(Factors.Moon, 4, 11),
            new RulerAndSigns(Factors.Mercury, 3, 6),
            new RulerAndSigns(Factors.Venus, 2, 7),
            new RulerAndSigns(Factors.Mars, 1, 8),
            new RulerAndSigns(Factors.Jupiter, 9, 12),
            new RulerAndSigns(Factors.Neptune, 12, 9),
            new RulerAndSigns(Factors.Pluto, 8, 1),
            new RulerAndSigns(Factors.PersephoneCarteret, 7, 2),
            new RulerAndSigns(Factors.VulcanusCarteret, 6, 3),
            new RulerAndSigns(Factors.ApogeeMean, 10, 5),
            new RulerAndSigns(Factors.Priapus, 11, 4)
        ];
    }

    /// <summary>Logical pairs of related factors, used to detect "analog house/sign" reinforcements.
    /// Takes the actual north/south node factor in play (mean or true) so the pairs match whatever
    /// was requested for this chart.</summary>
    public static List<(Factors, Factors)> FactorPairs(Factors northNode, Factors southNode)
    {
        return
        [
            (Factors.Mars, Factors.Pluto),
            (Factors.Venus, Factors.PersephoneCarteret),
            (Factors.Mercury, Factors.VulcanusCarteret),
            (Factors.Moon, Factors.Priapus),
            (Factors.Sun, Factors.ApogeeMean),
            (Factors.Jupiter, Factors.Neptune),
            (Factors.Saturn, Factors.Uranus),
            (northNode, southNode),
            (northNode, Factors.Beast),
            (northNode, Factors.Dragon),
            (southNode, Factors.Beast),
            (southNode, Factors.Dragon),
            (Factors.Beast, Factors.Dragon)
        ];
    }

    /// <summary>Chaldean-order rulers for the 7-decanate cycle used by the BLA schema.</summary>
    public static List<DecanateRuler> DecanateRulers()
    {
        return
        [
            new DecanateRuler(Factors.Mars, 1),
            new DecanateRuler(Factors.Sun, 2),
            new DecanateRuler(Factors.Venus, 3),
            new DecanateRuler(Factors.Mercury, 4),
            new DecanateRuler(Factors.Moon, 5),
            new DecanateRuler(Factors.Saturn, 6),
            new DecanateRuler(Factors.Jupiter, 7)
        ];
    }
}
