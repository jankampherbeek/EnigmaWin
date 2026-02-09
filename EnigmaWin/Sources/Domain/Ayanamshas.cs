// Ayanamshas.cs
// EnigmaWin
// Created by Jan Kampherbeek on 27-12-2025

using System;

namespace EnigmaWin.Sources.Domain;

/// <summary>Supported ayanamshas for sidereal calculations.</summary>
public enum Ayanamshas
{
    Tropical = 0,
    Fagan = 1,
    Lahiri = 2,
    DeLuce = 3,
    Raman = 4,
    UshaShashi = 5,
    Krishnamurti = 6,
    DjwhalKhul = 7,
    Yukteshwar = 8,
    Bhasin = 9,
    Kugler1 = 10,
    Kugler2 = 11,
    Kugler3 = 12,
    Huber = 13,
    EtaPiscium = 14,
    Aldebaran15Tau = 15,
    Hipparchus = 16,
    Sassanian = 17,
    GalactCtr0Sag = 18,
    J2000 = 19,
    J1900 = 20,
    B1950 = 21,
    SuryaSiddhanta = 22,
    SuryaSiddhantaMeanSun = 23,
    Aryabhata = 24,
    AryabhataMeanSun = 25,
    SsRevati = 26,
    SsCitra = 27,
    TrueCitra = 28,
    TrueRevati = 29,
    TruePushya = 30,
    GalacticCtrBrand = 31,
    GalacticEqIau1958 = 32,
    GalacticEq = 33,
    GalacticEqMidMula = 34,
    Skydram = 35,
    TrueMula = 36,
    Dhruva = 37,
    Aryabhata522 = 38,
    Britton = 39,
    GalacticCtrOCap = 40
}

/// <summary>Extension methods for the Ayanamshas enum.</summary>
public static class AyanamshasExtensions
{
    /// <summary>Swiss Ephemeris identifier for this ayanamsha.</summary>
    /// <param name="ayanamsha">The ayanamsha.</param>
    /// <returns>The Swiss Ephemeris identifier.</returns>
    public static int SeId(this Ayanamshas ayanamsha)
    {
        return ayanamsha switch
        {
            Ayanamshas.Tropical => -1,
            Ayanamshas.Fagan => 0,
            Ayanamshas.Lahiri => 1,
            Ayanamshas.DeLuce => 2,
            Ayanamshas.Raman => 3,
            Ayanamshas.UshaShashi => 4,
            Ayanamshas.Krishnamurti => 5,
            Ayanamshas.DjwhalKhul => 6,
            Ayanamshas.Yukteshwar => 7,
            Ayanamshas.Bhasin => 8,
            Ayanamshas.Kugler1 => 9,
            Ayanamshas.Kugler2 => 10,
            Ayanamshas.Kugler3 => 11,
            Ayanamshas.Huber => 12,
            Ayanamshas.EtaPiscium => 13,
            Ayanamshas.Aldebaran15Tau => 14,
            Ayanamshas.Hipparchus => 15,
            Ayanamshas.Sassanian => 16,
            Ayanamshas.GalactCtr0Sag => 17,
            Ayanamshas.J2000 => 18,
            Ayanamshas.J1900 => 19,
            Ayanamshas.B1950 => 20,
            Ayanamshas.SuryaSiddhanta => 21,
            Ayanamshas.SuryaSiddhantaMeanSun => 22,
            Ayanamshas.Aryabhata => 23,
            Ayanamshas.AryabhataMeanSun => 24,
            Ayanamshas.SsRevati => 25,
            Ayanamshas.SsCitra => 26,
            Ayanamshas.TrueCitra => 27,
            Ayanamshas.TrueRevati => 28,
            Ayanamshas.TruePushya => 29,
            Ayanamshas.GalacticCtrBrand => 30,
            Ayanamshas.GalacticEqIau1958 => 31,
            Ayanamshas.GalacticEq => 32,
            Ayanamshas.GalacticEqMidMula => 33,
            Ayanamshas.Skydram => 34,
            Ayanamshas.TrueMula => 35,
            Ayanamshas.Dhruva => 36,
            Ayanamshas.Aryabhata522 => 37,
            Ayanamshas.Britton => 38,
            Ayanamshas.GalacticCtrOCap => 39,
            _ => -1
        };
    }

