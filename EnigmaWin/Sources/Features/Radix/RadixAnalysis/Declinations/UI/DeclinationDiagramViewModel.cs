// DeclinationDiagramViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public sealed class DeclinationDiagramViewModel : INotifyPropertyChanged
{
    private readonly IRosetta       _rosetta;
    private readonly IConfigContext _configContext;
    private bool   _hasData;
    private bool   _isBlackWhite;
    private bool   _showPositionLines;
    private double _obliquity = 23.45;

    public record DiagramRow(
        string FactorGlyph, string LongitudeDms, string SignGlyph, string DeclinationDms, bool IsEvenRow);

    public ObservableCollection<DeclDiagramItem> DiagramItems { get; } = [];
    public ObservableCollection<DiagramRow>      Rows         { get; } = [];

    public IRelayCommand ToggleBlackWhiteCommand    { get; }
    public IRelayCommand TogglePositionLinesCommand { get; }

    public DeclinationDiagramViewModel(IRosetta rosetta, IConfigContext configContext)
    {
        _rosetta       = rosetta;
        _configContext = configContext;

        ToggleBlackWhiteCommand = new RelayCommand(() =>
        {
            _isBlackWhite = !_isBlackWhite;
            OnPropertyChanged(nameof(IsBlackWhite));
        });

        TogglePositionLinesCommand = new RelayCommand(() =>
        {
            _showPositionLines = !_showPositionLines;
            OnPropertyChanged(nameof(ShowPositionLines));
        });
    }

    public bool   IsBlackWhite     => _isBlackWhite;
    public bool   ShowPositionLines => _showPositionLines;
    public double Obliquity         => _obliquity;

    public bool HasData
    {
        get => _hasData;
        private set { if (_hasData == value) return; _hasData = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoData)); }
    }
    public bool HasNoData => !HasData;

    public string LabelTitle          => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.diagram.title");
    public string LabelBtnBlackWhite  => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.btn.blackwhite");
    public string LabelBtnExport      => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.btn.export");
    public string LabelBtnPosLines    => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.diagram.btn.poslines");
    public string TooltipFactsheet    => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.factsheet.tooltip");
    public string TooltipHelp         => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.help.tooltip");
    public string LabelColFactor      => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.col.factor");
    public string LabelColLongitude   => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.col.longitude");
    public string LabelColDeclination => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.col.declination");
    public string LabelEmpty          => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.nodata");

    public void LoadChart(FullChart? chart)
    {
        DiagramItems.Clear();
        Rows.Clear();

        if (chart is null)
        {
            HasData = false;
            OnPropertyChanged(nameof(DiagramItems));
            OnPropertyChanged(nameof(Rows));
            return;
        }

        _obliquity = chart.Obliquity;
        OnPropertyChanged(nameof(Obliquity));

        var factorSettings = _configContext.ActiveConfig.FactorConfig.Settings
            .Where(s => s.IsUsed)
            .ToList();

        var rowIndex = 0;
        foreach (var setting in factorSettings.OrderBy(s => (int)s.Factor))
        {
            double longitude;
            double declination;

            if (setting.Factor.CalculationType() == CalculationTypes.Mundane)
            {
                var mundane = MundanePosition(setting.Factor, chart.HousePositions);
                if (mundane is null) continue;
                longitude   = mundane.Longitude;
                declination = mundane.Declination;
            }
            else
            {
                if (!chart.Coordinates.TryGetValue(setting.Factor, out var pos)) continue;
                if (pos.Equatorial.Length == 0) continue;
                declination = pos.Equatorial[0].Deviation;
                longitude   = pos.Ecliptical.Length > 0 ? pos.Ecliptical[0].MainPos : 0.0;
            }

            var glyph = GlyphSelector.GetGlyphForFactor(setting.Factor);

            var declDms = PositionInDegreesConversion.DoubleToDms(declination);
            var (lonDms, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(longitude);
            var signGlyph = ok && sign.HasValue ? GlyphSelector.GetGlyphForSign(sign.Value) : "";

            DiagramItems.Add(new DeclDiagramItem(setting.Factor, glyph, longitude, declination, lonDms, signGlyph, declDms));
            Rows.Add(new DiagramRow(glyph, lonDms, signGlyph, declDms, rowIndex++ % 2 == 0));
        }

        HasData = DiagramItems.Count > 0;
        OnPropertyChanged(nameof(DiagramItems));
        OnPropertyChanged(nameof(Rows));
    }

    private static FullCuspPosition? MundanePosition(Factors factor, HousePositions hp) => factor switch
    {
        Factors.Ascendant => hp.Ascendant,
        Factors.Mc        => hp.Midheaven,
        Factors.EastPoint => hp.Eastpoint,
        Factors.Vertex    => hp.Vertex,
        _                 => null,
    };

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
