// ConfigGlyphsSectionViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Data.UserConfiguration;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Config.UI;

public sealed partial class ConfigGlyphsSectionViewModel : ObservableObject
{
    private readonly IUserConfigurationRepository _repo;
    private readonly INavigationService _nav;
    private readonly IConfigContext _configContext;
    private readonly IRosetta _rosetta;

    // ── Row collections (only items with >1 candidate) ──────────────────────

    public IReadOnlyList<GlyphRowViewModel> SignRows   { get; }
    public IReadOnlyList<GlyphRowViewModel> FactorRows { get; }
    public IReadOnlyList<GlyphRowViewModel> AspectRows { get; }

    public bool HasSignChoices   => SignRows.Count   > 0;
    public bool HasFactorChoices => FactorRows.Count > 0;
    public bool HasAspectChoices => AspectRows.Count > 0;

    // ── Dirty tracking ───────────────────────────────────────────────────────

    private bool _isDirty;
    public  bool  IsDirty => _isDirty;

    // ── Labels ───────────────────────────────────────────────────────────────

    public string SectionTitle        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.section.glyphs");
    public string LabelBackToOverview => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.backtoverview");
    public string LabelSave           => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.edit.save");
    public string LabelCancel         => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.cancel");
    public string LabelSectionSigns   => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.glyphs.section.signs");
    public string LabelSectionFactors => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.glyphs.section.factors");
    public string LabelSectionAspects => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.glyphs.section.aspects");
    public string LabelRestoreDefaults => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.glyphs.restoredefaults");
    public string LabelHelpTooltip    => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.glyphs.help.tooltip");
    public string LabelHelpTitle      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.glyphs.help.groupbox");
    public string LabelHelpClose      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.help.close");
    public string LabelHelpLine1      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.glyphs.help.line1");
    public string LabelHelpLine2      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.glyphs.help.line2");
    public string LabelHelpLine3      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.glyphs.help.line3");

    // ── Constructor ──────────────────────────────────────────────────────────

    public ConfigGlyphsSectionViewModel(
        IUserConfigurationRepository repo,
        INavigationService nav,
        IConfigContext configContext,
        IRosetta rosetta)
    {
        _repo = repo;
        _nav = nav;
        _configContext = configContext;
        _rosetta = rosetta;

        var config = configContext.EditingConfig;
        var glyphs = config?.GlyphsConfig ?? GlyphsConfig.Default;

        SignRows   = BuildSignRows(glyphs, rosetta);
        FactorRows = BuildFactorRows(glyphs, rosetta);
        AspectRows = BuildAspectRows(glyphs, rosetta);

        foreach (var row in SignRows.Concat(FactorRows).Concat(AspectRows))
            row.GlyphChanged += OnAnyGlyphChanged;
    }

    private void OnAnyGlyphChanged(object? sender, EventArgs e)
    {
        _isDirty = true;
        OnPropertyChanged(nameof(IsDirty));
    }

    // ── Build helpers ─────────────────────────────────────────────────────────

    private IReadOnlyList<GlyphRowViewModel> BuildSignRows(GlyphsConfig glyphs, IRosetta rosetta)
    {
        var defaults = GlyphsConfig.DefaultSignGlyphs.ToDictionary(g => g.Sign, g => g.Glyph);
        var current  = glyphs.SignGlyphs.ToDictionary(g => g.Sign, g => g.Glyph);

        return Enum.GetValues<Signs>()
            .Where(s => GlyphCandidates.ForSign(s).Count > 1)
            .Select(s => new GlyphRowViewModel(
                itemName:       rosetta.GetText(RbFile.Localizable, s.LocalizedName()),
                candidateGlyphs: GlyphCandidates.ForSign(s),
                currentGlyph:   current.GetValueOrDefault(s, defaults[s]),
                defaultGlyph:   defaults[s],
                sign: s))
            .ToList();
    }

    private IReadOnlyList<GlyphRowViewModel> BuildFactorRows(GlyphsConfig glyphs, IRosetta rosetta)
    {
        var defaults = GlyphsConfig.DefaultFactorGlyphs.ToDictionary(g => g.Factor, g => g.Glyph);
        var current  = glyphs.FactorGlyphs.ToDictionary(g => g.Factor, g => g.Glyph);

        return Enum.GetValues<Factors>()
            .Where(f => GlyphCandidates.ForFactor(f).Count > 1)
            .Where(f => defaults.ContainsKey(f))
            .Select(f => new GlyphRowViewModel(
                itemName:       rosetta.GetText(RbFile.Localizable, f.LocalizedName()),
                candidateGlyphs: GlyphCandidates.ForFactor(f),
                currentGlyph:   current.GetValueOrDefault(f, defaults[f]),
                defaultGlyph:   defaults[f],
                factor: f))
            .ToList();
    }