    /// <summary>Localized name key for this ayanamsha.</summary>
    /// <param name="ayanamsha">The ayanamsha.</param>
    /// <returns>The localized name key.</returns>
    public static string LocalizedName(this Ayanamshas ayanamsha)
    {
        return ayanamsha switch
        {
            Ayanamshas.Tropical => "enum.ayanamsha.tropical",
            Ayanamshas.Fagan => "enum.ayanamsha.fagan",
            Ayanamshas.Lahiri => "enum.ayanamsha.lahiri",
            Ayanamshas.DeLuce => "enum.ayanamsha.deluce",
            Ayanamshas.Raman => "enum.ayanamsha.raman",
            Ayanamshas.UshaShashi => "enum.ayanamsha.ushashashi",
            Ayanamshas.Krishnamurti => "enum.ayanamsha.krishnamurti",
            Ayanamshas.DjwhalKhul => "enum.ayanamsha.djwhalkhul",
            Ayanamshas.Yukteshwar => "enum.ayanamsha.yukteshwar",
            Ayanamshas.Bhasin => "enum.ayanamsha.bhasin",
            Ayanamshas.Kugler1 => "enum.ayanamsha.kugler1",
            Ayanamshas.Kugler2 => "enum.ayanamsha.kugler2",
            Ayanamshas.Kugler3 => "enum.ayanamsha.kugler3",
            Ayanamshas.Huber => "enum.ayanamsha.huber",
            Ayanamshas.EtaPiscium => "enum.ayanamsha.etapiscium",
            Ayanamshas.Aldebaran15Tau => "enum.ayanamsha.aldebaran15tau",
            Ayanamshas.Hipparchus => "enum.ayanamsha.hipparchus",
            Ayanamshas.Sassanian => "enum.ayanamsha.sassanian",
            Ayanamshas.GalactCtr0Sag => "enum.ayanamsha.galcent0sag",
            Ayanamshas.J2000 => "enum.ayanamsha.j2000",
            Ayanamshas.J1900 => "enum.ayanamsha.j1900",
            Ayanamshas.B1950 => "enum.ayanamsha.b1950",
            Ayanamshas.SuryaSiddhanta => "enum.ayanamsha.suryasiddhanta",
            Ayanamshas.SuryaSiddhantaMeanSun => "enum.ayanamsha.suryasiddhantameansun",
            Ayanamshas.Aryabhata => "enum.ayanamsha.aryabhata",
            Ayanamshas.AryabhataMeanSun => "enum.ayanamsha.aryabhatameansun",
            Ayanamshas.SsRevati => "enum.ayanamsha.ssrevati",
            Ayanamshas.SsCitra => "enum.ayanamsha.sscitra",
            Ayanamshas.TrueCitra => "enum.ayanamsha.truecitrapaksha",
            Ayanamshas.TrueRevati => "enum.ayanamsha.truerevati",
            Ayanamshas.TruePushya => "enum.ayanamsha.truepushya",
            Ayanamshas.GalacticCtrBrand => "enum.ayanamsha.galcentbrand",
            Ayanamshas.GalacticEqIau1958 => "enum.ayanamsha.galcentiau1958",
            Ayanamshas.GalacticEq => "enum.ayanamsha.galequator",
            Ayanamshas.GalacticEqMidMula => "enum.ayanamsha.galequatormidmula",
            Ayanamshas.Skydram => "enum.ayanamsha.skydram",
            Ayanamshas.TrueMula => "enum.ayanamsha.truemula",
            Ayanamshas.Dhruva => "enum.ayanamsha.dhruva",
            Ayanamshas.Aryabhata522 => "enum.ayanamsha.aryabhata522",
            Ayanamshas.Britton => "enum.ayanamsha.britton",
            Ayanamshas.GalacticCtrOCap => "enum.ayanamsha.galcent0cap",
            _ => string.Empty
        };
    }

    /// <summary>Get an Ayanamshas enum value from an index.</summary>
    /// <param name="index">The index (0-based).</param>
    /// <returns>The Ayanamshas enum value, or null if the index is out of range.</returns>
    public static Ayanamshas? FromIndex(int index)
    {
        var allCases = Enum.GetValues<Ayanamshas>();
        if (index < 0 || index >= allCases.Length)
        {
            return null;
        }
        return allCases[index];
    }
}

