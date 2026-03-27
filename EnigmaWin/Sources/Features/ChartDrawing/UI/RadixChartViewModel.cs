// RadixChartViewModel.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using System.ComponentModel;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

/// <summary>
/// ViewModel for the horoscope chart screen.
/// Manages toggle state and exposes the WheelPlotData to draw.
/// The DrawingType is read from the active configuration.
/// </summary>
public partial class RadixChartViewModel : ObservableObject
{
    private readonly IChartSession  _chartSession;
    private readonly IConfigContext _configContext;
    private readonly IRosetta       _rosetta;

    [ObservableProperty] private bool _isBlackWhite = false;
    [ObservableProperty] private bool _hideTime     = false;

    public RadixChartViewModel(IChartSession chartSession, IConfigContext configContext, IRosetta rosetta)
    {
        _chartSession  = chartSession;
        _configContext = configContext;
        _rosetta       = rosetta;

        if (_chartSession is INotifyPropertyChanged sessionNotify)
            sessionNotify.PropertyChanged += OnSessionChanged;

        if (_configContext is INotifyPropertyChanged configNotify)
            configNotify.PropertyChanged += OnConfigChanged;
    }

    // MARK: - Drawing type

    public DrawingTypes DrawingType =>
        _configContext.ActiveConfig.DisplayConfig.DrawingType;

    // MARK: - Plot data

    public WheelPlotData PlotData
    {
        get
        {
            var chart = _chartSession.SelectedChart;
            if (chart is null) return WheelPlotData.Empty;

            var raw = WheelPlotDataBuilder.Build(chart, _configContext.ActiveConfig);
            return ZodiacTypeWheelViewModel.EffectiveData(raw, HideTime);
        }
    }

    public WheelPlotData HousePlotData
    {
        get
        {
            var chart = _chartSession.SelectedChart;
            if (chart is null) return WheelPlotData.Empty;

            var raw = HouseWheelPlotDataBuilder.Build(chart, _configContext.ActiveConfig);
            return HouseTypeWheelViewModel.EffectiveData(raw, HideTime);
        }
    }

    public WheelPlotData DialPlotData
    {
        get
        {
            var chart = _chartSession.SelectedChart;
            if (chart is null) return WheelPlotData.Empty;

            var raw = DialPlotDataBuilder.Build(chart, _configContext.ActiveConfig);
            return DialPlotDataBuilder.EffectiveData(raw, HideTime);
        }
    }

    public WheelPlotData Dial90PlotData
    {
        get
        {
            var chart = _chartSession.SelectedChart;
            if (chart is null) return WheelPlotData.Empty;

            var raw = Dial90PlotDataBuilder.Build(chart, _configContext.ActiveConfig);
            return Dial90PlotDataBuilder.EffectiveData(raw, HideTime);
        }
    }

    public bool IsZodiacWheel  => DrawingType == DrawingTypes.SignBased;
    public bool IsHouseWheel   => DrawingType == DrawingTypes.HouseBased;
    public bool IsFrenchWheel  => DrawingType == DrawingTypes.French;
    public bool IsRingWheel    => DrawingType == DrawingTypes.Ring;
    public bool IsDial360Wheel => DrawingType == DrawingTypes.Dial360;
    public bool IsDial90Wheel  => DrawingType == DrawingTypes.Dial90;

    public WheelTheme Theme => IsBlackWhite ? WheelTheme.BlackWhite : WheelTheme.Color;

    public bool HasChart    => _chartSession.SelectedChart is not null;
    public bool ShowAspects => true;

    // MARK: - Button labels (toggle between two states)

    public string LabelBlackWhite => T(IsBlackWhite ? ChartWheelKeys.ColorButton   : ChartWheelKeys.BlackWhiteButton);
    public string LabelTime       => T(HideTime     ? ChartWheelKeys.WithTimeButton : ChartWheelKeys.NoTimeButton);
    public string LabelExport     => T(ChartWheelKeys.ExportButton);
    public string LabelHelp       => T(ChartWheelKeys.HelpButton);
    public string LabelDial360    => T(ChartWheelKeys.DialType360);
    public string LabelDial90     => T(ChartWheelKeys.DialType90);
    public string LabelDial45     => T(ChartWheelKeys.DialType45);

    private string T(string key) => _rosetta.GetText(RbFile.ChartWheel, key);

    // MARK: - Toggle commands

    [RelayCommand]
    private void ToggleBlackWhite() => IsBlackWhite = !IsBlackWhite;

    [RelayCommand]
    private void ToggleTime() => HideTime = !HideTime;

    // MARK: - Property change propagation

    partial void OnIsBlackWhiteChanged(bool value)
    {
        OnPropertyChanged(nameof(LabelBlackWhite));
        OnPropertyChanged(nameof(Theme));
    }

    partial void OnHideTimeChanged(bool value)
    {
        OnPropertyChanged(nameof(LabelTime));
        OnPropertyChanged(nameof(PlotData));
        OnPropertyChanged(nameof(HousePlotData));
        OnPropertyChanged(nameof(DialPlotData));
        OnPropertyChanged(nameof(Dial90PlotData));
    }

    private void OnSessionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IChartSession.SelectedChart))
        {
            OnPropertyChanged(nameof(HasChart));
            OnPropertyChanged(nameof(PlotData));
            OnPropertyChanged(nameof(HousePlotData));
            OnPropertyChanged(nameof(DialPlotData));
            OnPropertyChanged(nameof(Dial90PlotData));
        }
    }

    private void OnConfigChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IConfigContext.ActiveConfig))
        {
            OnPropertyChanged(nameof(DrawingType));
            OnPropertyChanged(nameof(IsZodiacWheel));
            OnPropertyChanged(nameof(IsHouseWheel));
            OnPropertyChanged(nameof(IsFrenchWheel));
            OnPropertyChanged(nameof(IsRingWheel));
            OnPropertyChanged(nameof(IsDial360Wheel));
            OnPropertyChanged(nameof(IsDial90Wheel));
            OnPropertyChanged(nameof(PlotData));
            OnPropertyChanged(nameof(HousePlotData));
            OnPropertyChanged(nameof(DialPlotData));
            OnPropertyChanged(nameof(Dial90PlotData));
        }
    }
}
