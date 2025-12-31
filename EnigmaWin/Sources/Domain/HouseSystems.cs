// HouseSystems.cs
// EnigmaWin
// Created by Jan Kampherbeek on 27-12-2025

using System;

namespace EnigmaWin.Sources.Domain;

/// <summary>House systems for astrological calculations.</summary>
public enum HouseSystems
{
    NoHouses = 0,
    Placidus = 1,
    Koch = 2,
    Porphyri = 3,
    Regiomontanus = 4,
    Campanus = 5,
    Alcabitius = 6,
    TopoCentric = 7,
    Krusinski = 8,
    Apc = 9,
    Morin = 10,
    WholeSign = 11,
    EqualAsc = 12,
    EqualMc = 13,
    EqualAries = 14,
    Vehlow = 15,
    Axial = 16,
    Horizon = 17,
    Carter = 18,
    Gauquelin = 19,
    SunShine = 20,
    SunShineTreindl = 21,
    PullenSd = 22,
    PullenSr = 23,
    Sripati = 24
}

/// <summary>Extension methods for the HouseSystems enum.</summary>
public static class HouseSystemsExtensions
{
    /// <summary>Swiss Ephemeris identifier for this house system.</summary>
    /// <param name="houseSystem">The house system.</param>
    /// <returns>The Swiss Ephemeris identifier character.</returns>
    public static char SeId(this HouseSystems houseSystem)
    {
        return houseSystem switch
        {
            HouseSystems.NoHouses => 'W',
            HouseSystems.Placidus => 'P',
            HouseSystems.Koch => 'K',
            HouseSystems.Porphyri => 'O',
            HouseSystems.Regiomontanus => 'R',
            HouseSystems.Campanus => 'C',
            HouseSystems.Alcabitius => 'B',
            HouseSystems.TopoCentric => 'T',
            HouseSystems.Krusinski => 'U',
            HouseSystems.Apc => 'Y',
            HouseSystems.Morin => 'M',
            HouseSystems.WholeSign => 'W',
            HouseSystems.EqualAsc => 'A',
            HouseSystems.EqualMc => 'D',
            HouseSystems.EqualAries => 'N',
            HouseSystems.Vehlow => 'V',
            HouseSystems.Axial => 'X',
            HouseSystems.Horizon => 'H',
            HouseSystems.Carter => 'F',
            HouseSystems.Gauquelin => 'G',
            HouseSystems.SunShine => 'i',
            HouseSystems.SunShineTreindl => 'I',
            HouseSystems.PullenSd => 'L',
            HouseSystems.PullenSr => 'Q',
            HouseSystems.Sripati => 'S',
            _ => ' '
        };
    }

    /// <summary>Localized name key for this house system.</summary>
    /// <param name="houseSystem">The house system.</param>
    /// <returns>The localized name key.</returns>
    public static string LocalizedName(this HouseSystems houseSystem)
    {
        return houseSystem switch
        {
            HouseSystems.NoHouses => "enum.housesystem.nohouses",
            HouseSystems.Placidus => "enum.housesystem.placidus",
            HouseSystems.Koch => "enum.housesystem.koch",
            HouseSystems.Porphyri => "enum.housesystem.porphyri",
            HouseSystems.Regiomontanus => "enum.housesystem.regiomontanus",
            HouseSystems.Campanus => "enum.housesystem.campanus",
            HouseSystems.Alcabitius => "enum.housesystem.alcabitius",
            HouseSystems.TopoCentric => "enum.housesystem.topocentric",
            HouseSystems.Krusinski => "enum.housesystem.krusinski",
            HouseSystems.Apc => "enum.housesystem.apc",
            HouseSystems.Morin => "enum.housesystem.morin",
            HouseSystems.WholeSign => "enum.housesystem.whole_sign",
            HouseSystems.EqualAsc => "enum.housesystem.equal_from_ascendant",
            HouseSystems.EqualMc => "enum.housesystem.equal_from_mc",
            HouseSystems.EqualAries => "enum.housesystem.equal_from_0_aries",
            HouseSystems.Vehlow => "enum.housesystem.vehlow",
            HouseSystems.Axial => "enum.housesystem.axial_rotation",
            HouseSystems.Horizon => "enum.housesystem.horizon",
            HouseSystems.Carter => "enum.housesystem.carter",
            HouseSystems.Gauquelin => "enum.housesystem.gauquelin",
            HouseSystems.SunShine => "enum.housesystem.sunshine",
            HouseSystems.SunShineTreindl => "enum.housesystem.sunshine_treindl",
            HouseSystems.PullenSd => "enum.housesystem.pullen_sin_diff",
            HouseSystems.PullenSr => "enum.housesystem.pullen_sin_ratio",
            HouseSystems.Sripati => "enum.housesys.sripati",
            _ => string.Empty
        };
    }

    /// <summary>Get a HouseSystems enum value from an index.</summary>
    /// <param name="index">The index (0-based).</param>
    /// <returns>The HouseSystems enum value, or null if the index is out of range.</returns>
    public static HouseSystems? FromIndex(int index)
    {
        var allCases = Enum.GetValues<HouseSystems>();
        if (index < 0 || index >= allCases.Length)
        {
            return null;
        }
        return allCases[index];
    }
}


