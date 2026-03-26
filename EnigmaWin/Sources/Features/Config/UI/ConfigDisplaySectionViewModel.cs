// ConfigDisplaySectionViewModel.cs
// EnigmaWin
// Created by Jan Kampherbeek on 22-03-2026

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

public sealed partial class ConfigDisplaySectionViewModel : ObservableObject
{
    private readonly IUserConfigurationRepository _repo;
    private readonly INavigationService _nav;
    private readonly IConfigContext _configContext;
    private readonly IRosetta _rosetta;

    // ── Picker ──────────────────────────────────────────────────────────────

    public IReadOnlyList<string> DrawingTypeNames { get; }

    private int _origDrawingTypeIndex;
    private bool _colorsDirty;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _selectedDrawingTypeIndex;

    public ObservableCollection<SignColorRowViewModel> SignColorRows { get; } = [];

    public bool IsDirty => SelectedDrawingTypeIndex != _origDrawingTypeIndex || _colorsDirty;

    // ── Labels ───────────────────────────────────────────────────────────────

    public string SectionTitle        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.section.display");
    public string LabelBackToOverview => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.backtoverview");
    public string LabelSave           => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.edit.save");
    public string LabelCancel         => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.cancel");
    public string LabelSectionDrawing => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.display.section.drawing");
    public string LabelDrawingType    => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.display.drawingtype");
    public string LabelSignColors     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.display.section.signcolors");
    public string LabelHelpTooltip    => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.display.help.tooltip");
    public string LabelHelpTitle      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.display.help.groupbox");
    public string LabelHelpClose      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.help.close");
    public string LabelHelpLine1      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.display.help.line1");
    public string LabelHelpLine2      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.display.help.line2");
    public string LabelHelpLine3      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.display.help.line3");
    public string LabelHelpLine4      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.display.help.line4");

    // ── Constructor ──────────────────────────────────────────────────────────

    public ConfigDisplaySectionViewModel(
        IUserConfigurationRepository repo,
        INavigationService nav,
        IConfigContext configContext,
        IRosetta rosetta)
    {
        _repo = repo;
        _nav = nav;
        _configContext = configContext;
        _rosetta = rosetta;

        DrawingTypeNames = Enum.GetValues<DrawingTypes>()
            .Select(d => rosetta.GetText(RbFile.Localizable, d.LocalizedName()))
            .ToList();

        var config  = configContext.EditingConfig;
        var display = config?.DisplayConfig ?? DisplayConfig.Default;

        var dtVals = Enum.GetValues<DrawingTypes>();
        _selectedDrawingTypeIndex = Math.Max(0, Array.IndexOf(dtVals, display.DrawingType));
        _origDrawingTypeIndex     = _selectedDrawingTypeIndex;

        foreach (var sign in Enum.GetValues<Signs>())
        {
            var existing  = display.SignColors.FirstOrDefault(sc => sc.Sign == sign);
            var colorConf = existing != default ? existing.Color : ColorConfig.DefaultSignColor;
            var signName  = rosetta.GetText(RbFile.Localizable, sign.LocalizedName());
            SignColorRows.Add(new SignColorRowViewModel(sign, signName, colorConf, OnColorChanged));
        }
    }

    private void OnColorChanged()
    {
        _colorsDirty = true;
        OnPropertyChanged(nameof(IsDirty));
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    internal async Task SaveAsync()
    {
        var config = _configContext.EditingConfig;
        if (config is null) return;

        var dtVals      = Enum.GetValues<DrawingTypes>();
        var drawingType = dtVals[SelectedDrawingTypeIndex];
        var signColors  = SignColorRows
            .Select(r => new SignColorOverride(r.Sign, r.ToColorConfig()))
            .ToList();

        config.DisplayConfig = new DisplayConfig(drawingType, signColors);
        await _repo.UpdateAsync(config);

        if (config.Id == _configContext.ActiveConfig.Id)
            _configContext.ActiveConfig = config;

        _origDrawingTypeIndex = SelectedDrawingTypeIndex;
        _colorsDirty = false;
        OnPropertyChanged(nameof(IsDirty));
    }

    internal void Revert()
    {
        SelectedDrawingTypeIndex = _origDrawingTypeIndex;

        var config  = _configContext.EditingConfig;
        var display = config?.DisplayConfig ?? DisplayConfig.Default;

        foreach (var row in SignColorRows)
        {
            var existing  = display.SignColors.FirstOrDefault(sc => sc.Sign == row.Sign);
            var colorConf = existing != default ? existing.Color : ColorConfig.DefaultSignColor;
            row.ResetColor(colorConf);
        }

        _colorsDirty = false;
        OnPropertyChanged(nameof(IsDirty));
    }

    internal void GoBack()
    {
        if (_nav.CanGoBackDetail)
            _nav.GoBackDetail();
        else
            _nav.NavigateDetail(AppRoutes.ConfigEdit);
    }
}
