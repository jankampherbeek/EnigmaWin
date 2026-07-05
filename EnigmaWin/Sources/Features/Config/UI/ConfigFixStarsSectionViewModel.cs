// ConfigFixStarsSectionViewModel.cs
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
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.FixStars;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Config.UI;

public sealed partial class FixStarRowViewModel : ObservableObject
{
    private readonly Action _onChanged;

    public StarDefinitions Star { get; }
    public string DisplayName { get; }

    [ObservableProperty]
    private bool _isUsed;

    public FixStarRowViewModel(StarDefinitions star, bool isUsed, Action onChanged)
    {
        Star        = star;
        DisplayName = star.Name;
        _isUsed     = isUsed;
        _onChanged  = onChanged;
    }

    partial void OnIsUsedChanged(bool value) => _onChanged();

    public FixStarSetting ToFixStarSetting() => new(Star, IsUsed);
}

public sealed partial class ConfigFixStarsSectionViewModel : ObservableObject
{
    private readonly IUserConfigurationRepository _repo;
    private readonly INavigationService           _nav;
    private readonly IConfigContext               _configContext;
    private readonly IRosetta                     _rosetta;

    private bool _rowsDirty;
    private bool _loading;

    public ObservableCollection<FixStarRowViewModel> StarRows { get; } = [];
    public FixStarSelections[] SelectionValues { get; } = Enum.GetValues<FixStarSelections>();

    [ObservableProperty]
    private FixStarSelections _activeSelection;

    [ObservableProperty]
    private bool _includeGalacticCenter;

    [ObservableProperty]
    private double _fixStarOrb;

    [ObservableProperty]
    private int _magnitudeLimit;

    public bool IsMagnitudeSelection => ActiveSelection == FixStarSelections.Magnitude;

    public bool IsDirty => _rowsDirty;

    public string SectionTitle         => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.section.fixstars");
    public string LabelBackToOverview  => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.backtoverview");
    public string LabelSave            => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.edit.save");
    public string LabelCancel          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.cancel");
    public string LabelRestoreDefaults => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.factor.restoredefaults");
    public string LabelActiveSelection => "Active Selection";
    public string LabelOrb             => "Orb (°)";
    public string LabelMagnitudeLimit  => "Magnitude Limit";
    public string LabelGalacticCenter  => "Include Galactic Center";
    public string LabelColName         => "Star";
    public string LabelColUsed         => "Use";

    public ConfigFixStarsSectionViewModel(
        IUserConfigurationRepository repo,
        INavigationService           nav,
        IConfigContext               configContext,
        IRosetta                     rosetta)
    {
        _repo          = repo;
        _nav           = nav;
        _configContext = configContext;
        _rosetta       = rosetta;

        LoadFromConfig(CurrentFixStarConfig());
    }

    partial void OnActiveSelectionChanged(FixStarSelections value)
    {
        OnPropertyChanged(nameof(IsMagnitudeSelection));
        if (_loading) return;
        ApplySelection(value);
        MarkDirty();
    }

    private void ApplySelection(FixStarSelections selection)
    {
        switch (selection)
        {
            case FixStarSelections.Ptolemy:
                foreach (var row in StarRows)
                    row.IsUsed = row.Star.SelectionMembership.InPtolemy;
                break;
            case FixStarSelections.Robson:
                foreach (var row in StarRows)
                    row.IsUsed = row.Star.SelectionMembership.InRobson;
                break;
            case FixStarSelections.Brady:
                foreach (var row in StarRows)
                    row.IsUsed = row.Star.SelectionMembership.InBrady;
                break;
            case FixStarSelections.Magnitude:
                ApplyMagnitudeFilter();
                break;
            case FixStarSelections.SelfDefined:
                break;
        }
    }

    partial void OnIncludeGalacticCenterChanged(bool value) { if (!_loading) MarkDirty(); }
    partial void OnFixStarOrbChanged(double value)          { if (!_loading) MarkDirty(); }
    partial void OnMagnitudeLimitChanged(int value)
    {
        if (_loading) return;
        if (ActiveSelection == FixStarSelections.Magnitude)
            ApplyMagnitudeFilter();
        MarkDirty();
    }

    internal async Task SaveAsync()
    {
        var config = _configContext.EditingConfig;
        if (config is null) return;

        config.FixStarConfig = new FixStarConfig(
            IncludeGalacticCenter: IncludeGalacticCenter,
            FixStarOrb:            FixStarOrb,
            ParanTimeOrb:          config.FixStarConfig.ParanTimeOrb,
            ActiveSelection:       ActiveSelection,
            MagnitudeLimit:        MagnitudeLimit,
            FixStarSettings:       StarRows.Select(r => r.ToFixStarSetting()).ToList());

        await _repo.UpdateAsync(config);
        if (config.Id == _configContext.ActiveConfig.Id)
            _configContext.ActiveConfig = config;
        _rowsDirty = false;
        OnPropertyChanged(nameof(IsDirty));
    }

    internal void Revert()
    {
        LoadFromConfig(CurrentFixStarConfig());
        _rowsDirty = false;
        OnPropertyChanged(nameof(IsDirty));
    }

    internal void RestoreDefaults()
    {
        LoadFromConfig(FixStarConfig.Default);
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

    private FixStarConfig CurrentFixStarConfig() =>
        _configContext.EditingConfig?.FixStarConfig ?? FixStarConfig.Default;

    private void LoadFromConfig(FixStarConfig cfg)
    {
        _loading = true;
        try
        {
            IncludeGalacticCenter = cfg.IncludeGalacticCenter;
            FixStarOrb            = cfg.FixStarOrb;
            MagnitudeLimit        = cfg.MagnitudeLimit;
            ActiveSelection       = cfg.ActiveSelection;

            StarRows.Clear();
            var settingsMap = cfg.FixStarSettings.ToDictionary(s => s.FixStar.Id, s => s.IsUsed);
            foreach (var star in StarDefinitions.AllCases)
            {
                var isUsed = settingsMap.TryGetValue(star.Id, out var used) && used;
                StarRows.Add(new FixStarRowViewModel(star, isUsed, OnRowChanged));
            }
            OnPropertyChanged(nameof(IsMagnitudeSelection));
        }
        finally
        {
            _loading = false;
        }
    }

    private void ApplyMagnitudeFilter()
    {
        foreach (var row in StarRows)
            row.IsUsed = row.Star.Magnitude <= MagnitudeLimit;
    }

    private void MarkDirty()
    {
        _rowsDirty = true;
        OnPropertyChanged(nameof(IsDirty));
    }

    private void OnRowChanged()
    {
        _rowsDirty = true;
        OnPropertyChanged(nameof(IsDirty));
    }
}
