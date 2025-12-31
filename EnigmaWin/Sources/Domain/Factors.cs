// Factors.cs
// EnigmaWin
// Created by Jan Kampherbeek on 27-12-2025

using System;

namespace EnigmaWin.Sources.Domain;

/// <summary>Methods for the calculation of a factor.</summary>
/// <remarks>
/// CommonSE: Calculation of celestial points via the Swiss Ephemeris.
/// CommonElements: Calculation of celestial points based on ecliptical elements.
/// CommonFormula: Calculation of celestial points based on a formula.
/// Mundane: Calculation of mundane points, either via the Swiss Ephemeris of based on a formula.
/// Lots: Calculation of Hellenistic lots.
/// </remarks>
public enum CalculationTypes
{
    Unknown = 0,
    CommonSe = 1,
    CommonElements = 2,
    CommonFormulaLongitude = 3,
    CommonFormulaFull = 4,
    Mundane = 5,
    Lots = 6,
    ZodiacFixed = 7,
    Apsides = 8
}

/// <summary>Factors, lights, planets, mathematical points etc. Solar system points.</summary>
public enum Factors
{
    Sun = 0,
    Moon = 1,
    Mercury = 2,
    Venus = 3,
    Earth = 4,
    Mars = 5,
    Jupiter = 6,
    Saturn = 7,
    Uranus = 8,
    Neptune = 9,
    Pluto = 10,
    NorthNodeMean = 11,
    NorthNodeTrue = 12,
    Chiron = 13,
    PersephoneRam = 14,
    HermesRam = 15,
    DemeterRam = 16,
    CupidoUra = 17,
    HadesUra = 18,
    ZeusUra = 19,
    KronosUra = 20,
    ApollonUra = 21,
    AdmetosUra = 22,
    VulcanusUra = 23,
    PoseidonUra = 24,
    Eris = 25,
    Pholus = 26,
    Ceres = 27,
    Pallas = 28,
    Juno = 29,
    Vesta = 30,
    Isis = 31,
    Nessus = 32,
    Huya = 33,
    Varuna = 34,
    Ixion = 35,
    Quaoar = 36,
    Haumea = 37,
    Orcus = 38,
    Makemake = 39,
    Sedna = 40,
    Hygieia = 41,
    Astraea = 42,
    ApogeeMean = 43,
    ApogeeCorrected = 44,
    ApogeeInterpolated = 45,
    PersephoneCarteret = 47,
    VulcanusCarteret = 48,
    PerigeeInterpolated = 49,
    Priapus = 50,
    PriapusCorrected = 51,
    Dragon = 52,
    Beast = 53,
    SouthNodeMean = 54,
    SouthNodeTrue = 55,
    BlackSun = 56,
    Diamond = 57,
    Ascendant = 1001,
    Mc = 1002,
    EastPoint = 1003,
    Vertex = 1004,
    ZeroAries = 3001,
    FortunaSect = 4001,
    FortunaNoSect = 4002
}

/// <summary>Extension methods for the Factors enum.</summary>
public static class FactorsExtensions
{
    /// <summary>Indicates if the calculation of this point can be performed by the Swiss Ephemeris.</summary>
    /// <param name="factor">The factor.</param>
    /// <returns>The calculation type for this factor.</returns>
    public static CalculationTypes CalculationType(this Factors factor)
    {
        return factor switch
        {
            Factors.Sun or Factors.Moon or Factors.Mercury or Factors.Venus or Factors.Earth or Factors.Mars or
            Factors.Jupiter or Factors.Saturn or Factors.Uranus or Factors.Neptune or Factors.Pluto or
            Factors.NorthNodeMean or Factors.NorthNodeTrue or Factors.ApogeeMean or Factors.ApogeeCorrected or
            Factors.Chiron or Factors.Pholus or Factors.Ceres or Factors.Pallas or Factors.Juno or Factors.Vesta or
            Factors.ApogeeInterpolated or Factors.PerigeeInterpolated or Factors.CupidoUra or Factors.HadesUra or
            Factors.ZeusUra or Factors.KronosUra or Factors.ApollonUra or Factors.AdmetosUra or
            Factors.VulcanusUra or Factors.PoseidonUra or Factors.Isis or Factors.Eris or Factors.Nessus or
            Factors.Huya or Factors.Varuna or Factors.Ixion or Factors.Quaoar or Factors.Haumea or Factors.Orcus or
            Factors.Makemake or Factors.Sedna or Factors.Hygieia or Factors.Astraea => CalculationTypes.CommonSe,
            Factors.PersephoneRam or Factors.HermesRam or Factors.DemeterRam => CalculationTypes.CommonElements,
            Factors.PersephoneCarteret or Factors.VulcanusCarteret => CalculationTypes.CommonFormulaLongitude,
            Factors.Priapus or Factors.PriapusCorrected or Factors.Dragon or Factors.Beast or
            Factors.SouthNodeMean or Factors.SouthNodeTrue => CalculationTypes.CommonFormulaFull,
            Factors.Mc or Factors.Ascendant or Factors.EastPoint or Factors.Vertex => CalculationTypes.Mundane,
            Factors.ZeroAries => CalculationTypes.ZodiacFixed,
            Factors.FortunaSect or Factors.FortunaNoSect => CalculationTypes.Lots,
            Factors.BlackSun or Factors.Diamond => CalculationTypes.Apsides,
            _ => CalculationTypes.Unknown
        };
    }

