// SynastryCompareViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Synastry.UI;

/// <summary>Bi-wheel comparison of two selected charts: one chart's wheel as the base
/// (with its own aspects), the other chart's planets plotted as an outer ring of positions only.</summary>
public sealed class SynastryCompareViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly SynastryModel _synastryModel;
    private readonly IConfigContext _configContext;

    private bool _isSwapped;
    private bool _isBlackWhite;
    private bool _hideAspects;

    public SynastryCompareViewModel(IRosetta rosetta, SynastryModel synastryModel, IConfigContext configContext)
    {
        _rosetta       = rosetta;
        _synastryModel = synastryModel;
        _configContext = configContext;

        ToggleBlackWhiteCommand = new RelayCommand(() => IsBlackWhite = !IsBlackWhite);
        ToggleAspectsCommand    = new RelayCommand(() => HideAspects  = !HideAspects);
        SwapCommand             = new RelayCommand(() => IsSwapped   = !IsSwapped);

        Rebuild();
    }

    public IRelayCommand ToggleBlackWhiteCommand { get; }
    public IRelayCommand ToggleAspectsCommand { get; }
    public IRelayCommand SwapCommand { get; }

    public bool HasTwoCharts => _synastryModel.HasExactlyTwo;

    public bool IsSwapped
    {
        get => _isSwapped;
        private set { _isSwapped = value; OnPropertyChanged(); Rebuild(); }
    }

    public bool IsBlackWhite
    {
        get => _isBlackWhite;
        private set
        {
            _isBlackWhite = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Theme));
            OnPropertyChanged(nameof(LabelWheelBlackWhite));
        }
    }

    public bool HideAspects
    {
        get => _hideAspects;
        private set
        {
            _hideAspects = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowAspects));
            OnPropertyChanged(nameof(LabelWheelAspects));
        }
    }

    public WheelTheme Theme       => IsBlackWhite ? WheelTheme.BlackWhite : WheelTheme.Color;
    public bool        ShowAspects => !HideAspects;

    public WheelPlotData RadixPlotData { get; private set; } = WheelPlotData.Empty;
    public WheelPlotItem[] OuterPlotItems { get; private set; } = [];

    public string InnerChartName { get; private set; } = string.Empty;
    public string OuterChartName { get; private set; } = string.Empty;

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelTitle      => T("view.synastry.results.title.compare");
    public string LabelInnerLabel => T("view.synastry.compare.innerlabel");
    public string LabelOuterLabel => T("view.synastry.compare.outerlabel");
    public string LabelSwap       => T("view.synastry.compare.swap");
    public string LabelHelp       => T("view.synastry.help.compare");

    public string LabelWheelBlackWhite => _rosetta.GetText(RbFile.ChartWheel,
        IsBlackWhite ? ChartWheelKeys.ColorButton : ChartWheelKeys.BlackWhiteButton);
    public string LabelWheelAspects => _rosetta.GetText(RbFile.ChartWheel,
        HideAspects ? ChartWheelKeys.ShowAspectsButton : ChartWheelKeys.NoAspectsButton);
    public string LabelWheelExport => _rosetta.GetText(RbFile.ChartWheel, ChartWheelKeys.ExportButton);

    // ── Rebuild ───────────────────────────────────────────────────────────────

    private void Rebuild()
    {
        if (!_synastryModel.HasExactlyTwo)
        {
            RadixPlotData     = WheelPlotData.Empty;
            OuterPlotItems    = [];
            OnPropertyChanged(nameof(RadixPlotData));
            OnPropertyChanged(nameof(OuterPlotItems));
            OnPropertyChanged(nameof(HasTwoCharts));
            return;
        }

        var chartA = _synastryModel.ChartA!;
        var chartB = _synastryModel.ChartB!;
        var (inner, outer) = IsSwapped ? (chartB, chartA) : (chartA, chartB);

        InnerChartName = inner.Name;
        OuterChartName = outer.Name;

        var config = _configContext.ActiveConfig;
        RadixPlotData = WheelPlotDataBuilder.Build(inner.Chart, config);

        var ascLong = inner.Chart.HousePositions.Ascendant.Longitude;
        var outerFactors = config.FactorConfig.Settings.Where(s => s.IsUsed).Select(s => s.Factor).ToHashSet();

        OuterPlotItems = outer.Chart.Coordinates
            .Where(kvp => outerFactors.Contains(kvp.Key) && kvp.Value.Ecliptical.Length > 0)
            .Select(kvp =>
            {
                var longitude = kvp.Value.Ecliptical[0].MainPos;
                var mundane   = WheelGeometry.MundaneAngle(longitude, ascLong);
                var inSign    = longitude % 30.0;
                var totalMin  = (int)(inSign * 60);
                return new WheelPlotItem(
                    Factor:            kvp.Key,
                    Glyph:             GlyphSelector.GetGlyphForFactor(kvp.Key),
                    EclipticLongitude: longitude,
                    MundaneAngle:      mundane,
                    PlotAngle:         mundane,
                    PositionText:      $"{totalMin / 60}°{totalMin % 60:D2}'",
                    SpeedType:         SpeedType.Direct);
            })
            .ToArray();

        OnPropertyChanged(nameof(RadixPlotData));
        OnPropertyChanged(nameof(OuterPlotItems));
        OnPropertyChanged(nameof(InnerChartName));
        OnPropertyChanged(nameof(OuterChartName));
        OnPropertyChanged(nameof(HasTwoCharts));
    }

    private string T(string key) => _rosetta.GetText(RbFile.Synastry, key);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
