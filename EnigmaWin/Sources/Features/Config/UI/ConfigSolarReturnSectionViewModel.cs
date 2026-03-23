// ConfigSolarReturnSectionViewModel.cs
// EnigmaWin
// Created by Jan Kampherbeek on 23-03-2026

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

public sealed partial class ConfigSolarReturnSectionViewModel : ObservableObject
{
    private readonly IUserConfigurationRepository _repo;
    private readonly INavigationService           _nav;
    private readonly IConfigContext               _configContext;
    private readonly IRosetta                     _rosetta;

    // ── Picker lists ─────────────────────────────────────────────────────────

    public IReadOnlyList<string> MethodNames   { get; }
    public IReadOnlyList<string> LocationNames { get; }

    // ── State ────────────────────────────────────────────────────────────────

    private int _origMethodIndex, _origLocationIndex;
    private int _origOrbDeg, _origOrbMin;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _selectedMethodIndex;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _selectedLocationIndex;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _orbDeg;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _orbMin;

    public bool IsDirty =>
        SelectedMethodIndex   != _origMethodIndex   ||
        SelectedLocationIndex != _origLocationIndex ||
        (int)(OrbDeg ?? 0)    != _origOrbDeg        ||
        (int)(OrbMin ?? 0)    != _origOrbMin;

    // ── Labels ───────────────────────────────────────────────────────────────

    public string SectionTitle         => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.nav.solar");
    public string LabelBackToOverview  => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.backtoverview");
    public string LabelSave            => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.edit.save");
    public string LabelCancel          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.cancel");
    public string LabelSectionSettings => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.section.settings");
    public string LabelMethod          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.solar.method");
    public string LabelLocation        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.solar.location");
    public string LabelOrb             => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.orb");
    public string LabelHelpTooltip     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.solar.help.tooltip");
    public string LabelHelpTitle       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.solar.help.groupbox");
    public string LabelHelpClose       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.help.close");
    public string LabelHelpLine1       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.solar.help.line1");
    public string LabelHelpLine2       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.solar.help.line2");

    // ── Constructor ──────────────────────────────────────────────────────────

    public ConfigSolarReturnSectionViewModel(
        IUserConfigurationRepository repo,
        INavigationService           nav,
        IConfigContext               configContext,
        IRosetta                     rosetta)
    {
        _repo          = repo;
        _nav           = nav;
        _configContext = configContext;
        _rosetta       = rosetta;

        MethodNames   = Enum.GetValues<SolarMethods>()
            .Select(m => rosetta.GetText(RbFile.Localizable, m.LocalizedName())).ToList();
        LocationNames = Enum.GetValues<SolarLocations>()
            .Select(l => rosetta.GetText(RbFile.Localizable, l.LocalizedName())).ToList();

        LoadFromConfig();
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    internal async Task SaveAsync()
    {
        var config = _configContext.EditingConfig;
        if (config is null) return;

        var newSolar = new SolarReturnConfig(
            FromSexagesimal(OrbDeg, OrbMin),
            Enum.GetValues<SolarMethods>()[SelectedMethodIndex],
            Enum.GetValues<SolarLocations>()[SelectedLocationIndex]);

        config.ProgressionsConfig = config.ProgressionsConfig with { SolarReturn = newSolar };
        await _repo.UpdateAsync(config);

        SaveOriginals();
        OnPropertyChanged(nameof(IsDirty));
    }

    internal void Revert()
    {
        LoadFromConfig();
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

    private SolarReturnConfig CurrentConfig() =>
        _configContext.EditingConfig?.ProgressionsConfig.SolarReturn ?? SolarReturnConfig.Default;

    private void LoadFromConfig()
    {
        var cfg       = CurrentConfig();
        var methods   = Enum.GetValues<SolarMethods>();
        var locations = Enum.GetValues<SolarLocations>();

        _selectedMethodIndex   = Math.Max(0, Array.IndexOf(methods,   cfg.Method));
        _selectedLocationIndex = Math.Max(0, Array.IndexOf(locations, cfg.Location));

        var (od, om) = ToSexagesimal(cfg.Orb);
        _orbDeg = od;
        _orbMin = om;

        SaveOriginals();
    }

    private void SaveOriginals()
    {
        _origMethodIndex   = SelectedMethodIndex;
        _origLocationIndex = SelectedLocationIndex;
        _origOrbDeg        = (int)(OrbDeg ?? 0);
        _origOrbMin        = (int)(OrbMin ?? 0);
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
