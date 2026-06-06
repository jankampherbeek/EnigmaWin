// Bounds.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.ZodiacDivisions;

/// <summary>Term/bound rulers using Egyptian or Ptolemy tables.</summary>
/// <remarks>Planet indices: Sun=0, Moon=1, Mercury=2, Venus=3, Mars=4, Jupiter=5, Saturn=6. Sun and Moon are not used as bound rulers.</remarks>
public static class Bounds
{
    private record TermPosition(int Degrees, int Planet);

    /// <param name="longitude">Ecliptic longitude, must be >= 0 and &lt; 360.</param>
    /// <param name="egyptian">true for Egyptian bounds, false for Ptolemy bounds.</param>
    /// <returns>Planet index (2–6), or -1 if longitude is out of bounds.</returns>
    public static int IndexBound(double longitude, bool egyptian)
    {
        if (longitude < 0.0 || longitude >= 360.0) return -1;
        var terms = egyptian ? EgyptianTerms : PtolemyTerms;
        foreach (var term in terms)
        {
            if ((double)term.Degrees > longitude)
                return term.Planet;
        }
        return terms[^1].Planet;
    }

    private static readonly TermPosition[] EgyptianTerms =
    [
        // Aries
        new(6, 5), new(12, 3), new(20, 2), new(25, 4), new(30, 6),
        // Taurus
        new(38, 3), new(44, 2), new(52, 5), new(57, 6), new(60, 4),
        // Gemini
        new(66, 2), new(72, 5), new(77, 3), new(84, 4), new(90, 6),
        // Cancer
        new(97, 4), new(103, 3), new(109, 2), new(116, 5), new(120, 6),
        // Leo
        new(126, 5), new(131, 3), new(138, 6), new(144, 2), new(150, 4),
        // Virgo
        new(157, 2), new(167, 3), new(171, 5), new(178, 4), new(180, 6),
        // Libra
        new(186, 6), new(194, 2), new(201, 5), new(208, 3), new(210, 4),
        // Scorpio
        new(217, 4), new(221, 3), new(229, 2), new(234, 5), new(240, 6),
        // Sagittarius
        new(252, 5), new(257, 3), new(261, 2), new(266, 6), new(270, 4),
        // Capricorn
        new(277, 2), new(284, 5), new(292, 3), new(296, 6), new(300, 4),
        // Aquarius
        new(307, 2), new(313, 3), new(320, 5), new(325, 4), new(330, 6),
        // Pisces
        new(342, 3), new(346, 5), new(349, 2), new(358, 4), new(360, 6),
    ];

    private static readonly TermPosition[] PtolemyTerms =
    [
        // Aries
        new(6, 5), new(14, 3), new(21, 2), new(26, 4), new(30, 6),
        // Taurus
        new(38, 3), new(45, 2), new(52, 5), new(54, 6), new(60, 4),
        // Gemini
        new(67, 2), new(73, 5), new(80, 3), new(86, 4), new(90, 6),
        // Cancer
        new(96, 4), new(103, 5), new(110, 2), new(117, 3), new(120, 6),
        // Leo
        new(126, 5), new(133, 2), new(139, 6), new(145, 3), new(150, 4),
        // Virgo
        new(157, 2), new(163, 3), new(168, 5), new(174, 6), new(180, 4),
        // Libra
        new(186, 6), new(191, 3), new(196, 2), new(204, 5), new(210, 4),
        // Scorpio
        new(216, 4), new(223, 3), new(231, 5), new(237, 2), new(240, 6),
        // Sagittarius
        new(248, 5), new(254, 3), new(259, 2), new(265, 6), new(270, 4),
        // Capricorn
        new(276, 3), new(282, 2), new(289, 5), new(295, 6), new(300, 4),
        // Aquarius
        new(306, 6), new(312, 2), new(320, 3), new(325, 5), new(330, 4),
        // Pisces
        new(338, 3), new(344, 5), new(350, 2), new(355, 4), new(360, 6),
    ];
}
