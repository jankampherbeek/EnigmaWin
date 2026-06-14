// PresentableVspPosition.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.VSP;

/// <summary>Venus Star Point position ready for display in the UI.</summary>
public sealed class PresentableVspPosition
{
    public int    SequenceId     { get; }
    public string Name           { get; }
    public string PhenomenonText { get; }
    public string DateTimeText   { get; }
    public string LongitudeText  { get; }
    public string SignGlyph      { get; }

    // Raw values used for wheel drawing
    public double         Longitude  { get; }
    public double         Jd         { get; }
    public VenusPhenomena Phenomenon { get; }
    public bool           IsEvenRow  { get; }

    public PresentableVspPosition(VspPosition position)
    {
        SequenceId  = position.SequenceId;
        Name        = GetName(position.SequenceId);
        Longitude   = position.Longitude;
        Jd          = position.Jd;
        Phenomenon  = position.Phenomenon;
        IsEvenRow   = position.SequenceId % 2 == 0;

        var dt = SEWrapper.DateFromJulianDay(position.Jd, true);
        DateTimeText   = $"{dt.Date.Year:D4}/{dt.Date.Month:D2}/{dt.Date.Day:D2} " +
                         $"{dt.Time.Hour:D2}:{dt.Time.Minute:D2}:{dt.Time.Second:D2}";
        PhenomenonText = position.Phenomenon == VenusPhenomena.InferiorConjunction ? "Inferior" : "Superior";

        var signIdx = (int)(((position.Longitude % 360 + 360) % 360) / 30.0);
        var sign    = (Signs)(signIdx + 1); // Signs enum starts at 1
        SignGlyph   = SignGlyphs.Glyph(sign);

        var inSign = position.Longitude % 30.0;
        var deg    = (int)inSign;
        var minD   = (inSign - deg) * 60.0;
        var min    = (int)minD;
        var sec    = (int)((minD - min) * 60.0);
        LongitudeText = $"{deg}°{min:D2}'{sec:D2}\"";
    }

    private static string GetName(int id) => id switch
    {
        1 => "Leg",
        2 => "Arm",
        3 => "Head",
        4 => "Arm",
        5 => "Leg",
        _ => "?"
    };
}
