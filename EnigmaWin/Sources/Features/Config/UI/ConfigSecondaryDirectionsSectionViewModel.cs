// ConfigSecondaryDirectionsSectionViewModel.cs
// EnigmaWin
// Created by Jan Kampherbeek on 23-03-2026

using System;
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

public sealed partial class ConfigSecondaryDirectionsSectionViewModel : ObservableObject
{
    private readonly IUserConfigurationRepository _repo;
    private readonly INavigationService           _nav;
    private readonly IConfigContext               _configContext;
    private readonly IRosetta                     _rosetta;

    private bool _rowsDirty;

    // ── Orb ──────────────────────────────────────────────────────────────────

    private int _origOrbDeg, _origOrbMin;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _orbDeg;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _orbMin;

    public bool IsDirty =>
        _rowsDirty                         ||
        (int)(OrbDeg ?? 0) != _origOrbDeg  ||
        (int)(OrbMin ?? 0) != _origOrbMin;

    // ── Factor rows ──────────────────────────────────────────────────────────

    public ObservableCollection<TransitsFactorRowViewModel> FactorRows { get; } = [];

    // ── Labels ───────────────────────────────────────────────────────────────

    public string SectionTitle        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.nav.secondary");
    public string LabelBackToOverview => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.backtoverview");
    public string LabelSave           => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.edit.save");
    public string LabelCancel         => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.cancel");
    public string LabelSectionOrb     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.section.orb");
    public string LabelSectionFactors => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.section.factors");
    public string LabelOrb            => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.orb");
    public string LabelHelpTooltip    => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.secondary.help.tooltip");
    public string LabelHelpTitle      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.secondary.help.groupbox");
    public string LabelHelpClose      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.help.close");
    public string LabelHelpLine1      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.secondary.help.line1");
    public string LabelHelpLine2      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.secondary.help.line2");

    // ── Constructor ──────────────────────────────────────────────────────────

    public ConfigSecondaryDirectionsSectionViewModel(
        IUserConfigurationRepository repo,
        INavigationService           nav,
        IConfigContext               configContext,
        IRosetta                     rosetta)
    {
        _repo          = repo;
        _nav           = nav;
        _configContext = configContext;
        _rosetta       = rosetta;

        LoadFromConfig();
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    internal async Task SaveAsync()
    {
        var config = _configContext.EditingConfig;
        if (config is null) return;

        var newSecondary = new SecondaryDirectionsConfig(
            FactorRows.Where(r => r.IsSelected).Select(r => r.Factor).ToList(),
            FromSexagesimal(OrbDeg, OrbMin));

        config.ProgressionsConfig = config.ProgressionsConfig with { SecondaryDirections = newSecondary };
        await _repo.UpdateAsync(config);

        _origOrbDeg = (int)(OrbDeg ?? 0);
        _origOrbMin = (int)(OrbMin ?? 0);
        _rowsDirty  = false;
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

    private SecondaryDirectionsConfig CurrentConfig() =>
        _configContext.EditingConfig?.ProgressionsConfig.SecondaryDirections
        ?? SecondaryDirectionsConfig.Default;

    private void LoadFromConfig()
    {
        var cfg = CurrentConfig();
        var (od, om) = ToSexagesimal(cfg.Orb);
        _orbDeg = od;
        _orbMin = om;
        _origOrbDeg = (int)od;
        _origOrbMin = (int)om;

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
