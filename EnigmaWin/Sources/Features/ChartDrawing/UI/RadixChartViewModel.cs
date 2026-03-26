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
    [ObservableProperty] private bool _hideAspects  = false;
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

    public bool HasChart    => _chartSession.SelectedChart is not null;
    public bool ShowAspects => !HideAspects;

    // MARK: - Button labels (toggle between two states)

    public string LabelBlackWhite => T(IsBlackWhite ? ChartWheelKeys.ColorButton       : ChartWheelKeys.BlackWhiteButton);
    public string LabelAspects    => T(HideAspects  ? ChartWheelKeys.ShowAspectsButton  : ChartWheelKeys.NoAspectsButton);
    public string LabelTime       => T(HideTime     ? ChartWheelKeys.WithTimeButton     : ChartWheelKeys.NoTimeButton);

    private string T(string key) => _rosetta.GetText(RbFile.ChartWheel, key);

    // MARK: - Toggle commands

    [RelayCommand]
    private void ToggleBlackWhite() => IsBlackWhite = !IsBlackWhite;

    [RelayCommand]
    private void ToggleAspects() => HideAspects = !HideAspects;

    [RelayCommand]
    private void ToggleTime() => HideTime = !HideTime;

    // MARK: - Property change propagation

    partial void OnIsBlackWhiteChanged(bool value)
    {
        OnPropertyChanged(nameof(LabelBlackWhite));
        OnPropertyChanged(nameof(PlotData));
    }

    partial void OnHideAspectsChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowAspects));
        OnPropertyChanged(nameof(LabelAspects));
        OnPropertyChanged(nameof(PlotData));
    }

    partial void OnHideTimeChanged(bool value)
    {
        OnPropertyChanged(nameof(LabelTime));
        OnPropertyChanged(nameof(PlotData));
    }

    private void OnSessionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IChartSession.SelectedChart))
        {
            OnPropertyChanged(nameof(HasChart));
            OnPropertyChanged(nameof(PlotData));
        }
    }

    private void OnConfigChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IConfigContext.ActiveConfig))
        {
            OnPropertyChanged(nameof(DrawingType));
            OnPropertyChanged(nameof(PlotData));
        }
    }
}
