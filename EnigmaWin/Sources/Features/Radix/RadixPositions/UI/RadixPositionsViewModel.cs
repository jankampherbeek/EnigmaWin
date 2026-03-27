// RadixPositionsViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixPositions.UI;

public sealed class RadixPositionsViewModel : INotifyPropertyChanged
{
    private readonly RadixPositionsModel _model = new();
    private readonly IRosetta _rosetta;
    private readonly IConfigContext? _configContext;
    private bool _hasData;

    public RadixPositionsViewModel(IRosetta rosetta, IConfigContext? configContext = null)
    {
        _rosetta = rosetta;
        _configContext = configContext;
    }

    public ObservableCollection<RadixPositionsModel.PlanetPositionRow> PlanetRows { get; } = [];
    public ObservableCollection<RadixPositionsModel.CuspPositionRow> CuspRows { get; } = [];
    public bool HasData
    {
        get => _hasData;
        private set
        {
            if (_hasData == value) return;
            _hasData = value;
            OnPropertyChanged(nameof(HasData));
            OnPropertyChanged(nameof(HasNoData));
        }
    }

    public bool HasNoData => !HasData;

    private bool _isWideLayout = true;
    public bool IsWideLayout
    {
        get => _isWideLayout;
        set
        {
            if (_isWideLayout == value) return;
            _isWideLayout = value;
            OnPropertyChanged(nameof(IsWideLayout));
            OnPropertyChanged(nameof(IsNarrowLayout));
        }
    }
    public bool IsNarrowLayout => !IsWideLayout;

    // Localized column headers
    public string LabelLength         => _rosetta.GetText(RbFile.RadixPositions, "positions.header.length");
    public string LabelLatitude       => _rosetta.GetText(RbFile.RadixPositions, "positions.header.latitude");
    public string LabelRightAscension => _rosetta.GetText(RbFile.RadixPositions, "positions.header.rightascension");
    public string LabelDeclination    => _rosetta.GetText(RbFile.RadixPositions, "positions.header.declination");
    public string LabelDistance       => _rosetta.GetText(RbFile.RadixPositions, "positions.header.distance");
    public string LabelAzimuth        => _rosetta.GetText(RbFile.RadixPositions, "positions.header.azimuth");
    public string LabelAltitude       => _rosetta.GetText(RbFile.RadixPositions, "positions.header.altitude");
    public string LabelCusp           => _rosetta.GetText(RbFile.RadixPositions, "positions.header.cusp");
    public string LabelEmpty          => _rosetta.GetText(RbFile.RadixPositions, "positions.empty");
    public string TooltipHelp         => _rosetta.GetText(RbFile.RadixPositions, "positions.help.tooltip");

    public event PropertyChangedEventHandler? PropertyChanged;

    public void LoadChart(FullChart? chart)
    {
        PlanetRows.Clear();
        CuspRows.Clear();

        if (chart == null)
        {
            HasData = false;
            return;
        }

        var factorConfig = _configContext?.ActiveConfig.FactorConfig;
        var (planetRows, cuspRows) = _model.BuildRows(chart, factorConfig);
        foreach (var row in planetRows)
        {
            PlanetRows.Add(row);
        }

        foreach (var row in cuspRows)
        {
            CuspRows.Add(row);
        }

        HasData = PlanetRows.Count > 0 || CuspRows.Count > 0;
        OnPropertyChanged(nameof(PlanetRows));
        OnPropertyChanged(nameof(CuspRows));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
