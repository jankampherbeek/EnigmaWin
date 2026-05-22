// WavesChartViewModel.cs
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
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using ScottPlot.Avalonia;

namespace EnigmaWin.Sources.Features.Cycles.CyclesWaves.UI;

public sealed class WavesChartViewModel : INotifyPropertyChanged
{
    private readonly IRosetta  _rosetta;
    private readonly WavesModel _model;

    private bool _showChart = true;
    private bool _showDms   = true;

    private static readonly Dictionary<Factors, System.Drawing.Color> FactorColor = new()
    {
        [Factors.Jupiter] = System.Drawing.Color.SteelBlue,
        [Factors.Saturn]  = System.Drawing.Color.OrangeRed,
        [Factors.Uranus]  = System.Drawing.Color.SeaGreen
    };

    public WavesChartViewModel(IRosetta rosetta, WavesModel model)
    {
        _rosetta = rosetta;
        _model   = model;

        ShowChartCommand = new RelayCommand(() => { ShowChart = true; });
        ShowTableCommand = new RelayCommand(() => { ShowChart = false; });
        ExportCsvCommand = new RelayCommand(ExportCsv);
        ToggleDmsCommand = new RelayCommand(() => ShowDms = !ShowDms);

        _model.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(WavesModel.HasResults)
                               or nameof(WavesModel.AllResults)
                               or nameof(WavesModel.SelectedFactors))
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

    public Action<AvaPlot>? PlotActions => BuildPlotAction();

    private Action<AvaPlot>? BuildPlotAction()
    {
        if (!_model.HasResults) return plot => { plot.Reset(); plot.Refresh(); };

        var factors = _model.SelectedFactors.ToList();
        var results = _model.AllResults
            .Where(kv => factors.Contains(kv.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToList());

        return plot =>
        {
            plot.Reset();
            plot.Plot.Axes.DateTimeTicksBottom();

            foreach (var factor in factors)
            {
                if (!results.TryGetValue(factor, out var series) || series.Count == 0) continue;
                var color = FactorColor.TryGetValue(factor, out var c)
                    ? ScottPlot.Color.FromColor(c) : ScottPlot.Colors.Gray;
                var xs = series.Select(p => JdToDateTime(p.JulianDay).ToOADate()).ToArray();
                var ys = series.Select(p => p.WaveValue).ToArray();
                var scatter = plot.Plot.Add.Scatter(xs, ys, color);
                scatter.LegendText = _rosetta.GetText(RbFile.Localizable, factor.LocalizedName());
                scatter.LineWidth  = 1.5f;
                scatter.MarkerSize = 0;
            }

            plot.Plot.ShowLegend();
            plot.Refresh();
        };
    }

    // ── Table data ────────────────────────────────────────────────────────────

    public List<string> ColumnHeaders =>
        _model.SelectedFactors
            .Select(f => _rosetta.GetText(RbFile.Localizable, f.LocalizedName()))
            .ToList();

    public List<WavesTableRow> TableRows
    {
        get
        {
            if (_model.AllResults.Count == 0) return [];

            var allJds = _model.AllResults.Values
                .SelectMany(s => s.Select(p => p.JulianDay))
                .Distinct().OrderBy(j => j).ToList();

            var lookup = _model.AllResults.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.ToDictionary(p => p.JulianDay, p => p.WaveValue));

            return allJds.Select(jd =>
            {
                var values = _model.SelectedFactors
                    .Select(f => lookup.TryGetValue(f, out var d) && d.TryGetValue(jd, out var v) ? v : (double?)null)
                    .ToList();
                return new WavesTableRow(JdToDateString(jd), jd, values, _showDms);
            }).ToList();
        }
    }

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelTabChart     => T("view.waves.tab.chart");
    public string LabelTabPositions => T("view.waves.tab.positions");
    public string LabelNoResults    => T("view.waves.chart.noresults");
    public string LabelExportCsv    => T("view.waves.positions.export");
    public string LabelDate         => T("view.waves.chart.date");
    public string LabelJulianDay    => T("view.waves.positions.julianday");
    public string LabelFormatToggle => _showDms
        ? T("view.waves.positions.format.decimal")
        : T("view.waves.positions.format.dms");

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
            sb.Append(CsvEscape(row.DateText)).Append(',').Append(CsvEscape($"{row.JulianDay:F4}"));
            foreach (var v in row.Values)
                sb.Append(',').Append(v.HasValue ? CsvEscape(FormatValue(v.Value)) : string.Empty);
            sb.AppendLine();
        }
        System.IO.File.WriteAllText("waves.csv", sb.ToString(), Encoding.UTF8);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

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

public sealed class WavesTableRow(string dateText, double julianDay, List<double?> values, bool showDms)
{
    public string        DateText  { get; } = dateText;
    public double        JulianDay { get; } = julianDay;
    public List<double?> Values    { get; } = values;

    public string FormattedJd => $"{JulianDay:F4}";
    public List<string> FormattedValues => Values
        .Select(v => v.HasValue
            ? (showDms ? PositionInDegreesConversion.DoubleToDms(v.Value) : $"{v.Value:F4}")
            : "—")
        .ToList();
}
