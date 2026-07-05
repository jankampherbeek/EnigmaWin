// CyclesChartViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Cycles;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using ScottPlot.WPF;

namespace EnigmaWin.Sources.Features.Cycles.CyclesAstronomical.UI;

public sealed class CyclesChartViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
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

    private static readonly string GlyphFontPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "Resources", "Fonts", "EnigmaAstrology3.ttf");

    public CyclesChartViewModel(IRosetta rosetta, AstronomicalCyclesModel model)
    {
        _rosetta = rosetta;
        _model   = model;

        ShowChartCommand   = new RelayCommand(() => { ShowChart = true; });
        ShowTableCommand   = new RelayCommand(() => { ShowChart = false; });
        ExportCsvCommand   = new RelayCommand(() => ExportRequested?.Invoke(this, EventArgs.Empty));
        ExportPngCommand   = new RelayCommand(() => ExportPngRequested?.Invoke(this, EventArgs.Empty));
        ToggleDmsCommand   = new RelayCommand(() => ShowDms = !ShowDms);

        _model.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(AstronomicalCyclesModel.HasResults)
                               or nameof(AstronomicalCyclesModel.SingleResults)
                               or nameof(AstronomicalCyclesModel.PairResults)
                               or nameof(AstronomicalCyclesModel.IsPairs))
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    OnPropertyChanged(nameof(HasResults));
                    OnPropertyChanged(nameof(TableRows));
                    OnPropertyChanged(nameof(ColumnHeaders));
                    OnPropertyChanged(nameof(PlotActions));
                });
            }
        };
    }

    public IRelayCommand ShowChartCommand { get; }
    public IRelayCommand ShowTableCommand { get; }
    public IRelayCommand ExportCsvCommand { get; }
    public IRelayCommand ExportPngCommand { get; }
    public IRelayCommand ToggleDmsCommand { get; }

    public event EventHandler? ExportRequested;
    public event EventHandler? ExportPngRequested;

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

    public Action<WpfPlot>? PlotActions => BuildPlotAction(0);

    public void ApplyPlot(WpfPlot plot) => BuildPlotAction(plot.ActualWidth)?.Invoke(plot);

    private Action<WpfPlot>? BuildPlotAction(double widthHint)
    {
        if (!_model.HasResults) return plot => { plot.Reset(); plot.Refresh(); };

        var isPairs   = _model.IsPairs;
        var pairs     = _model.FactorPairs.ToList();
        var pairData  = _model.PairResults.Select(s => s.ToList()).ToList();
        var singles   = _model.SingleResults.ToList();
        var coord     = _model.Coordinate;
        var isAngular = coord is Domain.Coordinates.Longitude or Domain.Coordinates.RightAscension;

        var allJds = isPairs
            ? pairData.SelectMany(s => s.Select(p => p.JulianDay))
            : singles.SelectMany(r => r.Series.Select(p => p.JulianDay));
        var spanDays = allJds.Any() ? allJds.Max() - allJds.Min() : 365;

        return plot =>
        {
            plot.Reset();

            if (File.Exists(GlyphFontPath))
                ScottPlot.Fonts.AddFontFile("EnigmaAstrology3", GlyphFontPath);

            var widthPx = widthHint > 0 ? widthHint : plot.ActualWidth;
            var dateAxis = plot.Plot.Axes.DateTimeTicksBottom();
            dateAxis.TickGenerator = PickTickInterval(spanDays, widthPx);
            dateAxis.TickLabelStyle.Rotation = -90;
            dateAxis.TickLabelStyle.Alignment = ScottPlot.Alignment.UpperRight;
            dateAxis.MinimumSize = 80;

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
                plot.Plot.ShowLegend();
            }
            else
            {
                // Collect first-point Y for each factor (for glyph placement).
                var glyphData = new List<(double Y, string Glyph)>();

                for (var i = 0; i < singles.Count; i++)
                {
                    var (factor, series) = singles[i];
                    if (series.Count == 0) continue;
                    var color = ScottPlot.Color.FromColor(SeriesColors[i % SeriesColors.Length]);
                    var glyph = GlyphSelector.GetGlyphForFactor(factor);

                    if (isAngular)
                    {
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
                            scatter.LegendText = string.Empty;
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
                        scatter.LegendText = string.Empty;
                        scatter.LineWidth  = 1.5f;
                        scatter.MarkerSize = 0;
                    }

                    if (series.Count > 0)
                        glyphData.Add((series[0].Position, glyph));
                }

                // Y-axis glyphs: place each factor glyph at its first-point Y value, left of the chart.
                // Use the same column-packing algorithm as EphemerisResultViewModel.
                if (glyphData.Count > 0)
                {
                    var allYs  = glyphData.Select(g => g.Y).ToList();
                    var yMin   = allYs.Min();
                    var yMax   = allYs.Max();
                    var yRange = Math.Max(yMax - yMin, 1.0);

                    const double glyphPxHeight = 20.0;
                    const double plotHeightPx  = 300.0;
                    var minGapData = yRange * glyphPxHeight / plotHeightPx;

                    // Column width in OADate units: ~1 day = 1 OADate unit.
                    // Glyph column width = ~2 days; keeps glyphs readable without crowding.
                    var glyphColWidthOa = Math.Max(spanDays * 0.015, 2.0);

                    // The leftmost X of the data in OADate.
                    var firstOa = singles
                        .Where(r => r.Series.Count > 0)
                        .Select(r => JdToDateTime(r.Series[0].JulianDay).ToOADate())
                        .DefaultIfEmpty(0.0)
                        .Min();
                    var xOrigin = firstOa - glyphColWidthOa * 0.5;

                    var sorted = glyphData
                        .OrderByDescending(g => g.Y)
                        .ToList();

                    var columns = new List<List<double>>();
                    var placed  = new List<(double Y, string Glyph, int Col)>();
                    foreach (var (y, glyph) in sorted)
                    {
                        int col = 0;
                        while (col < columns.Count && columns[col].Any(py => Math.Abs(py - y) < minGapData))
                            col++;
                        if (col == columns.Count) columns.Add([]);
                        columns[col].Add(y);
                        placed.Add((y, glyph, col));
                    }

                    int maxCol = placed.Count > 0 ? placed.Max(p => p.Col) : 0;

                    // Expand X-axis left to make room; keep right margin.
                    var lastOa = singles
                        .Where(r => r.Series.Count > 0)
                        .Select(r => JdToDateTime(r.Series[^1].JulianDay).ToOADate())
                        .DefaultIfEmpty(firstOa + 365)
                        .Max();
                    double xLeft = xOrigin - maxCol * glyphColWidthOa;
                    plot.Plot.Axes.SetLimitsX(xLeft, lastOa + glyphColWidthOa * 0.5);

                    // Hide numeric left-axis tick labels; glyphs serve as labels.
                    plot.Plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericManual();

                    foreach (var (y, glyph, col) in placed)
                    {
                        double x = xOrigin - col * glyphColWidthOa;
                        var txt                  = plot.Plot.Add.Text(glyph, x, y);
                        txt.LabelFontName        = "EnigmaAstrology3";
                        txt.LabelFontSize        = 16;
                        txt.Alignment            = ScottPlot.Alignment.MiddleRight;
                        txt.LabelFontColor       = ScottPlot.Colors.Black;
                        txt.LabelBackgroundColor = ScottPlot.Colors.Transparent;
                    }
                }

                plot.Plot.Legend.IsVisible = false;
            }

            plot.Refresh();
        };
    }

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

    public string LabelTabChart        => T("view.astrocycles.tab.chart");
    public string LabelTabPositions    => T("view.astrocycles.tab.positions");
    public string LabelNoResults       => T("view.astrocycles.chart.noresults");
    public string LabelExportPng       => T("view.astrocycles.chart.export");
    public string LabelExportCsv       => T("view.astrocycles.positions.export");
    public string LabelDate            => T("view.astrocycles.chart.date");
    public string LabelJulianDay       => T("view.astrocycles.positions.julianday");
    public string LabelFormatToggle    => _showDms
        ? T("view.astrocycles.positions.format.decimal")
        : T("view.astrocycles.positions.format.dms");

    public string BuildCsv()
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
        return sb.ToString();
    }

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

    private static DateTickGenerator PickTickInterval(double spanDays, double widthPx)
    {
        const double minTickSpacingPx = 80;
        var maxTicks = Math.Max(2, (int)(widthPx / minTickSpacingPx));

        (ScottPlot.TickGenerators.ITimeUnit maj, int majN,
         ScottPlot.TickGenerators.ITimeUnit min, int minN,
         Func<DateTime, DateTime> snap)[] ladder =
        [
            (new ScottPlot.TickGenerators.TimeUnits.Day(),    1,  new ScottPlot.TickGenerators.TimeUnits.Day(),    1,  dt => dt.Date),
            (new ScottPlot.TickGenerators.TimeUnits.Day(),    7,  new ScottPlot.TickGenerators.TimeUnits.Day(),    1,  dt => dt.Date.AddDays(-(int)dt.DayOfWeek)),
            (new ScottPlot.TickGenerators.TimeUnits.Day(),   14,  new ScottPlot.TickGenerators.TimeUnits.Day(),    7,  dt => dt.Date.AddDays(-(int)dt.DayOfWeek)),
            (new ScottPlot.TickGenerators.TimeUnits.Month(),  1,  new ScottPlot.TickGenerators.TimeUnits.Day(),    7,  dt => new DateTime(dt.Year, dt.Month, 1)),
            (new ScottPlot.TickGenerators.TimeUnits.Month(),  2,  new ScottPlot.TickGenerators.TimeUnits.Month(),  1,  dt => new DateTime(dt.Year, dt.Month, 1)),
            (new ScottPlot.TickGenerators.TimeUnits.Month(),  3,  new ScottPlot.TickGenerators.TimeUnits.Month(),  1,  dt => new DateTime(dt.Year, dt.Month, 1)),
            (new ScottPlot.TickGenerators.TimeUnits.Month(),  6,  new ScottPlot.TickGenerators.TimeUnits.Month(),  1,  dt => new DateTime(dt.Year, dt.Month, 1)),
            (new ScottPlot.TickGenerators.TimeUnits.Year(),   1,  new ScottPlot.TickGenerators.TimeUnits.Month(),  3,  dt => new DateTime(dt.Year, 1, 1)),
            (new ScottPlot.TickGenerators.TimeUnits.Year(),   2,  new ScottPlot.TickGenerators.TimeUnits.Month(),  6,  dt => new DateTime(dt.Year, 1, 1)),
            (new ScottPlot.TickGenerators.TimeUnits.Year(),   5,  new ScottPlot.TickGenerators.TimeUnits.Year(),   1,  dt => new DateTime(dt.Year, 1, 1)),
            (new ScottPlot.TickGenerators.TimeUnits.Year(),  10,  new ScottPlot.TickGenerators.TimeUnits.Year(),   2,  dt => new DateTime(dt.Year, 1, 1)),
            (new ScottPlot.TickGenerators.TimeUnits.Year(),  20,  new ScottPlot.TickGenerators.TimeUnits.Year(),   5,  dt => new DateTime(dt.Year, 1, 1)),
        ];

        double[] daysPerStep = [1, 7, 14, 30, 60, 91, 182, 365, 730, 1825, 3650, 7300];

        for (var i = 0; i < ladder.Length; i++)
        {
            if (spanDays / daysPerStep[i] <= maxTicks)
            {
                var (maj, majN, min, minN, snap) = ladder[i];
                return new DateTickGenerator(maj, majN, min, minN, snap);
            }
        }

        var last = ladder[^1];
        return new DateTickGenerator(last.maj, last.majN, last.min, last.minN, last.snap);
    }

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
