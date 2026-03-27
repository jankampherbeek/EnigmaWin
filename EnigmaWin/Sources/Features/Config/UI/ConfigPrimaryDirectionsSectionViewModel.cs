// ConfigPrimaryDirectionsSectionViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

public sealed partial class ConfigPrimaryDirectionsSectionViewModel : ObservableObject
{
    private readonly IUserConfigurationRepository _repo;
    private readonly INavigationService           _nav;
    private readonly IConfigContext               _configContext;
    private readonly IRosetta                     _rosetta;

    private bool _rowsDirty;

    // ── Picker lists ─────────────────────────────────────────────────────────

    public IReadOnlyList<string> MethodNames   { get; }
    public IReadOnlyList<string> ApproachNames { get; }
    public IReadOnlyList<string> TimeKeyNames  { get; }

    // ── Picker selection ─────────────────────────────────────────────────────

    private int _origMethodIndex;
    private int _origApproachIndex;
    private int _origTimeKeyIndex;
    private int _origOrbDeg, _origOrbMin;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _selectedMethodIndex;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _selectedApproachIndex;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _selectedTimeKeyIndex;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _orbDeg;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _orbMin;

    public bool IsDirty =>
        _rowsDirty                                         ||
        SelectedMethodIndex   != _origMethodIndex          ||
        SelectedApproachIndex != _origApproachIndex        ||
        SelectedTimeKeyIndex  != _origTimeKeyIndex         ||
        (int)(OrbDeg ?? 0)    != _origOrbDeg               ||
        (int)(OrbMin ?? 0)    != _origOrbMin;

    // ── Factor rows ──────────────────────────────────────────────────────────

    public ObservableCollection<PrimaryDirectionsFactorRowViewModel> FactorRows { get; } = [];

    // ── Labels ───────────────────────────────────────────────────────────────

    public string SectionTitle         => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.nav.primary");
    public string LabelBackToOverview  => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.backtoverview");
    public string LabelSave            => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.edit.save");
    public string LabelCancel          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.cancel");
    public string LabelSectionSettings => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.section.settings");
    public string LabelSectionFactors  => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.section.factors");
    public string LabelMethod          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.primary.method");
    public string LabelApproach        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.primary.approach");
    public string LabelTimeKey         => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.primary.timekey");
    public string LabelOrb             => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.orb");
    public string LabelPromissors      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.primary.promissors");
    public string LabelSignificators   => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.primary.significators");
    public string LabelHelpTooltip     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.primary.help.tooltip");
    public string LabelHelpTitle       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.primary.help.groupbox");
    public string LabelHelpClose       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.help.close");
    public string LabelHelpLine1       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.primary.help.line1");
    public string LabelHelpLine2       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.primary.help.line2");
    public string LabelHelpLine3       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.primary.help.line3");

    // ── Constructor ──────────────────────────────────────────────────────────

    public ConfigPrimaryDirectionsSectionViewModel(
        IUserConfigurationRepository repo,
        INavigationService           nav,
        IConfigContext               configContext,
        IRosetta                     rosetta)
    {
        _repo          = repo;
        _nav           = nav;
        _configContext = configContext;
        _rosetta       = rosetta;

        MethodNames   = Enum.GetValues<PrimaryMethods>()
            .Select(m => rosetta.GetText(RbFile.Localizable, m.LocalizedName())).ToList();
        ApproachNames = Enum.GetValues<PrimaryApproaches>()
            .Select(a => rosetta.GetText(RbFile.Localizable, a.LocalizedName())).ToList();
        TimeKeyNames  = Enum.GetValues<PrimaryTimeKeys>()
            .Select(k => rosetta.GetText(RbFile.Localizable, k.LocalizedName())).ToList();

        LoadFromConfig();
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    internal async Task SaveAsync()
    {
        var config = _configContext.EditingConfig;
        if (config is null) return;

        var primary = config.ProgressionsConfig.PrimaryDirections;
        var newPrimary = primary with
        {
            Method        = Enum.GetValues<PrimaryMethods>()[SelectedMethodIndex],
            Approach      = Enum.GetValues<PrimaryApproaches>()[SelectedApproachIndex],
            TimeKey       = Enum.GetValues<PrimaryTimeKeys>()[SelectedTimeKeyIndex],
            Orb           = FromSexagesimal(OrbDeg, OrbMin),
            Promissors    = FactorRows.Where(r => r.IsPromissor)  .Select(r => r.Factor).ToList(),
            Significators = FactorRows.Where(r => r.IsSignificator).Select(r => r.Factor).ToList()
        };

        config.ProgressionsConfig = config.ProgressionsConfig with { PrimaryDirections = newPrimary };
        await _repo.UpdateAsync(config);

        foreach (var row in FactorRows) row.SaveOriginals();
        SaveOriginals();
        _rowsDirty = false;
        OnPropertyChanged(nameof(IsDirty));
    }

    internal void Revert()
    {
        LoadFromConfig();
        _rowsDirty = false;
        OnPropertyChanged(nameof(IsDirty));
    }

    internal void GoBack()
    {
        if (_nav.CanGoBackDetail)
            _nav.GoBackDetail();
        else
            _nav.NavigateDetail(AppRoutes.ConfigSectionProgressions);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private PrimaryDirectionsConfig CurrentConfig() =>
        _configContext.EditingConfig?.ProgressionsConfig.PrimaryDirections
        ?? PrimaryDirectionsConfig.Default;

    private void LoadFromConfig()
    {
        var cfg = CurrentConfig();

        var methods   = Enum.GetValues<PrimaryMethods>();
        var approaches = Enum.GetValues<PrimaryApproaches>();
        var timeKeys  = Enum.GetValues<PrimaryTimeKeys>();

        _selectedMethodIndex   = Math.Max(0, Array.IndexOf(methods,   cfg.Method));
        _selectedApproachIndex = Math.Max(0, Array.IndexOf(approaches, cfg.Approach));
        _selectedTimeKeyIndex  = Math.Max(0, Array.IndexOf(timeKeys,  cfg.TimeKey));

        var (od, om) = ToSexagesimal(cfg.Orb);
        _orbDeg = od;
        _orbMin = om;

        SaveOriginals();
        LoadFactorRows(cfg);
    }

    private void LoadFactorRows(PrimaryDirectionsConfig cfg)
    {
        FactorRows.Clear();
        foreach (var factor in Enum.GetValues<Factors>())
        {
            var name          = _rosetta.GetText(RbFile.Localizable, factor.LocalizedName());
            var isPromissor   = cfg.Promissors.Contains(factor);
            var isSignificator = cfg.Significators.Contains(factor);
            FactorRows.Add(new PrimaryDirectionsFactorRowViewModel(
                factor, name, isPromissor, isSignificator, OnRowChanged));
        }
    }

    private void SaveOriginals()
    {
        _origMethodIndex   = SelectedMethodIndex;
        _origApproachIndex = SelectedApproachIndex;
        _origTimeKeyIndex  = SelectedTimeKeyIndex;
        _origOrbDeg        = (int)(OrbDeg ?? 0);
        _origOrbMin        = (int)(OrbMin ?? 0);
    }

    private void OnRowChanged()
    {
        _rowsDirty = true;
        OnPropertyChanged(nameof(IsDirty));
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
