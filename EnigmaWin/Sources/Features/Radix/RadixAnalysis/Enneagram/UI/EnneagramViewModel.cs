// EnneagramViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Enneagram.UI;

public sealed class EnneagramViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly IConfigContext _configContext;
    private readonly IChartSession _chartSession;
    private FullChart? _currentChart;

    // Per-factor toggle backing fields
    private bool _sunSelected = true;
    private bool _moonSelected = true;
    private bool _mercurySelected = true;
    private bool _venusSelected = true;
    private bool _marsSelected = true;
    private bool _jupiterSelected = true;
    private bool _saturnSelected = true;
    private bool _uranusSelected = true;
    private bool _neptuneSelected = true;
    private bool _plutoSelected = true;
    private bool _chironSelected = true;
    private bool _northNodeTrueSelected = true;
    private bool _apogeeMeanSelected = true;

    // Options
    private bool _includeHouses = true;
    private bool _countPlutoTwice;
    private bool _useUpdated = true;

    // Tab state
    private enum Tab { Overview, Details }
    private Tab _activeTab = Tab.Overview;

    // Results
    public ObservableCollection<EnneagramTypeRow>   TypeRows   { get; } = new();
    public ObservableCollection<EnneagramDetailRow> DetailRows { get; } = new();

    // Enneagram drawing data
    public IReadOnlyList<EnneagramCircleData> Circles { get; private set; } = new List<EnneagramCircleData>();
    public IReadOnlyList<EnneagramLineData>   Lines   { get; private set; } = new List<EnneagramLineData>();

    public bool ShowOverview => _activeTab == Tab.Overview;
    public bool ShowDetails  => _activeTab == Tab.Details;

    private bool _hasData;
    public bool HasData    { get => _hasData; private set { if (_hasData == value) return; _hasData = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoData)); } }
    public bool HasNoData  => !HasData;

    // Factor selection properties
    public bool SunSelected           { get => _sunSelected;           set { if (_sunSelected           == value) return; _sunSelected           = value; OnPropertyChanged(); Recalculate(); } }
    public bool MoonSelected          { get => _moonSelected;          set { if (_moonSelected          == value) return; _moonSelected          = value; OnPropertyChanged(); Recalculate(); } }
    public bool MercurySelected       { get => _mercurySelected;       set { if (_mercurySelected       == value) return; _mercurySelected       = value; OnPropertyChanged(); Recalculate(); } }
    public bool VenusSelected         { get => _venusSelected;         set { if (_venusSelected         == value) return; _venusSelected         = value; OnPropertyChanged(); Recalculate(); } }
    public bool MarsSelected          { get => _marsSelected;          set { if (_marsSelected          == value) return; _marsSelected          = value; OnPropertyChanged(); Recalculate(); } }
    public bool JupiterSelected       { get => _jupiterSelected;       set { if (_jupiterSelected       == value) return; _jupiterSelected       = value; OnPropertyChanged(); Recalculate(); } }
    public bool SaturnSelected        { get => _saturnSelected;        set { if (_saturnSelected        == value) return; _saturnSelected        = value; OnPropertyChanged(); Recalculate(); } }
    public bool UranusSelected        { get => _uranusSelected;        set { if (_uranusSelected        == value) return; _uranusSelected        = value; OnPropertyChanged(); Recalculate(); } }
    public bool NeptuneSelected       { get => _neptuneSelected;       set { if (_neptuneSelected       == value) return; _neptuneSelected       = value; OnPropertyChanged(); Recalculate(); } }
    public bool PlutoSelected         { get => _plutoSelected;         set { if (_plutoSelected         == value) return; _plutoSelected         = value; OnPropertyChanged(); Recalculate(); } }
    public bool ChironSelected        { get => _chironSelected;        set { if (_chironSelected        == value) return; _chironSelected        = value; OnPropertyChanged(); Recalculate(); } }
    public bool NorthNodeTrueSelected { get => _northNodeTrueSelected; set { if (_northNodeTrueSelected == value) return; _northNodeTrueSelected = value; OnPropertyChanged(); Recalculate(); } }
    public bool ApogeeMeanSelected    { get => _apogeeMeanSelected;    set { if (_apogeeMeanSelected    == value) return; _apogeeMeanSelected    = value; OnPropertyChanged(); Recalculate(); } }

    public bool IncludeHouses   { get => _includeHouses;   set { if (_includeHouses   == value) return; _includeHouses   = value; OnPropertyChanged(); Recalculate(); } }
    public bool CountPlutoTwice { get => _countPlutoTwice; set { if (_countPlutoTwice == value) return; _countPlutoTwice = value; OnPropertyChanged(); Recalculate(); } }
    public bool UseUpdated      { get => _useUpdated;      set { if (_useUpdated      == value) return; _useUpdated      = value; OnPropertyChanged(); Recalculate(); } }

    public IRelayCommand ShowOverviewCommand { get; }
    public IRelayCommand ShowDetailsCommand  { get; }

    public EnneagramViewModel(IRosetta rosetta, IConfigContext configContext, IChartSession chartSession)
    {
        _rosetta        = rosetta;
        _configContext  = configContext;
        _chartSession   = chartSession;
        _currentChart   = chartSession.SelectedChart;

        if (chartSession is INotifyPropertyChanged notify)
            notify.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != nameof(IChartSession.SelectedChart)) return;
                _currentChart = _chartSession.SelectedChart;
                Recalculate();
            };

        ShowOverviewCommand = new RelayCommand(() => SetTab(Tab.Overview));
        ShowDetailsCommand  = new RelayCommand(() => SetTab(Tab.Details));

        Recalculate();
    }

    private void SetTab(Tab tab)
    {
        if (_activeTab == tab) return;
        _activeTab = tab;
        OnPropertyChanged(nameof(ShowOverview));
        OnPropertyChanged(nameof(ShowDetails));
    }

    public void Refresh()
    {
        _currentChart = _chartSession.SelectedChart;
        Recalculate();
    }

    public void LoadChart(FullChart? chart)
    {
        _currentChart = chart;
        Recalculate();
    }

    private void Recalculate()
    {
        TypeRows.Clear();
        DetailRows.Clear();
        Circles = new List<EnneagramCircleData>();
        Lines   = new List<EnneagramLineData>();
        OnPropertyChanged(nameof(Circles));
        OnPropertyChanged(nameof(Lines));

        if (_currentChart == null) { HasData = false; return; }

        var selected = GetSelectedFactors();
        if (selected.Count == 0) { HasData = false; return; }

        try
        {
            var strengths = EnneagramCalculator.CalcStrengths(
                _currentChart, selected, _includeHouses, _countPlutoTwice, _useUpdated);
            var details = EnneagramCalculator.CalcDetails(
                _currentChart, selected, _includeHouses, _countPlutoTwice, _useUpdated);

            var typeRowIndex = 0;
            foreach (var (type, strength) in strengths)
                TypeRows.Add(new EnneagramTypeRow(type, GetTypeName(type), strength, typeRowIndex++ % 2 == 0));

            var rowIdx = 0;
            foreach (var d in details)
                DetailRows.Add(new EnneagramDetailRow(
                    GetFactorName(d.Factor),
                    d.InSigns ? LabelSign : LabelHouse,
                    d.PositionIndex,
                    d.TypeFactors,
                    rowIdx++ % 2 == 0));

            BuildDrawing(strengths);
            HasData = TypeRows.Count > 0;
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Enneagram calculation failed.");
            HasData = false;
        }
    }

    private void BuildDrawing(List<(int Type, double Strength)> strengths)
    {
        var sorted = strengths.OrderByDescending(x => x.Strength).ToList();
        var top1 = sorted.Count > 0 ? sorted[0].Type : -1;
        var top2 = sorted.Count > 1 ? sorted[1].Type : -1;
        var top3 = sorted.Count > 2 ? sorted[2].Type : -1;

        // Positions: 9 at top, then 1-8 clockwise (9 nodes at 40° intervals starting at -90°)
        var order = new[] { 9, 1, 2, 3, 4, 5, 6, 7, 8 };
        var circles = new List<EnneagramCircleData>();

        for (var i = 0; i < order.Length; i++)
        {
            var t     = order[i];
            var angle = i * 40.0 - 90.0;
            var rad   = angle * Math.PI / 180.0;
            var x     = 0.5 + 0.38 * Math.Cos(rad); // unit coordinates [0,1]
            var y     = 0.5 + 0.38 * Math.Sin(rad);

            Color color;
            if (t == top1)                color = Colors.Red;
            else if (t == top2 || t == top3) color = Colors.Orange;
            else                          color = Colors.CornflowerBlue;

            var strength = strengths.FirstOrDefault(s => s.Type == t).Strength;
            circles.Add(new EnneagramCircleData(t, GetTypeName(t), x, y, color, GetTypeTooltip(t), strength));
        }

        Circles = circles;

        Lines = new List<EnneagramLineData>
        {
            new(3, 6), new(6, 9), new(9, 3),          // triangle
            new(1, 4), new(4, 2), new(2, 8), new(8, 5), new(5, 7), new(7, 1)  // path
        };

        OnPropertyChanged(nameof(Circles));
        OnPropertyChanged(nameof(Lines));
    }

    private List<Factors> GetSelectedFactors()
    {
        var list = new List<Factors>();
        if (_sunSelected)           list.Add(Factors.Sun);
        if (_moonSelected)          list.Add(Factors.Moon);
        if (_mercurySelected)       list.Add(Factors.Mercury);
        if (_venusSelected)         list.Add(Factors.Venus);
        if (_marsSelected)          list.Add(Factors.Mars);
        if (_jupiterSelected)       list.Add(Factors.Jupiter);
        if (_saturnSelected)        list.Add(Factors.Saturn);
        if (_uranusSelected)        list.Add(Factors.Uranus);
        if (_neptuneSelected)       list.Add(Factors.Neptune);
        if (_plutoSelected)         list.Add(Factors.Pluto);
        if (_chironSelected)        list.Add(Factors.Chiron);
        if (_northNodeTrueSelected) list.Add(Factors.NorthNodeTrue);
        if (_apogeeMeanSelected)    list.Add(Factors.ApogeeMean);
        return list;
    }

    public string GetTypeName(int type) => type switch
    {
        1 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.type.1"),
        2 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.type.2"),
        3 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.type.3"),
        4 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.type.4"),
        5 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.type.5"),
        6 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.type.6"),
        7 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.type.7"),
        8 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.type.8"),
        9 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.type.9"),
        _ => $"Type {type}"
    };

    public string GetTypeDescription(int type) => type switch
    {
        1 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.popup.1"),
        2 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.popup.2"),
        3 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.popup.3"),
        4 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.popup.4"),
        5 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.popup.5"),
        6 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.popup.6"),
        7 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.popup.7"),
        8 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.popup.8"),
        9 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.popup.9"),
        _ => ""
    };

    private string GetTypeTooltip(int type) => type switch
    {
        1 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.tooltip.1"),
        2 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.tooltip.2"),
        3 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.tooltip.3"),
        4 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.tooltip.4"),
        5 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.tooltip.5"),
        6 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.tooltip.6"),
        7 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.tooltip.7"),
        8 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.tooltip.8"),
        9 => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.tooltip.9"),
        _ => ""
    };

    private string GetFactorName(Factors factor) =>
        _rosetta.GetText(RbFile.Localizable, factor.LocalizedName());

    // i18n labels
    public string LabelTitle          => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.title");
    public string LabelNoChart        => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.nochart");
    public string LabelBtnOverview    => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.tab.overview");
    public string LabelBtnDetails     => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.tab.details");
    public string LabelChartPoints    => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.label.chartpoints");
    public string LabelOptions        => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.label.options");
    public string LabelIncludeHouses  => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.option.includehouses");
    public string LabelCountPluto     => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.option.countpluto");
    public string LabelUseUpdated     => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.option.useupdated");
    public string LabelColType        => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.col.type");
    public string LabelColName        => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.col.name");
    public string LabelColStrength    => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.col.strength");
    public string LabelColFactor      => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.col.factor");
    public string LabelColPosition    => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.col.position");
    public string LabelSign           => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.label.sign");
    public string LabelHouse          => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.label.house");
    public string TooltipHelp         => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.help.tooltip");
    public string TooltipFactsheet    => _rosetta.GetText(RbFile.RadixEnneagram, "enneagram.factsheet.tooltip");

    // Sun through ApogeeMean labels from Localizable
    public string LabelSun           => _rosetta.GetText(RbFile.Localizable, "enum.factor.sun");
    public string LabelMoon          => _rosetta.GetText(RbFile.Localizable, "enum.factor.moon");
    public string LabelMercury       => _rosetta.GetText(RbFile.Localizable, "enum.factor.mercury");
    public string LabelVenus         => _rosetta.GetText(RbFile.Localizable, "enum.factor.venus");
    public string LabelMars          => _rosetta.GetText(RbFile.Localizable, "enum.factor.mars");
    public string LabelJupiter       => _rosetta.GetText(RbFile.Localizable, "enum.factor.jupiter");
    public string LabelSaturn        => _rosetta.GetText(RbFile.Localizable, "enum.factor.saturn");
    public string LabelUranus        => _rosetta.GetText(RbFile.Localizable, "enum.factor.uranus");
    public string LabelNeptune       => _rosetta.GetText(RbFile.Localizable, "enum.factor.neptune");
    public string LabelPluto         => _rosetta.GetText(RbFile.Localizable, "enum.factor.pluto");
    public string LabelChiron        => _rosetta.GetText(RbFile.Localizable, "enum.factor.chiron");
    public string LabelNorthNodeTrue => _rosetta.GetText(RbFile.Localizable, "enum.factor.truenode");
    public string LabelApogeeMean    => _rosetta.GetText(RbFile.Localizable, "enum.factor.apogeemean");

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

// Supporting display records
public record EnneagramTypeRow(int Type, string Name, double Strength, bool IsEvenRow);

public record EnneagramDetailRow(
    string   FactorName,
    string   PositionType,
    int      Position,
    double[] TypeFactors,
    bool     IsEvenRow)
{
    public double F1 => TypeFactors.Length > 0 ? TypeFactors[0] : 0;
    public double F2 => TypeFactors.Length > 1 ? TypeFactors[1] : 0;
    public double F3 => TypeFactors.Length > 2 ? TypeFactors[2] : 0;
    public double F4 => TypeFactors.Length > 3 ? TypeFactors[3] : 0;
    public double F5 => TypeFactors.Length > 4 ? TypeFactors[4] : 0;
    public double F6 => TypeFactors.Length > 5 ? TypeFactors[5] : 0;
    public double F7 => TypeFactors.Length > 6 ? TypeFactors[6] : 0;
    public double F8 => TypeFactors.Length > 7 ? TypeFactors[7] : 0;
    public double F9 => TypeFactors.Length > 8 ? TypeFactors[8] : 0;
}

public record EnneagramCircleData(int Type, string Name, double X, double Y, Color Color, string Tooltip, double Strength);
public record EnneagramLineData(int FromType, int ToType);
