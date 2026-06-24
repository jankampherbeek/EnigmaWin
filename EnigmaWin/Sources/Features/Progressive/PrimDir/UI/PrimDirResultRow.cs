// PrimDirResultRow.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.Progressive.PrimDir.UI;

public sealed class PrimDirResultRow
{
    public string DateText        { get; }
    public string PromissorGlyph  { get; }
    public string AspectGlyph     { get; }
    public string SignificatorGlyph { get; }
    public string OrbText         { get; }
    public bool   IsOddRow        { get; }

    public PrimDirResultRow(PrimDirHit hit, int index)
    {
        DateText           = hit.DateText;
        PromissorGlyph     = GlyphSelector.GetGlyphForFactor(hit.Promissor);
        AspectGlyph        = GlyphSelector.GetGlyphForAspect(hit.Aspect);
        SignificatorGlyph  = GlyphSelector.GetGlyphForFactor(hit.Significator);
        OrbText            = FormatOrb(hit.OrbDays);
        IsOddRow           = index % 2 != 0;
    }

    private static string FormatOrb(double? days)
    {
        if (days is null) return string.Empty;
        var absD = (int)Math.Round(Math.Abs(days.Value));
        if (absD == 0) return "0d";
        return days.Value < 0 ? $"−{absD}d" : $"+{absD}d";
    }
}
