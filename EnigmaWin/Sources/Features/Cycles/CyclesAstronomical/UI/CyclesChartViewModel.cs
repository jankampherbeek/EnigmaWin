// CyclesChartViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using ScottPlot;
using ScottPlot.Avalonia;

namespace EnigmaWin.Sources.Features.Cycles.CyclesAstronomical.UI;

public sealed class CyclesChartViewModel : INotifyPropertyChanged
{
    private readonly IRosetta  _rosetta;
    private readonly AstronomicalCyclesModel _model;

    private bool _showChart = true;
    private bool _showDms   = true;

    private static readonly System.Drawing.Color[] SeriesColors =
    [
        System.Drawing.Color.SteelBlue,  System.Drawing.Color.OrangeRed,  System.Drawing.Color.SeaGreen,
        System.Drawing.Color.DarkOrchid, System.Drawing.Color.Goldenrod,  System.Drawing.Color.Teal,
        System.Drawing.Color.Crimson,    System.Drawing.Color.SlateBlue,  System.Drawing.Color.DarkOliveGreen,
        System.Drawing.Color.Peru,       System.Drawing.Color.DodgerBlue, System.Drawing.Color.HotPink
    ];

    public CyclesChartViewModel(IRosetta rosetta, AstronomicalCyclesModel model)
    {
        _rosetta = rosetta;
        _model   = model;

        ShowChartCommand = new RelayCommand(() => { ShowChart = true; });
        ShowTableCommand = new RelayCommand(() => { ShowChart = false; });
        ExportCsvCommand = new RelayCommand(ExportCsv);
        ToggleDmsCommand = new RelayCommand(() => ShowDms = !ShowDms);

        _model.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(AstronomicalCyclesModel.HasResults)
                               or nameof(AstronomicalCyclesModel.SingleResults)
                               or nameof(AstronomicalCyclesModel.PairResults)
                               or nameof(AstronomicalCyclesModel.IsPairs))
            {
                Dispatcher.UIThread.Post(() =>
                {
                    OnPropertyChanged(nameof(HasResults));
                    OnPropertyChanged(nameof(TableRows));
                    OnPropertyChanged(nameof(ColumnHeaders));
                    OnPropertyChanged(nameof(PlotActions));
                });
            }
        };
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public IRelayCommand ShowChartCommand { get; }
    public IRelayCommand ShowTableCommand { get; }
    public IRelayCommand ExportCsvCommand { get; }
    public IRelayCommand ToggleDmsCommand { get; }

    // ── Properties ────────────────────────────────────────────────────────────

    public bool HasResults => _model.HasResults;

    public bool ShowChart
    {
        get => _showChart;
        set { _showChart = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowTable)); }
    }

    public bool ShowTable => !_showChart;

    public bool ShowDms
    {
        get => _showDms;
        set
        {
            _showDms = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LabelFormatToggle));
            OnPropertyChanged(nameof(TableRows));
        }
    }

    // PlotActions is an Action<AvaPlot> that the code-behind subscribes to.
    // It fires whenever the plot needs to be rebuilt.
    public Action<AvaPlot>? PlotActions => BuildPlotAction();

    private Action<AvaPlot>? BuildPlotAction()
    {
        if (!_model.HasResults) return plot => { plot.Reset(); plot.Refresh(); };

        var isPairs   = _model.IsPairs;
        var pairs     = _model.FactorPairs.ToList();
        var pairData  = _model.PairResults.Select(s => s.ToList()).ToList();
        var singles   = _model.SingleResults.ToList();
        var coord     = _model.Coordinate;
        var isAngular = coord is Domain.Coordinates.Longitude or Domain.Coordinates.RightAscension;

        return plot =>
        {
            plot.Reset();
            plot.Plot.Axes.DateTimeTicksBottom();

            if (isPairs)
            {
                for (var i = 0; i < pairData.Count; i++)
                {
                    var series = pairData[i];
                    if (series.Count == 0) continue;
                    var xs = series.Select(p => JdToDateTime(p.JulianDay).ToOADate()).ToArray();
                    var ys = series.Select(p => p.Difference).ToArray();
                    var label = pairs.Count > i
                        ? $"{FactorName(pairs[i].Factor1)} – {FactorName(pairs[i].Factor2)}"
                        : $"{i + 1}";
                    var color = ScottPlot.Color.FromColor(SeriesColors[i % SeriesColors.Length]);
                    var scatter = plot.Plot.Add.Scatter(xs, ys, color);
                    scatter.LegendText = label;
                    scatter.LineWidth  = 1.5f;
                    scatter.MarkerSize = 0;
                }
            }
            else
            {
                for (var i = 0; i < singles.Count; i++)
                {
                    var (factor, series) = singles[i];
                    if (series.Count == 0) continue;
                    var color = ScottPlot.Color.FromColor(SeriesColors[i % SeriesColors.Length]);
                    var label = FactorName(factor);

                    if (isAngular)
                    {
                        // Split at wrap-arounds > 180°
                        var segStart = 0;
                        for (var j = 1; j <= series.Count; j++)
                        {
                            var isBreak = j == series.Count ||
                                Math.Abs(series[j].Position - series[j - 1].Position) > 180.0;
                            if (!isBreak) continue;
                            var seg = series.Skip(segStart).Take(j - segStart).ToList();
                            var xs  = seg.Select(p => JdToDateTime(p.JulianDay).ToOADate()).ToArray();
                            var ys  = seg.Select(p => p.Position).ToArray();
                            var scatter = plot.Plot.Add.Scatter(xs, ys, color);
                            scatter.LegendText = segStart == 0 ? label : string.Empty;
                            scatter.LineWidth  = 1.5f;
                            scatter.MarkerSize = 0;
                            segStart = j;
                        }
                    }
                    else
                    {
                        var xs = series.Select(p => JdToDateTime(p.JulianDay).ToOADate()).ToArray();
                        var ys = series.Select(p => p.Position).ToArray();
                        var scatter = plot.Plot.Add.Scatter(xs, ys, color);
                        scatter.LegendText = label;
                        scatter.LineWidth  = 1.5f;
                        scatter.MarkerSize = 0;
                    }
                }
            }

            plot.Plot.ShowLegend();
            plot.Refresh();
        };
    }

    // ── Table data ────────────────────────────────────────────────────────────

    public List<string> ColumnHeaders
    {
        get
        {
            if (_model.IsPairs)
                return _model.FactorPairs
                    .Select(p => $"{FactorName(p.Factor1)} – {FactorName(p.Factor2)}")
                    .ToList();
            return _model.SingleResults.Select(r => FactorName(r.Factor)).ToList();
        }
    }

    public List<TableRow> TableRows
    {
        get
        {
            if (_model.IsPairs)
            {
                if (_model.PairResults.Count == 0) return [];
                var first = _model.PairResults[0];
                return first.Select((entry, i) =>
                {
                    var values = _model.PairResults.Select(s => s.Count > i ? s[i].Difference : 0.0).ToList();
                    return new TableRow(JdToDateString(entry.JulianDay), entry.JulianDay, values, _showDms);
                }).ToList();
            }
            else
            {
                if (_model.SingleResults.Count == 0) return [];
                var first = _model.SingleResults[0].Series;
                return first.Select((entry, i) =>
                {
                    var values = _model.SingleResults.Select(r => r.Series.Count > i ? r.Series[i].Position : 0.0).ToList();
                    return new TableRow(JdToDateString(entry.JulianDay), entry.JulianDay, values, _showDms);
                }).ToList();
            }
        }
    }

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelTabChart        => T("view.astrocycles.tab.chart");
    public string LabelTabPositions    => T("view.astrocycles.tab.positions");
    public string LabelNoResults       => T("view.astrocycles.chart.noresults");
    public string LabelExportCsv       => T("view.astrocycles.positions.export");
    public string LabelDate            => T("view.astrocycles.chart.date");
    public string LabelJulianDay       => T("view.astrocycles.positions.julianday");
    public string LabelFormatToggle    => _showDms
        ? T("view.astrocycles.positions.format.decimal")
        : T("view.astrocycles.positions.format.dms");

    // ── CSV export ────────────────────────────────────────────────────────────

    private void ExportCsv()
    {
        var rows    = TableRows;
        var headers = ColumnHeaders;
        var sb      = new StringBuilder();
        sb.Append(CsvEscape(LabelDate)).Append(',').Append(CsvEscape(LabelJulianDay));
        foreach (var h in headers) sb.Append(',').Append(CsvEscape(h));
        sb.AppendLine();
        foreach (var row in rows)
        {
            sb.Append(CsvEscape(row.DateText)).Append(',')
              .Append(CsvEscape($"{row.JulianDay:F4}"));
            foreach (var v in row.Values) sb.Append(',').Append(CsvEscape(FormatValue(v)));
            sb.AppendLine();
        }
        System.IO.File.WriteAllText("astronomical_cycles.csv", sb.ToString(), Encoding.UTF8);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string FactorName(Factors f) => _rosetta.GetText(RbFile.Localizable, f.LocalizedName());

    private string FormatValue(double v) =>
        _showDms ? PositionInDegreesConversion.DoubleToDms(v) : $"{v:F4}";

    private static string CsvEscape(string s) =>
        s.Contains(',') || s.Contains('"') || s.Contains('\n')
            ? $"\"{s.Replace("\"", "\"\"")}\"" : s;

    private static DateTime JdToDateTime(double jd) =>
        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(jd - 2440587.5);

    private static string JdToDateString(double jd) => JdToDateTime(jd).ToString("yyyy/MM/dd");

    private string T(string key) => _rosetta.GetText(RbFile.Localizable, key);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed class TableRow
{
    public string       DateText   { get; }
    public double       JulianDay  { get; }
    public List<double> Values     { get; }
    public bool         ShowDms    { get; }

    public TableRow(string dateText, double julianDay, List<double> values, bool showDms)
    {
        DateText  = dateText;
        JulianDay = julianDay;
        Values    = values;
        ShowDms   = showDms;
    }

    public string FormattedJd => $"{JulianDay:F4}";
    public List<string> FormattedValues => Values
        .Select(v => ShowDms ? PositionInDegreesConversion.DoubleToDms(v) : $"{v:F4}")
        .ToList();
}
