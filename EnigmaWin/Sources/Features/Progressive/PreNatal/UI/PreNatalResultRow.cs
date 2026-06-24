// PreNatalResultRow.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Progressive.PreNatal.UI;

public sealed class PreNatalResultRow
{
    public string ActualDateText   { get; }
    public string TypeText         { get; }
    public string TypeGlyphs       { get; }
    public bool   TypeIsGlyph      { get; }
    public string Factor1Glyph    { get; }
    public string Longitude1Text  { get; }
    public string Sign1Glyph      { get; }
    public string Factor2Glyph    { get; }
    public string Longitude2Text  { get; }
    public string Sign2Glyph      { get; }
    public string PrenatalDateText { get; }
    public bool   IsOddRow         { get; }
    public double ActualJD         { get; }

    public PreNatalResultRow(PreNatalMoment moment, int index, IRosetta rosetta)
    {
        ActualDateText   = moment.ActualDateTxt;
        PrenatalDateText = moment.PrenatalDateTxt;
        IsOddRow         = index % 2 != 0;
        ActualJD         = moment.ActualJD;

        var (dms1, sign1, ok1) = PositionInDegreesConversion.DoubleToDmsSign(moment.Longitude1);
        Longitude1Text = ok1 ? dms1 : PositionInDegreesConversion.DoubleToDms(moment.Longitude1);
        Sign1Glyph     = (ok1 && sign1.HasValue) ? GlyphSelector.GetGlyphForSign(sign1.Value) : "";

        switch (moment.Kind)
        {
            case SolarEclipseKind:
                TypeIsGlyph  = false;
                TypeText     = rosetta.GetText(RbFile.PreNatal, "view.prenatal.type.solareclipse");
                TypeGlyphs   = "";
                Factor1Glyph = GlyphSelector.GetGlyphForFactor(Factors.Sun);
                Factor2Glyph = "";
                Longitude2Text = "";
                Sign2Glyph   = "";
                break;

            case LunarEclipseKind:
                TypeIsGlyph  = false;
                TypeText     = rosetta.GetText(RbFile.PreNatal, "view.prenatal.type.lunareclipse");
                TypeGlyphs   = "";
                Factor1Glyph = GlyphSelector.GetGlyphForFactor(Factors.Sun);
                Factor2Glyph = "";
                Longitude2Text = "";
                Sign2Glyph   = "";
                break;

            case IngressKind ik:
                TypeIsGlyph  = false;
                TypeText     = rosetta.GetText(RbFile.PreNatal, "view.prenatal.type.ingress");
                TypeGlyphs   = "";
                Factor1Glyph = GlyphSelector.GetGlyphForFactor(ik.Factor);
                Factor2Glyph = "";
                Longitude2Text = "";
                Sign2Glyph   = "";
                break;

            case RetrogradeKind rk:
                TypeIsGlyph  = false;
                TypeText     = rosetta.GetText(RbFile.PreNatal, "view.prenatal.type.retrograde");
                TypeGlyphs   = "";
                Factor1Glyph = GlyphSelector.GetGlyphForFactor(rk.Factor);
                Factor2Glyph = "";
                Longitude2Text = "";
                Sign2Glyph   = "";
                break;

            case DirectKind dk:
                TypeIsGlyph  = false;
                TypeText     = rosetta.GetText(RbFile.PreNatal, "view.prenatal.type.direct");
                TypeGlyphs   = "";
                Factor1Glyph = GlyphSelector.GetGlyphForFactor(dk.Factor);
                Factor2Glyph = "";
                Longitude2Text = "";
                Sign2Glyph   = "";
                break;

            case AspectKind ak:
                TypeIsGlyph  = true;
                TypeText     = "";
                TypeGlyphs   = GlyphSelector.GetGlyphForFactor(ak.Factor1)
                             + GlyphSelector.GetGlyphForAspect(ak.AspectType)
                             + GlyphSelector.GetGlyphForFactor(ak.Factor2);
                Factor1Glyph = GlyphSelector.GetGlyphForFactor(ak.Factor1);
                Factor2Glyph = GlyphSelector.GetGlyphForFactor(ak.Factor2);
                if (moment.Longitude2.HasValue)
                {
                    var (dms2, sign2, ok2) = PositionInDegreesConversion.DoubleToDmsSign(moment.Longitude2.Value);
                    Longitude2Text = ok2 ? dms2 : PositionInDegreesConversion.DoubleToDms(moment.Longitude2.Value);
                    Sign2Glyph     = (ok2 && sign2.HasValue) ? GlyphSelector.GetGlyphForSign(sign2.Value) : "";
                }
                else
                {
                    Longitude2Text = "";
                    Sign2Glyph     = "";
                }
                break;

            default:
                TypeIsGlyph    = false;
                TypeText       = "";
                TypeGlyphs     = "";
                Factor1Glyph   = "";
                Factor2Glyph   = "";
                Longitude2Text = "";
                Sign2Glyph     = "";
                break;
        }
    }
}
