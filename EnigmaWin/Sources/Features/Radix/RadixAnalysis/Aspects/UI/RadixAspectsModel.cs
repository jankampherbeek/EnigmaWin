// RadixAspectsModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects.UI;

public sealed class RadixAspectsModel
{
    public sealed record AspectRow(
        string Factor1Glyph,
        string Factor1Name,
        string AspectGlyph,
        string AspectName,
        string Factor2Glyph,
        string Factor2Name,
        string OrbText,
        string ExactnessText);

    public IReadOnlyList<AspectRow> BuildRows(IReadOnlyList<FoundAspect> aspects, IRosetta rosetta)
    {
        var rows = new List<AspectRow>();
        foreach (var aspect in aspects)
        {
            var exactnessPct = aspect.MaxOrb > 0
                ? Math.Max(0, 100 - (int)Math.Round(aspect.Orb / aspect.MaxOrb * 100))
                : 0;

            rows.Add(new AspectRow(
                Factor1Glyph:   GlyphSelector.GetGlyphForFactor(aspect.Factor1),
                Factor1Name:    rosetta.GetText(RbFile.Localizable, aspect.Factor1.LocalizedName()),
                AspectGlyph:    GlyphSelector.GetGlyphForAspect(aspect.Aspect),
                AspectName:     rosetta.GetText(RbFile.Localizable, aspect.Aspect.LocalizedName()),
                Factor2Glyph:   GlyphSelector.GetGlyphForFactor(aspect.Factor2),
                Factor2Name:    rosetta.GetText(RbFile.Localizable, aspect.Factor2.LocalizedName()),
                OrbText:        PositionInDegreesConversion.DoubleToDms(aspect.Orb),
                ExactnessText:  $"{exactnessPct}%"
            ));
        }
        return rows;
    }
}
