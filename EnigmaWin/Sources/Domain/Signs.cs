// Signs.cs
// EnigmaWin
// Created by Jan Kampherbeek on 27-12-2025

namespace EnigmaWin.Sources.Domain;

/// <summary>Zodiac signs.</summary>
public enum Signs
{
    Aries = 1,
    Taurus = 2,
    Gemini = 3,
    Cancer = 4,
    Leo = 5,
    Virgo = 6,
    Libra = 7,
    Scorpio = 8,
    Sagittarius = 9,
    Capricorn = 10,
    Aquarius = 11,
    Pisces = 12
}

/// <summary>Extension methods for the Signs enum.</summary>
public static class SignsExtensions
{
    /// <summary>Localized name key for this sign.</summary>
    public static string LocalizedName(this Signs sign) => sign switch
    {
        Signs.Aries       => "enum.sign.aries",
        Signs.Taurus      => "enum.sign.taurus",
        Signs.Gemini      => "enum.sign.gemini",
        Signs.Cancer      => "enum.sign.cancer",
        Signs.Leo         => "enum.sign.leo",
        Signs.Virgo       => "enum.sign.virgo",
        Signs.Libra       => "enum.sign.libra",
        Signs.Scorpio     => "enum.sign.scorpio",
        Signs.Sagittarius => "enum.sign.sagittarius",
        Signs.Capricorn   => "enum.sign.capricorn",
        Signs.Aquarius    => "enum.sign.aquarius",
        Signs.Pisces      => "enum.sign.pisces",
        _                 => string.Empty
    };
}

