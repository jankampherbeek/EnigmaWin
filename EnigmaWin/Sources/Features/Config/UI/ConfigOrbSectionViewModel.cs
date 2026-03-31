// ConfigOrbSectionViewModel.cs
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
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Config.UI;

public sealed partial class ConfigOrbSectionViewModel : ObservableObject
{
    private readonly IUserConfigurationRepository _repo;
    private readonly INavigationService           _nav;
    private readonly IConfigContext               _configContext;
    private readonly IRosetta                     _rosetta;

    // ── Picker ───────────────────────────────────────────────────────────────

    public IReadOnlyList<string> OrbSystemNames { get; }

    private int _origOrbSystemIndex;
    private int _origAspectDeg,      _origAspectMin;
    private int _origMidpoint360Deg, _origMidpoint360Min;
    private int _origMidpoint90Deg,  _origMidpoint90Min;
    private int _origMidpoint45Deg,  _origMidpoint45Min;
    private int _origHarmonicDeg,    _origHarmonicMin;
    private int _origParallelDeg,    _origParallelMin;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _selectedOrbSystemIndex;

    // decimal? so NumericUpDown compiled binding works
    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _aspectBaseDeg;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _aspectBaseMin;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _midpoint360Deg;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _midpoint360Min;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _midpoint90Deg;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _midpoint90Min;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _midpoint45Deg;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _midpoint45Min;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _harmonicDeg;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _harmonicMin;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _parallelDeg;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _parallelMin;

    public bool IsDirty =>
        SelectedOrbSystemIndex       != _origOrbSystemIndex          ||
        (int)(AspectBaseDeg  ?? 0)   != _origAspectDeg               ||
        (int)(AspectBaseMin  ?? 0)   != _origAspectMin               ||
        (int)(Midpoint360Deg ?? 0)   != _origMidpoint360Deg          ||
        (int)(Midpoint360Min ?? 0)   != _origMidpoint360Min          ||
        (int)(Midpoint90Deg  ?? 0)   != _origMidpoint90Deg           ||
        (int)(Midpoint90Min  ?? 0)   != _origMidpoint90Min           ||
        (int)(Midpoint45Deg  ?? 0)   != _origMidpoint45Deg           ||
        (int)(Midpoint45Min  ?? 0)   != _origMidpoint45Min           ||
        (int)(HarmonicDeg    ?? 0)   != _origHarmonicDeg             ||
        (int)(HarmonicMin    ?? 0)   != _origHarmonicMin             ||
        (int)(ParallelDeg    ?? 0)   != _origParallelDeg             ||
        (int)(ParallelMin    ?? 0)   != _origParallelMin;

    // ── Labels ───────────────────────────────────────────────────────────────

    public string SectionTitle          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.section.orbs");
    public string LabelBackToOverview   => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.backtoverview");
    public string LabelSave             => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.edit.save");
    public string LabelCancel           => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.cancel");
    public string LabelSectionSystem    => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.orb.section.system");
    public string LabelSectionValues    => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.orb.section.values");
    public string LabelOrbSystem        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.orb.system");
    public string LabelAspectBase       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.orb.aspectbase");
    public string LabelMidpoint360      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.orb.midpoint360");
    public string LabelMidpoint90       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.orb.midpoint90");
    public string LabelMidpoint45       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.orb.midpoint45");
    public string LabelHarmonic         => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.orb.harmonic");
    public string LabelParallel         => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.orb.parallel");
    public string LabelHelpTooltip      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.orb.help.tooltip");
    public string LabelHelpTitle        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.orb.help.groupbox");
    public string LabelHelpClose        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.help.close");
    public string LabelHelpLine1        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.orb.help.line1");
    public string LabelHelpLine2        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.orb.help.line2");
    public string LabelHelpLine3        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.orb.help.line3");

    // ── Constructor ──────────────────────────────────────────────────────────