    /// <summary>Swiss Ephemeris index for this point.</summary>
    /// <param name="factor">The factor.</param>
    /// <returns>The Swiss Ephemeris index.</returns>
    public static int SeId(this Factors factor)
    {
        return factor switch
        {
            Factors.Sun => 0,
            Factors.Moon => 1,
            Factors.Mercury => 2,
            Factors.Venus => 3,
            Factors.Mars => 4,
            Factors.Jupiter => 5,
            Factors.Saturn => 6,
            Factors.Uranus => 7,
            Factors.Neptune => 8,
            Factors.Pluto => 9,
            Factors.NorthNodeMean => 10,
            Factors.NorthNodeTrue => 11,
            Factors.ApogeeMean => 12,
            Factors.ApogeeCorrected => 13,
            Factors.Earth => 14,
            Factors.Chiron => 15,
            Factors.Pholus => 16,
            Factors.Ceres => 17,
            Factors.Pallas => 18,
            Factors.Juno => 19,
            Factors.Vesta => 20,
            Factors.ApogeeInterpolated => 21,
            Factors.PerigeeInterpolated => 22,
            Factors.CupidoUra => 40,
            Factors.HadesUra => 41,
            Factors.ZeusUra => 42,
            Factors.KronosUra => 43,
            Factors.ApollonUra => 44,
            Factors.AdmetosUra => 45,
            Factors.VulcanusUra => 46,
            Factors.PoseidonUra => 47,
            Factors.Isis => 48,
            Factors.Eris => 1009001,
            Factors.Nessus => 17066,
            Factors.Huya => 48628,
            Factors.Varuna => 30000,
            Factors.Ixion => 38978,
            Factors.Quaoar => 60000,
            Factors.Haumea => 146108,
            Factors.Orcus => 100482,
            Factors.Makemake => 146472,
            Factors.Sedna => 100377,
            Factors.Hygieia => 10010,
            Factors.Astraea => 10005,
            Factors.PersephoneRam => 300,
            Factors.HermesRam => 301,
            Factors.DemeterRam => 302,
            Factors.PersephoneCarteret => 400,
            Factors.VulcanusCarteret => 401,
            Factors.Priapus => 501,
            Factors.PriapusCorrected => 502,
            Factors.Dragon => 503,
            Factors.Beast => 504,
            Factors.SouthNodeMean => 505,
            Factors.SouthNodeTrue => 506, // TODO check
            Factors.BlackSun => 601,
            Factors.Diamond => 602,
            Factors.Mc => 700,
            Factors.Ascendant => 701,
            Factors.EastPoint => 702,
            Factors.Vertex => 703,
            Factors.ZeroAries => 800,
            Factors.FortunaSect => 900,
            Factors.FortunaNoSect => 901,
            _ => 0
        };
    }

