// ConfigSymbolicDirectionsSectionViewModel.cs
// EnigmaWin
// Created by Jan Kampherbeek on 23-03-2026

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

public sealed partial class ConfigSymbolicDirectionsSectionViewModel : ObservableObject
{
    private readonly IUserConfigurationRepository _repo;
    private readonly INavigationService           _nav;
    private readonly IConfigContext               _configContext;
    private readonly IRosetta                     _rosetta;

    private bool _rowsDirty;

    // ── Picker ───────────────────────────────────────────────────────────────

    public IReadOnlyList<string> TimeKeyNames { get; }

    private int _origTimeKeyIndex;
    private int _origOrbDeg, _origOrbMin;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _selectedTimeKeyIndex;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _orbDeg;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _orbMin;

    public bool IsDirty =>
        _rowsDirty                              ||
        SelectedTimeKeyIndex != _origTimeKeyIndex ||
        (int)(OrbDeg ?? 0)   != _origOrbDeg      ||
        (int)(OrbMin ?? 0)   != _origOrbMin;

    // ── Factor rows ──────────────────────────────────────────────────────────

    public ObservableCollection<TransitsFactorRowViewModel> FactorRows { get; } = [];

    // ── Labels ───────────────────────────────────────────────────────────────

    public string SectionTitle         => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.nav.symbolic");
    public string LabelBackToOverview  => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.backtoverview");
    public string LabelSave            => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.edit.save");
    public string LabelCancel          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.cancel");
    public string LabelSectionSettings => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.section.settings");
    public string LabelSectionFactors  => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.section.factors");
    public string LabelTimeKey         => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.symbolic.timekey");
    public string LabelOrb             => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.orb");
    public string LabelHelpTooltip     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.symbolic.help.tooltip");
    public string LabelHelpTitle       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.symbolic.help.groupbox");
    public string LabelHelpClose       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.help.close");
    public string LabelHelpLine1       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.symbolic.help.line1");
    public string LabelHelpLine2       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.symbolic.help.line2");

    // ── Constructor ──────────────────────────────────────────────────────────

    public ConfigSymbolicDirectionsSectionViewModel(
        IUserConfigurationRepository repo,
        INavigationService           nav,
        IConfigContext               configContext,
        IRosetta                     rosetta)
    {
        _repo          = repo;
        _nav           = nav;
        _configContext = configContext;
        _rosetta       = rosetta;

        TimeKeyNames = Enum.GetValues<SymbolicTimeKeys>()
            .Select(k => rosetta.GetText(RbFile.Localizable, k.LocalizedName()))
            .ToList();

        LoadFromConfig();
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    internal async Task SaveAsync()
    {
        var config = _configContext.EditingConfig;
        if (config is null) return;

        var newSymbolic = new SymbolicDirectionsConfig(
            FactorRows.Where(r => r.IsSelected).Select(r => r.Factor).ToList(),
            FromSexagesimal(OrbDeg, OrbMin),
            Enum.GetValues<SymbolicTimeKeys>()[SelectedTimeKeyIndex]);

        config.ProgressionsConfig = config.ProgressionsConfig with { SymbolicDirections = newSymbolic };
        await _repo.UpdateAsync(config);

        _origTimeKeyIndex = SelectedTimeKeyIndex;
        _origOrbDeg       = (int)(OrbDeg ?? 0);
        _origOrbMin       = (int)(OrbMin ?? 0);
        _rowsDirty        = false;
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

    // ── Helpers ──────────────────────────────────────────────────────────────

    private SymbolicDirectionsConfig CurrentConfig() =>
        _configContext.EditingConfig?.ProgressionsConfig.SymbolicDirections
        ?? SymbolicDirectionsConfig.Default;

    private void LoadFromConfig()
    {
        var cfg      = CurrentConfig();
        var timeKeys = Enum.GetValues<SymbolicTimeKeys>();

        _selectedTimeKeyIndex = Math.Max(0, Array.IndexOf(timeKeys, cfg.TimeKey));

        var (od, om) = ToSexagesimal(cfg.Orb);
        _orbDeg = od;
        _orbMin = om;
        _origTimeKeyIndex = _selectedTimeKeyIndex;
        _origOrbDeg       = (int)od;
        _origOrbMin       = (int)om;

        FactorRows.Clear();
        foreach (var factor in Enum.GetValues<Factors>())
        {
            var name       = _rosetta.GetText(RbFile.Localizable, factor.LocalizedName());
            var isSelected = cfg.Factors.Contains(factor);
            FactorRows.Add(new TransitsFactorRowViewModel(factor, name, isSelected, OnRowChanged));
        }
        _rowsDirty = false;
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
