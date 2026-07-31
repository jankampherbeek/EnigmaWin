// SynastryCombineViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Synastry.UI;

public sealed class SynastryCombineViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly SynastryModel _synastryModel;
    private readonly IConfigContext _configContext;

    private int _methodIndex; // 0=Simplified, 1=Original, 2=ReferenceLocation, 3=SphericalMidpoint
    private DavisonOrchestrator.Result? _result;

    public SynastryCombineViewModel(IRosetta rosetta, SynastryModel synastryModel, IConfigContext configContext, SynastryDerivedChartViewModel chartViewModel)
    {
        _rosetta       = rosetta;
        _synastryModel = synastryModel;
        _configContext = configContext;
        ChartViewModel = chartViewModel;
        LocationPicker = new SynastryLocationPickerViewModel(rosetta);

        ShowChartCommand = new RelayCommand(ShowChart, CanShowChart);
        LocationPicker.PropertyChanged += (_, _) => ShowChartCommand.NotifyCanExecuteChanged();
    }

    public SynastryDerivedChartViewModel ChartViewModel { get; }
    public SynastryLocationPickerViewModel LocationPicker { get; }
    public IRelayCommand ShowChartCommand { get; }

    public bool HasTwoOrMore => _synastryModel.HasTwoOrMore;
    public bool HasResult    => _result is not null;

    /// <summary>True when every selected chart has a known birth location. Simplified, Original
    /// and Spherical Midpoint all derive their location from the selected charts' own locations,
    /// so none of them can silently fall back to (0,0) when a chart's location is unknown.</summary>
    public bool AllChartsHaveLocation =>
        _synastryModel.SelectedCharts.All(c => _synastryModel.LocationOf(c) is not null);

    public bool ShowMissingLocationWarning => !AllChartsHaveLocation && _methodIndex != 2;

    public int MethodIndex
    {
        get => _methodIndex;
        set
        {
            _methodIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowReferenceLocationFields));
            OnPropertyChanged(nameof(ShowMissingLocationWarning));
            ShowChartCommand.NotifyCanExecuteChanged();
        }
    }

    public bool ShowReferenceLocationFields => _methodIndex == 2;

    public string ResultDateText { get; private set; } = string.Empty;
    public string ResultTimeText { get; private set; } = string.Empty;
    public string ResultLocationText { get; private set; } = string.Empty;

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelTitle          => T("view.synastry.results.title.combine");
    public string LabelMethod         => T("view.synastry.combine.methodlabel");
    public string LabelMethodSimplified => T("view.synastry.combine.method.simplified");
    public string LabelMethodOriginal   => T("view.synastry.combine.method.original");
    public string LabelMethodReferenceLocation => T("view.synastry.combine.method.referencelocation");
    public string LabelMethodSphericalMidpoint => T("view.synastry.combine.method.sphericalmidpoint");
    public string LabelShowChart      => T("view.synastry.combine.showchart");
    public string LabelResultDate     => T("view.synastry.combine.result.date");
    public string LabelResultTime     => T("view.synastry.combine.result.time");
    public string LabelResultLocation => T("view.synastry.combine.result.location");
    public string LabelMissingLocation => T("view.synastry.combine.missinglocation");
    public string LabelHelp           => T("view.synastry.help.combine");
    public string LabelFactsheet      => T("view.synastry.combine.factsheet");

    private bool CanShowChart() =>
        HasTwoOrMore
        && (_methodIndex == 2 ? LocationPicker.HasLocation : AllChartsHaveLocation);

    private void ShowChart()
    {
        if (!CanShowChart()) return;

        var config = _configContext.ActiveConfig;
        var factorsToUse = config.FactorConfig.Settings.Where(s => s.IsUsed).Select(s => s.Factor).ToList();
        var houseSystem = (int)config.CalculationConfig.HouseSystem.SeId();

        var inputs = _synastryModel.SelectedCharts
            .Select(c =>
            {
                var loc = _synastryModel.LocationOf(c)!.Value;
                return new DavisonChartInput(
                    c.Chart.JulianDay, loc.Latitude, loc.Longitude,
                    c.Chart.Obliquity, c.Chart.HousePositions.Midheaven.Longitude);
            })
            .ToList();

        DavisonLocationMethod method = _methodIndex switch
        {
            1 => new DavisonLocationMethod.Original(),
            2 => new DavisonLocationMethod.ReferenceLocation(LocationPicker.Latitude, LocationPicker.Longitude),
            3 => new DavisonLocationMethod.SphericalMidpoint(),
            _ => new DavisonLocationMethod.Simplified()
        };

        _result = DavisonOrchestrator.Calculate(inputs, factorsToUse, houseSystem, config.CalculationConfig, method);
        ChartViewModel.SetChart(_result.Chart);

        var dt = SEWrapper.DateFromJulianDay(_result.JulianDay, gregorian: true);
        ResultDateText     = $"{dt.Date.Year:D4}-{dt.Date.Month:D2}-{dt.Date.Day:D2}";
        ResultTimeText     = $"{dt.Time.Hour:D2}:{dt.Time.Minute:D2}:{dt.Time.Second:D2}";
        ResultLocationText = FormatLocation(_result.Latitude, _result.Longitude);

        OnPropertyChanged(nameof(HasResult));
        OnPropertyChanged(nameof(ResultDateText));
        OnPropertyChanged(nameof(ResultTimeText));
        OnPropertyChanged(nameof(ResultLocationText));
    }

    private static string FormatLocation(double latitude, double longitude)
    {
        var latHemi = latitude >= 0 ? "N" : "S";
        var lonHemi = longitude >= 0 ? "E" : "W";
        return $"{PositionInDegreesConversion.DoubleToDms(latitude)} {latHemi}  {PositionInDegreesConversion.DoubleToDms(longitude)} {lonHemi}";
    }

    private string T(string key) => _rosetta.GetText(RbFile.Synastry, key);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