    public ConfigOrbSectionViewModel(
        IUserConfigurationRepository repo,
        INavigationService           nav,
        IConfigContext               configContext,
        IRosetta                     rosetta)
    {
        _repo          = repo;
        _nav           = nav;
        _configContext = configContext;
        _rosetta       = rosetta;

        OrbSystemNames = Enum.GetValues<OrbSystems>()
            .Select(s => rosetta.GetText(RbFile.Localizable, s.LocalizedName()))
            .ToList();

        var orb     = configContext.EditingConfig?.OrbConfig ?? OrbConfig.Default;
        var orbVals = Enum.GetValues<OrbSystems>();
        _selectedOrbSystemIndex = Math.Max(0, Array.IndexOf(orbVals, orb.OrbSystem));

        var (ad,   am)  = ToSexagesimal(orb.AspectBaseOrb);
        var (m3d,  m3m) = ToSexagesimal(orb.Midpoint360DialOrb);
        var (m9d,  m9m) = ToSexagesimal(orb.Midpoint90DialOrb);
        var (m4d,  m4m) = ToSexagesimal(orb.Midpoint45DialOrb);
        var (hd,   hm)  = ToSexagesimal(orb.HarmonicOrb);
        var (pd,   pm)  = ToSexagesimal(orb.ParallelOrb);

        _aspectBaseDeg  = ad;  _aspectBaseMin  = am;
        _midpoint360Deg = m3d; _midpoint360Min = m3m;
        _midpoint90Deg  = m9d; _midpoint90Min  = m9m;
        _midpoint45Deg  = m4d; _midpoint45Min  = m4m;
        _harmonicDeg    = hd;  _harmonicMin    = hm;
        _parallelDeg    = pd;  _parallelMin    = pm;

        SaveOriginals();
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    internal async Task SaveAsync()
    {
        var config = _configContext.EditingConfig;
        if (config is null) return;

        config.OrbConfig = new OrbConfig(
            Enum.GetValues<OrbSystems>()[SelectedOrbSystemIndex],
            FromSexagesimal(AspectBaseDeg,  AspectBaseMin),
            FromSexagesimal(Midpoint360Deg, Midpoint360Min),
            FromSexagesimal(Midpoint90Deg,  Midpoint90Min),
            FromSexagesimal(Midpoint45Deg,  Midpoint45Min),
            FromSexagesimal(HarmonicDeg,    HarmonicMin),
            FromSexagesimal(ParallelDeg,    ParallelMin));

        await _repo.UpdateAsync(config);
        if (config.Id == _configContext.ActiveConfig.Id)
            _configContext.ActiveConfig = config;
        SaveOriginals();
        OnPropertyChanged(nameof(IsDirty));
    }

    internal void Revert()
    {
        SelectedOrbSystemIndex = _origOrbSystemIndex;
        AspectBaseDeg  = _origAspectDeg;      AspectBaseMin  = _origAspectMin;
        Midpoint360Deg = _origMidpoint360Deg; Midpoint360Min = _origMidpoint360Min;
        Midpoint90Deg  = _origMidpoint90Deg;  Midpoint90Min  = _origMidpoint90Min;
        Midpoint45Deg  = _origMidpoint45Deg;  Midpoint45Min  = _origMidpoint45Min;
        HarmonicDeg    = _origHarmonicDeg;    HarmonicMin    = _origHarmonicMin;
        ParallelDeg    = _origParallelDeg;    ParallelMin    = _origParallelMin;
    }

    internal void GoBack()
    {
        if (_nav.CanGoBackDetail)
            _nav.GoBackDetail();
        else
            _nav.NavigateDetail(AppRoutes.ConfigEdit);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void SaveOriginals()
    {
        _origOrbSystemIndex  = SelectedOrbSystemIndex;
        _origAspectDeg       = (int)(AspectBaseDeg  ?? 0); _origAspectMin       = (int)(AspectBaseMin  ?? 0);
        _origMidpoint360Deg  = (int)(Midpoint360Deg ?? 0); _origMidpoint360Min  = (int)(Midpoint360Min ?? 0);
        _origMidpoint90Deg   = (int)(Midpoint90Deg  ?? 0); _origMidpoint90Min   = (int)(Midpoint90Min  ?? 0);
        _origMidpoint45Deg   = (int)(Midpoint45Deg  ?? 0); _origMidpoint45Min   = (int)(Midpoint45Min  ?? 0);
        _origHarmonicDeg     = (int)(HarmonicDeg    ?? 0); _origHarmonicMin     = (int)(HarmonicMin    ?? 0);
        _origParallelDeg     = (int)(ParallelDeg    ?? 0); _origParallelMin     = (int)(ParallelMin    ?? 0);
    }

    private static (decimal deg, decimal min) ToSexagesimal(double value)
    {
        int deg = (int)value;
        int min = (int)Math.Round((value - deg) * 60);
        if (min == 60) { deg++; min = 0; }
        return (deg, min);
    }

    private static double FromSexagesimal(decimal? deg, decimal? min) =>
        (double)(deg ?? 0) + (double)(min ?? 0) / 60.0;
}
