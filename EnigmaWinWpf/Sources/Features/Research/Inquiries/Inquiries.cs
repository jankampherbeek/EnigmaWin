// Inquiries.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Research.Inquiries;

/// <summary>Types of research inquiries that can be performed on a dataset.</summary>
public enum Inquiries
{
    FactorsInSigns = 0,
    FactorsInHouses = 1,
    Aspects = 2,
    Unaspect = 3,
    Midpoints = 4,
    Harmonics = 5,
    Parallels = 6,
    DeclMidpoints = 7,
    Oob = 8
}

public static class InquiriesExtensions
{
    /// <summary>Whether this inquiry uses ecliptical longitude.
    /// OOB, parallels, and declination midpoints use equatorial declination instead.</summary>
    public static bool UsesEcliptical(this Inquiries inquiry) =>
        inquiry != Inquiries.Oob && inquiry != Inquiries.Parallels && inquiry != Inquiries.DeclMidpoints;

    /// <summary>Whether this inquiry uses equatorial declination.</summary>
    public static bool UsesEquatorial(this Inquiries inquiry) => !inquiry.UsesEcliptical();

    /// <summary>Whether this inquiry requires house cusps (only FactorsInHouses).</summary>
    public static bool RequiresHouses(this Inquiries inquiry) => inquiry == Inquiries.FactorsInHouses;
}