    /// <summary>Localized name key for this factor.</summary>
    /// <param name="factor">The factor.</param>
    /// <returns>The localized name key.</returns>
    public static string LocalizedName(this Factors factor)
    {
        return factor switch
        {
            Factors.Sun => "enum.factor.sun",
            Factors.Moon => "enum.factor.moon",
            Factors.Mercury => "enum.factor.mercury",
            Factors.Venus => "enum.factor.venus",
            Factors.Earth => "enum.factor.earth",
            Factors.Mars => "enum.factor.mars",
            Factors.Jupiter => "enum.factor.jupiter",
            Factors.Saturn => "enum.factor.saturn",
            Factors.Uranus => "enum.factor.uranus",
            Factors.Neptune => "enum.factor.neptune",
            Factors.Pluto => "enum.factor.pluto",
            Factors.NorthNodeMean => "enum.factor.northnode",
            Factors.NorthNodeTrue => "enum.factor.truenode",
            Factors.Chiron => "enum.factor.chiron",
            Factors.PersephoneRam => "enum.factor.persephoneram",
            Factors.HermesRam => "enum.factor.hermesram",
            Factors.DemeterRam => "enum.factor.demeterram",
            Factors.CupidoUra => "enum.factor.cupidoura",
            Factors.HadesUra => "enum.factor.hadesura",
            Factors.ZeusUra => "enum.factor.zeusura",
            Factors.KronosUra => "enum.factor.kronosura",
            Factors.ApollonUra => "enum.factor.apollonura",
            Factors.AdmetosUra => "enum.factor.admetosura",
            Factors.VulcanusUra => "enum.factor.vulcanusura",
            Factors.PoseidonUra => "enum.factor.poseidonura",
            Factors.Eris => "enum.factor.eris",
            Factors.Pholus => "enum.factor.pholus",
            Factors.Ceres => "enum.factor.ceres",
            Factors.Pallas => "enum.factor.pallas",
            Factors.Juno => "enum.factor.juno",
            Factors.Vesta => "enum.factor.vesta",
            Factors.Isis => "enum.factor.isis",
            Factors.Nessus => "enum.factor.nessus",
            Factors.Huya => "enum.factor.huya",
            Factors.Varuna => "enum.factor.varuna",
            Factors.Ixion => "enum.factor.ixion",
            Factors.Quaoar => "enum.factor.quaoar",
            Factors.Haumea => "enum.factor.haumea",
            Factors.Orcus => "enum.factor.orcus",
            Factors.Makemake => "enum.factor.makemake",
            Factors.Sedna => "enum.factor.sedna",
            Factors.Hygieia => "enum.factor.hygieia",
            Factors.Astraea => "enum.factor.astraea",
            Factors.ApogeeMean => "enum.factor.apogeemean",
            Factors.ApogeeCorrected => "enum.factor.apogeecorrected",
            Factors.ApogeeInterpolated => "enum.factor.apogeeinterpolated",
            Factors.PersephoneCarteret => "enum.factor.persephonecarteret",
            Factors.VulcanusCarteret => "enum.factor.vulcanuscarteret",
            Factors.PerigeeInterpolated => "enum.factor.perigeeinterpolated",
            Factors.Priapus => "enum.factor.priapus",
            Factors.PriapusCorrected => "enum.factor.priapuscorrected",
            Factors.Dragon => "enum.factor.dragon",
            Factors.Beast => "enum.factor.beast",
            Factors.SouthNodeMean => "enum.factor.southnodemean",
            Factors.SouthNodeTrue => "enum.factor.southnodetrue",
            Factors.BlackSun => "enum.factor.blacksun",
            Factors.Diamond => "enum.factor.diamond",
            Factors.Ascendant => "enum.factor.ascendant",
            Factors.Mc => "enum.factor.mc",
            Factors.EastPoint => "enum.factor.eastpoint",
            Factors.Vertex => "enum.factor.vertex",
            Factors.ZeroAries => "enum.factor.zeroaries",
            Factors.FortunaSect => "enum.factor.fortunasect",
            Factors.FortunaNoSect => "enum.factor.fortunanosect",
            _ => string.Empty
        };
    }

    /// <summary>Get a Factors enum value from an index.</summary>
    /// <param name="index">The index (0-based).</param>
    /// <returns>The Factors enum value, or null if the index is out of range.</returns>
    public static Factors? FromIndex(int index)
    {
        var allCases = Enum.GetValues<Factors>();
        if (index < 0 || index >= allCases.Length)
        {
            return null;
        }
        return allCases[index];
    }
}

