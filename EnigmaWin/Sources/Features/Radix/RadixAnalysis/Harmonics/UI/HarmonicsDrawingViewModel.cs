// HarmonicsDrawingViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics.UI;

public sealed class HarmonicsDrawingViewModel : INotifyPropertyChanged
{
    private readonly IRosetta          _rosetta;
    private readonly IConfigContext    _configContext;
    private FullChart?                 _currentChart;
    private double                     _harmonic = 2.0;
    private bool                       _isBlackWhite;

    public WheelPlotData PlotData { get; private set; } = WheelPlotData.Empty;
    public WheelTheme    Theme    => _isBlackWhite ? WheelTheme.BlackWhite : WheelTheme.Color;

    public IRelayCommand ToggleBlackWhiteCommand { get; }

    public HarmonicsDrawingViewModel(IRosetta rosetta, IConfigContext configContext)
    {
        _rosetta       = rosetta;
        _configContext = configContext;

        ToggleBlackWhiteCommand = new RelayCommand(() =>
        {
            _isBlackWhite = !_isBlackWhite;
            OnPropertyChanged(nameof(Theme));
            OnPropertyChanged(nameof(IsBlackWhite));
        });
    }

    public bool IsBlackWhite => _isBlackWhite;

    public string LabelTitle         => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.title.drawing");
    public string TooltipFactsheet   => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.factsheet.tooltip");
    public string TooltipHelp        => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.help.tooltip");
    public string LabelBtnBlackWhite => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.btn.blackwhite");
    public string LabelBtnExport     => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.btn.export");

    public void Load(FullChart? chart, double harmonic)
    {
        _currentChart = chart;
        _harmonic     = harmonic;
        Rebuild();
    }

    private void Rebuild()
    {
        if (_currentChart is null)
        {
            PlotData = WheelPlotData.Empty;
            OnPropertyChanged(nameof(PlotData));
            return;
        }

        var config = _configContext.ActiveConfig;

        // Get harmonic positions for ALL active configured factors (including ASC/MC)
        var harmonicPositions = HarmonicsOrchestrator.Calculate(_currentChart, config, _harmonic);

        // Build WheelPlotItems from harmonic positions.
        // AscendantLongitude=0 so 0° Aries is at left (9 o'clock). MundaneAngle(lon, 0) = lon+90.
        const double ascLong = 0.0;
        var calcConfig = config.CalculationConfig;

        var items = new List<WheelPlotItem>();
        double harmonicMcLongitude = 0.0;

        foreach (var (factor, harmonicLon) in harmonicPositions)
        {
            if (factor == Factors.Mc)
                harmonicMcLongitude = harmonicLon;

            var mundane   = WheelGeometry.MundaneAngle(harmonicLon, ascLong);
            var glyph     = GlyphSelector.GetGlyphForFactor(factor);
            var speedType = SpeedType.Direct;   // harmonic positions have no speed concept
            var text      = PositionText(harmonicLon);

            items.Add(new WheelPlotItem(
                Factor:            factor,
                Glyph:             glyph,
                EclipticLongitude: harmonicLon,
                MundaneAngle:      mundane,
                PlotAngle:         mundane,
                PositionText:      text,
                SpeedType:         speedType));
        }

        var resolved = GlyphOverlapResolver.Resolve(items);

        PlotData = new WheelPlotData(
            AscendantLongitude: ascLong,
            McLongitude:        harmonicMcLongitude,
            CuspLongitudes:     [],
            PlanetItems:        resolved,
            HasTime:            false,          // suppresses house cusp drawing; HarmonicsWheelCanvas draws ASC/MC lines itself
            AspectItems:        []);

        OnPropertyChanged(nameof(PlotData));
    }

    /// <summary>Formats a harmonic longitude as "deg°min'" within-sign.</summary>
    private static string PositionText(double longitude)
    {
        var inSign   = longitude % 30.0;
        var totalMin = (int)(System.Math.Abs(inSign) * 60);
        var deg      = totalMin / 60;
        var min      = totalMin % 60;
        return $"{deg}°{min:D2}'";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
