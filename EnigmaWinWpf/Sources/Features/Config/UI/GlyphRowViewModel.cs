// GlyphRowViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Config.UI;

// ── Candidate ────────────────────────────────────────────────────────────────

/// <summary>A single glyph candidate button in the glyph picker grid.</summary>
public sealed partial class GlyphCandidateViewModel : ObservableObject
{
    public string Glyph { get; }

    [ObservableProperty] private bool _isSelected;

    public IRelayCommand SelectCommand { get; }

    public GlyphCandidateViewModel(string glyph, bool isSelected, Action<string> onSelect)
    {
        Glyph       = glyph;
        _isSelected = isSelected;
        SelectCommand = new RelayCommand(() => onSelect(Glyph));
    }
}

// ── Row ──────────────────────────────────────────────────────────────────────

/// <summary>
/// One row in the glyph editor — item name + selectable glyph candidates.
/// The optional Sign/Factor/Aspect is used by the ViewModel when building
/// the updated GlyphsConfig on save.
/// </summary>
public sealed class GlyphRowViewModel
{
    public Signs?   Sign   { get; }
    public Factors? Factor { get; }
    public Aspects? Aspect { get; }
    public string   ItemName     { get; }
    public string   DefaultGlyph { get; }

    public IReadOnlyList<GlyphCandidateViewModel> Candidates { get; }

    private string _currentGlyph;
    public  string  CurrentGlyph => _currentGlyph;

    public event EventHandler? GlyphChanged;

    public GlyphRowViewModel(
        string               itemName,
        IReadOnlyList<string> candidateGlyphs,
        string               currentGlyph,
        string               defaultGlyph,
        Signs?               sign   = null,
        Factors?             factor = null,
        Aspects?             aspect = null)
    {
        ItemName     = itemName;
        DefaultGlyph = defaultGlyph;
        _currentGlyph = currentGlyph;
        Sign   = sign;
        Factor = factor;
        Aspect = aspect;

        Candidates = candidateGlyphs
            .Select(g => new GlyphCandidateViewModel(g, g == currentGlyph, SelectGlyph))
            .ToList();
    }

    private void SelectGlyph(string glyph)
    {
        _currentGlyph = glyph;
        foreach (var c in Candidates)
            c.IsSelected = c.Glyph == glyph;
        GlyphChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ResetToDefault() => SelectGlyph(DefaultGlyph);
}
