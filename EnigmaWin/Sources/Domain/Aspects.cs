namespace EnigmaWin.Sources.Domain;

public enum Aspects
{
    Conjunction = 0,
    Opposition = 1,
    Trine = 2,
    Square = 3,
    Septile = 4,
    Sextile = 5,
    Quintile = 6,
    Semisextile = 7,
    Semisquare = 8,
    Semiquintile = 9,
    Biquintile = 10,
    Inconjunct = 11,
    Sesquiquadrate = 12,
    Tridecile = 13,
    Biseptile = 14,
    Triseptile = 15,
    Novile = 16,
    Binovile = 17,
    Quadranovile = 18,
    Undecile = 19,
    Centile = 20,
    Vigintile = 21
}

/// <summary>Extension methods for the Aspects enum.</summary>
public static class AspectsExtensions
{
    /// <summary>Localized name key for this aspect.</summary>
    public static string LocalizedName(this Aspects aspect) => aspect switch
    {
        Aspects.Conjunction    => "enum.aspects.conjunction",
        Aspects.Opposition     => "enum.aspects.oppposition",   // existing key has triple-p typo
        Aspects.Trine          => "enum.aspects.trine",
        Aspects.Square         => "enum.aspects.square",
        Aspects.Septile        => "enum.aspects.septile",
        Aspects.Sextile        => "enum.aspects.sextile",
        Aspects.Quintile       => "enum.aspects.quintile",
        Aspects.Semisextile    => "enum.aspects.semisextile",
        Aspects.Semisquare     => "enum.aspects.semisquare",
        Aspects.Semiquintile   => "enum.aspects.semiquintile",
        Aspects.Biquintile     => "enum.aspects.biquintile",
        Aspects.Inconjunct     => "enum.aspects.inconjunct",
        Aspects.Sesquiquadrate => "enum.aspects.sesquiquadrate",
        Aspects.Tridecile      => "enum.aspects.tridecile",
        Aspects.Biseptile      => "enum.aspects.biseptile",
        Aspects.Triseptile     => "enum.aspects.triseptile",
        Aspects.Novile         => "enum.aspects.novile",
        Aspects.Binovile       => "enum.aspects.binovile",
        Aspects.Quadranovile   => "enum.aspects.quadranovile",
        Aspects.Undecile       => "enum.aspects.undecile",
        Aspects.Centile        => "enum.aspects.centile",
        Aspects.Vigintile      => "enum.aspects.vigintile",
        _                      => string.Empty
    };
}
