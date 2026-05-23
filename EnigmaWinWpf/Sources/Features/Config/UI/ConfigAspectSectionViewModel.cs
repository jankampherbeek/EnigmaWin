// ConfigAspectSectionViewModel.cs
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

public sealed partial class ConfigAspectSectionViewModel : ObservableObject
{
    private readonly IUserConfigurationRepository _repo;
    private readonly INavigationService           _nav;
    private readonly IConfigContext               _configContext;
    private readonly IRosetta                     _rosetta;

    private bool _rowsDirty;

    public ObservableCollection<AspectRowViewModel> AspectRows { get; } = [];

    public bool IsDirty => _rowsDirty;

    // ── Labels ───────────────────────────────────────────────────────────────

    public string SectionTitle         => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.section.aspects");
    public string LabelBackToOverview  => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.backtoverview");
    public string LabelSave            => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.edit.save");
    public string LabelCancel          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.cancel");
    public string LabelRestoreDefaults => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.aspect.restoredefaults");
    public string LabelColUsed         => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.aspect.col.used");
    public string LabelColDrawn        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.aspect.col.drawn");
    public string LabelColOrb          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.aspect.col.orb");
    public string LabelColColor        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.aspect.col.color");
    public string LabelHelpTooltip     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.aspect.help.tooltip");
    public string LabelHelpTitle       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.aspect.help.groupbox");
    public string LabelHelpClose       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.help.close");
    public string LabelHelpLine1       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.aspect.help.line1");
    public string LabelHelpLine2       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.aspect.help.line2");
    public string LabelHelpLine3       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.aspect.help.line3");
    public string LabelHelpLine4       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.aspect.help.line4");

    // ── Constructor ──────────────────────────────────────────────────────────

    public ConfigAspectSectionViewModel(
        IUserConfigurationRepository repo,
        INavigationService           nav,
        IConfigContext               configContext,
        IRosetta                     rosetta)
    {
        _repo          = repo;
        _nav           = nav;
        _configContext = configContext;
        _rosetta       = rosetta;

        LoadRows(CurrentAspectConfig());
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    internal async Task SaveAsync()
    {
        var config = _configContext.EditingConfig;
        if (config is null) return;

        config.AspectConfig = new AspectConfig(
            AspectRows.Select(r => r.ToAspectSettings()).ToList());

        await _repo.UpdateAsync(config);
        if (config.Id == _configContext.ActiveConfig.Id)
            _configContext.ActiveConfig = config;
        _rowsDirty = false;
        OnPropertyChanged(nameof(IsDirty));
    }

    internal void Revert()
    {
        var aspectConfig = CurrentAspectConfig();
        foreach (var row in AspectRows)
        {
            var settings = aspectConfig.Settings.FirstOrDefault(s => s.Aspect == row.Aspect);
            row.ResetTo(settings == default ? DefaultSettingsFor(row.Aspect) : settings);
        }
        _rowsDirty = false;
        OnPropertyChanged(nameof(IsDirty));
    }

    internal void RestoreDefaults()
    {
        foreach (var row in AspectRows)
            row.ResetTo(DefaultSettingsFor(row.Aspect));
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

    private AspectConfig CurrentAspectConfig() =>
        _configContext.EditingConfig?.AspectConfig ?? AspectConfig.Default;

    private void LoadRows(AspectConfig aspectConfig)
    {
        AspectRows.Clear();
        foreach (var aspect in Enum.GetValues<Aspects>())
        {
            var name     = _rosetta.GetText(RbFile.Localizable, aspect.LocalizedName());
            var existing = aspectConfig.Settings.FirstOrDefault(s => s.Aspect == aspect);
            var settings = existing == default ? DefaultSettingsFor(aspect) : existing;
            AspectRows.Add(new AspectRowViewModel(aspect, name, settings, OnRowChanged));
        }
    }

    private static AspectSettings DefaultSettingsFor(Aspects aspect) =>
        AspectConfig.DefaultSettings.FirstOrDefault(s => s.Aspect == aspect);

    private void OnRowChanged()
    {
        _rowsDirty = true;
        OnPropertyChanged(nameof(IsDirty));
    }
}