    private IReadOnlyList<GlyphRowViewModel> BuildAspectRows(GlyphsConfig glyphs, IRosetta rosetta)
    {
        var defaults = GlyphsConfig.DefaultAspectGlyphs.ToDictionary(g => g.Aspect, g => g.Glyph);
        var current  = glyphs.AspectGlyphs.ToDictionary(g => g.Aspect, g => g.Glyph);

        return Enum.GetValues<Aspects>()
            .Where(a => GlyphCandidates.ForAspect(a).Count > 1)
            .Where(a => defaults.ContainsKey(a))
            .Select(a => new GlyphRowViewModel(
                itemName:       rosetta.GetText(RbFile.Localizable, a.LocalizedName()),
                candidateGlyphs: GlyphCandidates.ForAspect(a),
                currentGlyph:   current.GetValueOrDefault(a, defaults[a]),
                defaultGlyph:   defaults[a],
                aspect: a))
            .ToList();
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    internal async Task SaveAsync()
    {
        var config = _configContext.EditingConfig;
        if (config is null) return;

        var existing = config.GlyphsConfig;

        // Sign glyphs: merge displayed rows back into the full list
        var signRowMap    = SignRows.Where(r => r.Sign.HasValue).ToDictionary(r => r.Sign!.Value, r => r.CurrentGlyph);
        var factorRowMap  = FactorRows.Where(r => r.Factor.HasValue).ToDictionary(r => r.Factor!.Value, r => r.CurrentGlyph);
        var aspectRowMap  = AspectRows.Where(r => r.Aspect.HasValue).ToDictionary(r => r.Aspect!.Value, r => r.CurrentGlyph);

        var defaultSignGlyphs   = GlyphsConfig.DefaultSignGlyphs.ToDictionary(g => g.Sign,   g => g.Glyph);
        var defaultFactorGlyphs = GlyphsConfig.DefaultFactorGlyphs.ToDictionary(g => g.Factor, g => g.Glyph);
        var defaultAspectGlyphs = GlyphsConfig.DefaultAspectGlyphs.ToDictionary(g => g.Aspect, g => g.Glyph);

        var existingSignMap   = existing.SignGlyphs.ToDictionary(g => g.Sign,   g => g.Glyph);
        var existingFactorMap = existing.FactorGlyphs.ToDictionary(g => g.Factor, g => g.Glyph);
        var existingAspectMap = existing.AspectGlyphs.ToDictionary(g => g.Aspect, g => g.Glyph);

        var newSignGlyphs = Enum.GetValues<Signs>()
            .Select(s => new SignGlyphSetting(s,
                signRowMap.TryGetValue(s, out var g) ? g :
                existingSignMap.GetValueOrDefault(s, defaultSignGlyphs.GetValueOrDefault(s, "?"))))
            .ToList();

        var newFactorGlyphs = Enum.GetValues<Factors>()
            .Where(f => defaultFactorGlyphs.ContainsKey(f))
            .Select(f => new FactorGlyphSetting(f,
                factorRowMap.TryGetValue(f, out var g) ? g :
                existingFactorMap.GetValueOrDefault(f, defaultFactorGlyphs[f])))
            .ToList();

        var newAspectGlyphs = Enum.GetValues<Aspects>()
            .Where(a => defaultAspectGlyphs.ContainsKey(a))
            .Select(a => new AspectGlyphSetting(a,
                aspectRowMap.TryGetValue(a, out var g) ? g :
                existingAspectMap.GetValueOrDefault(a, defaultAspectGlyphs[a])))
            .ToList();

        config.GlyphsConfig = new GlyphsConfig(newSignGlyphs, newFactorGlyphs, newAspectGlyphs);
        await _repo.UpdateAsync(config);
        if (config.Id == _configContext.ActiveConfig.Id)
            _configContext.ActiveConfig = config;
        _isDirty = false;
        OnPropertyChanged(nameof(IsDirty));
    }

    internal void RestoreDefaults()
    {
        foreach (var row in SignRows.Concat(FactorRows).Concat(AspectRows))
            row.ResetToDefault();
    }

    internal void GoBack()
    {
        if (_nav.CanGoBackDetail)
            _nav.GoBackDetail();
        else
            _nav.NavigateDetail(AppRoutes.ConfigEdit);
    }
}
