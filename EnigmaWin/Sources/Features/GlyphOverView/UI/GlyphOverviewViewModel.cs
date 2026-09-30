// GlyphOverviewViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.GlyphOverView.UI;

/// <summary>A single glyph with the name of the item it represents.</summary>
public sealed record GlyphOverviewEntry(string Glyph, string Name);

/// <summary>
/// ViewModel for GlyphOverviewWindow: the currently active glyph (standard or user-customized
/// via GlyphsConfig) for every factor, aspect and sign, together with its display name.
/// </summary>
public sealed class GlyphOverviewViewModel
{
    public IReadOnlyList<GlyphOverviewEntry> FactorEntries { get; }
    public IReadOnlyList<GlyphOverviewEntry> AspectEntries { get; }
    public IReadOnlyList<GlyphOverviewEntry> SignEntries   { get; }

    public string Title               { get; }
    public string LabelSectionFactors { get; }
    public string LabelSectionAspects { get; }
    public string LabelSectionSigns   { get; }
    public string LabelClose          { get; }

    public GlyphOverviewViewModel(IRosetta rosetta)
    {
        Title               = rosetta.GetText(RbFile.GlyphOverview, GlyphOverviewKeys.Title);
        LabelSectionFactors = rosetta.GetText(RbFile.GlyphOverview, GlyphOverviewKeys.SectionFactors);
        LabelSectionAspects = rosetta.GetText(RbFile.GlyphOverview, GlyphOverviewKeys.SectionAspects);
        LabelSectionSigns   = rosetta.GetText(RbFile.GlyphOverview, GlyphOverviewKeys.SectionSigns);
        LabelClose          = rosetta.GetText(RbFile.GlyphOverview, GlyphOverviewKeys.Close);

        FactorEntries = FactorsExtensions.SelectableFactors
            .Select(f => new GlyphOverviewEntry(
                GlyphSelector.GetGlyphForFactor(f),
                rosetta.GetText(RbFile.Localizable, f.LocalizedName())))
            .ToList();

        AspectEntries = Enum.GetValues<Aspects>()
            .Select(a => new GlyphOverviewEntry(
                GlyphSelector.GetGlyphForAspect(a),
                rosetta.GetText(RbFile.Localizable, a.LocalizedName())))
            .ToList();

        SignEntries = Enum.GetValues<Signs>()
            .Select(s => new GlyphOverviewEntry(
                GlyphSelector.GetGlyphForSign(s),
                rosetta.GetText(RbFile.Localizable, s.LocalizedName())))
            .ToList();
    }
}
