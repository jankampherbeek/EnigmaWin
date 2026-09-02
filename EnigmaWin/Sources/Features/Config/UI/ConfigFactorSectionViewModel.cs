// ConfigFactorSectionViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

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

public sealed partial class ConfigFactorSectionViewModel : ObservableObject
{
    private readonly IUserConfigurationRepository _repo;
    private readonly INavigationService           _nav;
    private readonly IConfigContext               _configContext;
    private readonly IRosetta                     _rosetta;

    private bool _rowsDirty;

    public ObservableCollection<FactorRowViewModel> FactorRows { get; } = [];

    public bool IsDirty => _rowsDirty;

    // ── Labels ───────────────────────────────────────────────────────────────

    public string SectionTitle        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.section.factors");
    public string LabelBackToOverview => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.backtoverview");
    public string LabelSave           => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.edit.save");
    public string LabelCancel         => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.cancel");
    public string LabelRestoreDefaults => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.factor.restoredefaults");
    public string LabelColUsed        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.factor.col.used");
    public string LabelColDrawn       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.factor.col.drawn");
    public string LabelColOrb         => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.factor.col.orb");
    public string LabelHelpTooltip    => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.factor.help.tooltip");
    public string LabelHelpTitle      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.factor.help.groupbox");
    public string LabelHelpClose      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.help.close");
    public string LabelHelpLine1      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.factor.help.line1");
    public string LabelHelpLine2      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.factor.help.line2");
    public string LabelHelpLine3      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.factor.help.line3");

    // ── Constructor ──────────────────────────────────────────────────────────

    public ConfigFactorSectionViewModel(
        IUserConfigurationRepository repo,
        INavigationService           nav,
        IConfigContext               configContext,
        IRosetta                     rosetta)
    {
        _repo          = repo;
        _nav           = nav;
        _configContext = configContext;
        _rosetta       = rosetta;

        LoadRows(CurrentFactorConfig());
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    internal async Task SaveAsync()
    {
        var config = _configContext.EditingConfig;
        if (config is null) return;

        // FactorRows only covers the selectable factors (LogTimeScale/AgePoint are excluded from
        // this editor — see FactorsExtensions.SelectableFactors) — carry their existing settings
        // through unchanged so they aren't dropped from the persisted config.
        var editedSettings = FactorRows.Select(r => r.ToFactorSettings()).ToList();
        var editedFactors = editedSettings.Select(s => s.Factor).ToHashSet();
        var preserved = CurrentFactorConfig().Settings.Where(s => !editedFactors.Contains(s.Factor));
        config.FactorConfig = new FactorConfig(editedSettings.Concat(preserved).ToList());

        await _repo.UpdateAsync(config);
        if (config.Id == _configContext.ActiveConfig.Id)
            _configContext.ActiveConfig = config;
        _rowsDirty = false;
        OnPropertyChanged(nameof(IsDirty));
    }

    internal void Revert()
    {
        var factorConfig = CurrentFactorConfig();
        foreach (var row in FactorRows)
        {
            var settings = factorConfig.Settings.FirstOrDefault(s => s.Factor == row.Factor);
            row.ResetTo(settings == default ? DefaultSettingsFor(row.Factor) : settings);
        }
        _rowsDirty = false;
        OnPropertyChanged(nameof(IsDirty));
    }

    internal void RestoreDefaults()
    {
        foreach (var row in FactorRows)
            row.ResetTo(DefaultSettingsFor(row.Factor));
        _rowsDirty = true;
        OnPropertyChanged(nameof(IsDirty));
    }

    internal void GoBack()
    {
        if (_nav.CanGoBackDetail)
            _nav.GoBackDetail();
        else
            _nav.NavigateDetail(AppRoutes.ConfigEdit);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private FactorConfig CurrentFactorConfig() =>
        _configContext.EditingConfig?.FactorConfig ?? FactorConfig.Default;

    private void LoadRows(FactorConfig factorConfig)
    {
        FactorRows.Clear();
        foreach (var factor in FactorsExtensions.SelectableFactors)
        {
            var name     = _rosetta.GetText(RbFile.Localizable, factor.LocalizedName());
            var existing = factorConfig.Settings.FirstOrDefault(s => s.Factor == factor);
            var settings = existing == default ? DefaultSettingsFor(factor) : existing;
            FactorRows.Add(new FactorRowViewModel(factor, name, settings, OnRowChanged));
        }
    }

    private static FactorSettings DefaultSettingsFor(Factors factor) =>
        FactorConfig.DefaultSettings.FirstOrDefault(s => s.Factor == factor);

    private void OnRowChanged()
    {
        _rowsDirty = true;
        OnPropertyChanged(nameof(IsDirty));
    }
}
