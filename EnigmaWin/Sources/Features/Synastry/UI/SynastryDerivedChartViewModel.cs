// SynastryDerivedChartViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Synastry.UI;

/// <summary>
/// Renders a single zodiac-type wheel for a derived/synthetic FullChart (Composite or Combine
/// result) that is not stored in IChartSession. Reuses the same wheel-drawing plumbing as
/// RadixChartViewModel (WheelPlotDataBuilder + ZodiacWheelCanvas) without depending on the
/// chart session, since the source chart here is computed on demand rather than selected.
/// </summary>
public sealed class SynastryDerivedChartViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly IConfigContext _configContext;

    private bool _isBlackWhite;
    private bool _hideAspects;
    private FullChart? _chart;

    public SynastryDerivedChartViewModel(IRosetta rosetta, IConfigContext configContext)
    {
        _rosetta       = rosetta;
        _configContext = configContext;

        ToggleBlackWhiteCommand = new RelayCommand(() => IsBlackWhite = !IsBlackWhite);
        ToggleAspectsCommand    = new RelayCommand(() => HideAspects  = !HideAspects);
    }

    public IRelayCommand ToggleBlackWhiteCommand { get; }
    public IRelayCommand ToggleAspectsCommand { get; }

    /// <summary>Sets the chart to render. Pass null to clear the display.</summary>
    public void SetChart(FullChart? chart)
    {
        _chart = chart;
        OnPropertyChanged(nameof(HasChart));
        OnPropertyChanged(nameof(PlotData));
    }

    public bool HasChart => _chart is not null;

    public WheelPlotData PlotData => _chart is null
        ? WheelPlotData.Empty
        : WheelPlotDataBuilder.Build(_chart, _configContext.ActiveConfig);

    public bool IsBlackWhite
    {
        get => _isBlackWhite;
        private set
        {
            _isBlackWhite = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Theme));
            OnPropertyChanged(nameof(LabelBlackWhite));
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
            OnPropertyChanged(nameof(LabelAspects));
        }
    }

    public WheelTheme Theme       => IsBlackWhite ? WheelTheme.BlackWhite : WheelTheme.Color;
    public bool        ShowAspects => !HideAspects;

    public string LabelBlackWhite => T(IsBlackWhite ? ChartWheelKeys.ColorButton       : ChartWheelKeys.BlackWhiteButton);
    public string LabelAspects    => T(HideAspects  ? ChartWheelKeys.ShowAspectsButton : ChartWheelKeys.NoAspectsButton);
    public string LabelExport     => T(ChartWheelKeys.ExportButton);

    private string T(string key) => _rosetta.GetText(RbFile.ChartWheel, key);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
