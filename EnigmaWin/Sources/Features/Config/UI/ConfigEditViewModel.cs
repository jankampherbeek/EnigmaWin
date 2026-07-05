// ConfigEditViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Data.UserConfiguration;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Config.UI;

public sealed partial class ConfigEditViewModel : ObservableObject
{
    private readonly IUserConfigurationRepository _repo;
    private readonly INavigationService _nav;
    private readonly IConfigContext _configContext;
    private readonly IRosetta _rosetta;

    private string _originalName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDirty))]
    [NotifyPropertyChangedFor(nameof(ConfigTitle))]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _isActive;

    public bool IsDirty => Name.Trim() != _originalName;

    // ── Localized labels ────────────────────────────────────────────────────

    public string LabelName          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.edit.namelabel");
    public string LabelSettings      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.edit.settings");
    public string LabelSave          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.edit.save");
    public string LabelActiveToggle  => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.edit.activetoggle");
    public string LabelActiveFooter  => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.edit.activefooter");
    public string LabelCancel        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.cancel");
    public string LabelHelpTooltip   => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.help.tooltip");
    public string LabelHelpTitle     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.help.title");
    public string LabelHelpClose     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.help.close");
    public string LabelHelpLine1     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.help.line1");
    public string LabelHelpLine2     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.help.line2");
    public string LabelSetActive     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.edit.activetoggle");

    public string LabelCalcSection        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.section.calculation");
    public string LabelDisplaySection     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.section.display");
    public string LabelGlyphsSection      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.section.glyphs");
    public string LabelFactorsSection     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.section.factors");
    public string LabelAspectsSection     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.section.aspects");
    public string LabelOrbsSection        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.section.orbs");
    public string LabelProgressionsSection => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.section.progressions");
    public string LabelFixStarsSection     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.section.fixstars");

    public string ConfigTitle => string.IsNullOrWhiteSpace(Name)
        ? _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.edit.fallbacktitle")
        : Name;

    public ConfigEditViewModel(
        IUserConfigurationRepository repo,
        INavigationService nav,
        IConfigContext configContext,
        IRosetta rosetta)
    {
        _repo = repo;
        _nav = nav;
        _configContext = configContext;
        _rosetta = rosetta;

        var config = _configContext.EditingConfig;
        if (config is not null)
        {
            _name = config.Name;
            _originalName = config.Name;
            _isActive = config.IsActive;
        }
    }

    // ── Save / Revert ───────────────────────────────────────────────────────

    internal async Task SaveAsync()
    {
        var config = _configContext.EditingConfig;
        if (config is null) return;

        var trimmed = Name.Trim();
        if (string.IsNullOrEmpty(trimmed)) return;

        config.Name = trimmed;
        await _repo.UpdateAsync(config);
        if (config.Id == _configContext.ActiveConfig.Id)
            _configContext.ActiveConfig = config;
        _originalName = trimmed;
        Name = trimmed;
        OnPropertyChanged(nameof(IsDirty));
        OnPropertyChanged(nameof(ConfigTitle));
    }

    internal void Revert()
    {
        Name = _originalName;
    }

    // ── Set Active ──────────────────────────────────────────────────────────

    internal async Task SetActiveAsync()
    {
        var config = _configContext.EditingConfig;
        if (config is null || config.IsActive) return;

        await _repo.SetActiveAsync(config.Id);
        config.IsActive = true;
        _configContext.ActiveConfig = config;
        IsActive = true;
    }

    // ── Section navigation ──────────────────────────────────────────────────

    internal void NavigateTo(string sectionRoute) => _nav.NavigateDetail(sectionRoute);
}
