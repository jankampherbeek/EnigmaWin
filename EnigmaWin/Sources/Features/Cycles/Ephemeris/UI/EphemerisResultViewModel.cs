// EphemerisResultViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using ScottPlot.WPF;

namespace EnigmaWin.Sources.Features.Cycles.Ephemeris.UI;

public sealed class EphemerisResultViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly EphemerisModel _model;

    private bool _showTabular = true;

    // Path to the glyph font TTF, resolved once at startup relative to the exe.
    private static readonly string GlyphFontPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "Resources", "Fonts", "EnigmaAstrology3.ttf");

    private static readonly System.Drawing.Color[] SeriesColors =
    [
        System.Drawing.Color.SteelBlue,  System.Drawing.Color.OrangeRed,  System.Drawing.Color.SeaGreen,
        System.Drawing.Color.DarkOrchid, System.Drawing.Color.Goldenrod,  System.Drawing.Color.Teal,
        System.Drawing.Color.Crimson,    System.Drawing.Color.SlateBlue,  System.Drawing.Color.DarkOliveGreen,
        System.Drawing.Color.Peru,       System.Drawing.Color.DodgerBlue, System.Drawing.Color.HotPink
    ];

    public EphemerisResultViewModel(IRosetta rosetta, EphemerisModel model)
    {
        _rosetta = rosetta;
        _model   = model;

        _model.PropertyChanged += (_, _) =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(HasResults));
                OnPropertyChanged(nameof(TableColumns));
                OnPropertyChanged(nameof(TableRows));
                OnPropertyChanged(nameof(PlotActions));
                OnPropertyChanged(nameof(ChartTitle));
                OnPropertyChanged(nameof(TableTitle));
                OnPropertyChanged(nameof(LegendItems));
            });
        };
    }

    // ── Tab switching ─────────────────────────────────────────────────────────

    public bool ShowTabular
    {
        get => _showTabular;
        set { _showTabular = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowGraphic)); }
    }
    public bool ShowGraphic => !_showTabular;

    public void SelectTabular() => ShowTabular = true;
    public void SelectGraphic() => ShowTabular = false;

    // ── Results ───────────────────────────────────────────────────────────────

    public bool HasResults => _model.HasResults;

    // ── Titles ────────────────────────────────────────────────────────────────

    public string ChartTitle => HasResults ? BuildTitle() : string.Empty;
    public string TableTitle => ChartTitle;

    private string BuildTitle()
    {
        if (_model.Rows.Count == 0) return string.Empty;
        var first      = _model.Rows[0];
        var last       = _model.Rows[^1];
        var coordLabel = CoordLabel(_model.SelectedCoord);
        var dateRange  = $"{first.Day:D2}/{first.Month:D2}/{first.Year} – {last.Day:D2}/{last.Month:D2}/{last.Year}";
        var ayanLabel  = _rosetta.GetText(RbFile.Localizable, _model.Ayanamsha.LocalizedName());
        var obsLabel   = _rosetta.GetText(RbFile.Localizable,
            (_model.IsHeliocentric ? ObserverPositions.Heliocentric : ObserverPositions.Geocentric).LocalizedName());
        return $"{coordLabel}  |  {dateRange}  |  {ayanLabel}  |  {obsLabel}";
    }

    // ── Tabular columns ───────────────────────────────────────────────────────

    public List<EphemerisColumn> TableColumns
    {
        get
        {
            var coord = _model.SelectedCoord;
            var coordLabel = CoordLabel(coord);
            return _model.Factors
                .Select(f => new EphemerisColumn($"{FactorName(f)} {coordLabel}", coord, f))
                .ToList();
        }
    }

    public List<EphemerisTableRow> TableRows
    {
        get
        {
            if (_model.Rows.Count == 0) return [];
            var cols = TableColumns;
            return _model.Rows.Select((r, idx) =>
            {
                var cells = cols.Select(c =>
                {
                    if (!r.Positions.TryGetValue(c.Factor, out var pos)) return new EphemerisTableCell("—", string.Empty);
                    return c.Coord switch
                    {
                        EphemerisCoordinate.Longitude        => FormatLongitude(pos.Longitude),
                        EphemerisCoordinate.Latitude         => new EphemerisTableCell(FormatSexagesimal(pos.Latitude), string.Empty),
                        EphemerisCoordinate.RightAscension   => new EphemerisTableCell(FormatSexagesimal(pos.RightAscension), string.Empty),
                        EphemerisCoordinate.Declination      => new EphemerisTableCell(FormatSexagesimal(pos.Declination), string.Empty),
                        EphemerisCoordinate.Distance         => new EphemerisTableCell($"{pos.Distance:F6}", string.Empty),
                        EphemerisCoordinate.SpeedLongitude   => new EphemerisTableCell(FormatSexagesimal(pos.SpeedLongitude), string.Empty),
                        EphemerisCoordinate.SpeedDeclination => new EphemerisTableCell(FormatSexagesimal(pos.SpeedDeclination), string.Empty),
                        _                                    => new EphemerisTableCell("—", string.Empty)
                    };
                }).ToList();
                return new EphemerisTableRow($"{r.Day:D2}", cells, isEven: idx % 2 == 0);
            }).ToList();
        }
    }

    // ── Graphic (ScottPlot) ───────────────────────────────────────────────────

    public Action<WpfPlot>? PlotActions => BuildPlotAction();

    public void ApplyPlot(WpfPlot plot) => BuildPlotAction()?.Invoke(plot);

    private Action<WpfPlot>? BuildPlotAction()
    {
        if (!_model.HasResults || _model.Rows.Count == 0)
            return plot => { plot.Reset(); plot.Refresh(); };

        var rows    = _model.Rows.ToList();
        var factors = _model.Factors.ToList();
        var coord   = _model.SelectedCoord;

        return plot =>
        {
            plot.Reset();

            if (File.Exists(GlyphFontPath))
                ScottPlot.Fonts.AddFontFile("EnigmaAstrology3", GlyphFontPath);

            // Collect per-factor data and raw y-tick positions.
            var factorData = new List<(Factors Factor, double[] Xs, double[] Ys, string Glyph)>();
            var colorIdx   = 0;

            foreach (var f in factors)
            {
                var xs    = rows.Select(r => (double)r.Day).ToArray();
                var ys    = rows.Select(r => r.Positions.TryGetValue(f, out var p) ? GetValue(p, coord) : 0.0).ToArray();
                var glyph = GlyphSelector.GetGlyphForFactor(f);
                factorData.Add((f, xs, ys, glyph));

                if (coord == EphemerisCoordinate.Longitude)
                    AddSegmented(plot, xs, ys, glyph, colorIdx++);
                else
                {
                    var color   = ScottPlot.Color.FromColor(SeriesColors[colorIdx++ % SeriesColors.Length]);
                    var scatter = plot.Plot.Add.Scatter(xs, ys, color);
                    scatter.LegendText = glyph;
                    scatter.LineWidth  = 1.5f;
                    scatter.MarkerSize = 0;
                }
            }

            // Y-axis glyphs: draw each factor's glyph as an Add.Text at its true day-1 Y value.
            // When multiple glyphs land within minGapData of each other in Y, place them side-by-side
            // (different X columns, same row) rather than spreading them vertically.
            var allYs      = factorData.SelectMany(fd => fd.Ys).ToList();
            var yMin       = allYs.Count > 0 ? allYs.Min() : 0;
            var yMax       = allYs.Count > 0 ? allYs.Max() : 1;
            var yRange     = Math.Max(yMax - yMin, 1.0);
            const double glyphPxHeight = 20.0;
            const double plotHeightPx  = 300.0;
            var minGapData = yRange * glyphPxHeight / plotHeightPx;

            // Each glyph sits at x = 0.5 (just left of day 1), shifted further left by its column index.
            // Column width in data units: each glyph is ~16px wide; X range is ~31 days.
            // We reserve glyphColWidth per column; xOrigin is where column 0 sits.
            const double glyphColWidth = 1.2;
            const double xOrigin       = 0.5; // column 0 anchor (left of day 1)

            // Sort by true day-1 Y descending, then assign horizontal column to avoid vertical overlap.
            var glyphEntries = factorData
                .Where(fd => fd.Ys.Length > 0)
                .Select(fd => (Y: fd.Ys[0], fd.Glyph))
                .OrderByDescending(t => t.Y)
                .ToList();

            // Assign each glyph to the leftmost column where its Y clears all previously placed glyphs.
            var columns = new List<List<double>>(); // per column: list of placed Y values
            var placed  = new List<(double Y, string Glyph, int Col)>();
            foreach (var (y, glyph) in glyphEntries)
            {
                int col = 0;
                while (col < columns.Count && columns[col].Any(py => Math.Abs(py - y) < minGapData))
                    col++;
                if (col == columns.Count) columns.Add([]);
                columns[col].Add(y);
                placed.Add((y, glyph, col));
            }

            int maxCol = placed.Count > 0 ? placed.Max(p => p.Col) : 0;

            // Expand X axis left to make room for the glyph columns; keep right margin.
            double xLeft = xOrigin - (maxCol + 1) * glyphColWidth;
            plot.Plot.Axes.SetLimitsX(xLeft, rows.Count + 0.5);

            // Hide the numeric left-axis tick labels; the Add.Text glyphs serve as labels.
            plot.Plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericManual();

            foreach (var (y, glyph, col) in placed)
            {
                double x = xOrigin - col * glyphColWidth;
                var txt                  = plot.Plot.Add.Text(glyph, x, y);
                txt.LabelFontName        = "EnigmaAstrology3";
                txt.LabelFontSize        = 16;
                txt.Alignment            = ScottPlot.Alignment.MiddleRight;
                txt.LabelFontColor       = ScottPlot.Colors.Black;
                txt.LabelBackgroundColor = ScottPlot.Colors.Transparent;
            }

            // Legend is rendered as a WPF WrapPanel below the chart (LegendItems property).
            plot.Plot.Legend.IsVisible = false;

            plot.Plot.XLabel(LabelDay);
            plot.Refresh();
        };
    }

    private static double GetValue(EphemerisPositions p, EphemerisCoordinate coord) => coord switch
    {
        EphemerisCoordinate.Longitude        => p.Longitude,
        EphemerisCoordinate.Latitude         => p.Latitude,
        EphemerisCoordinate.RightAscension   => p.RightAscension,
        EphemerisCoordinate.Declination      => p.Declination,
        EphemerisCoordinate.Distance         => p.Distance,
        EphemerisCoordinate.SpeedLongitude   => p.SpeedLongitude,
        EphemerisCoordinate.SpeedDeclination => p.SpeedDeclination,
        _                                    => 0.0
    };

    private void AddSegmented(WpfPlot plot, double[] xs, double[] ys, string glyph, int colorIdx)
    {
        var color    = ScottPlot.Color.FromColor(SeriesColors[colorIdx % SeriesColors.Length]);
        var segStart = 0;
        for (var j = 1; j <= ys.Length; j++)
        {
            var isBreak = j == ys.Length || Math.Abs(ys[j] - ys[j - 1]) > 180.0;
            if (!isBreak) continue;
            var segXs   = xs.Skip(segStart).Take(j - segStart).ToArray();
            var segYs   = ys.Skip(segStart).Take(j - segStart).ToArray();
            var scatter = plot.Plot.Add.Scatter(segXs, segYs, color);
            scatter.LegendText = segStart == 0 ? glyph : string.Empty;
            scatter.LineWidth  = 1.5f;
            scatter.MarkerSize = 0;
            segStart = j;
        }
    }

    // ── WPF legend items (glyph + line color per factor) ─────────────────────

    public List<EphemerisLegendItem> LegendItems =>
        _model.Factors
            .Select((f, i) =>
            {
                var color = SeriesColors[i % SeriesColors.Length];
                var hex   = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                return new EphemerisLegendItem(GlyphSelector.GetGlyphForFactor(f), hex);
            })
            .ToList();

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelTabTabular  => T("view.ephemeris.tab.tabular");
    public string LabelTabGraphic  => T("view.ephemeris.tab.graphic");
    public string LabelNoResults   => T("view.ephemeris.noresults");
    public string LabelExportPdf   => T("view.ephemeris.export.pdf");
    public string LabelExportPng   => T("view.ephemeris.export.png");
    public string LabelDay         => T("view.ephemeris.day");

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string FactorName(Factors f) => _rosetta.GetText(RbFile.Localizable, f.LocalizedName());

    private string CoordLabel(EphemerisCoordinate coord) => coord switch
    {
        EphemerisCoordinate.Longitude        => T("view.ephemeris.col.lon"),
        EphemerisCoordinate.Latitude         => T("view.ephemeris.col.lat"),
        EphemerisCoordinate.RightAscension   => T("view.ephemeris.col.ra"),
        EphemerisCoordinate.Declination      => T("view.ephemeris.col.dec"),
        EphemerisCoordinate.Distance         => T("view.ephemeris.col.dist"),
        EphemerisCoordinate.SpeedLongitude   => T("view.ephemeris.col.vlon"),
        EphemerisCoordinate.SpeedDeclination => T("view.ephemeris.col.vdec"),
        _                                    => string.Empty
    };

    private static EphemerisTableCell FormatLongitude(double value)
    {
        var (dmsString, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(value);
        if (!ok || sign == null) return new EphemerisTableCell(PositionInDegreesConversion.DoubleToDms(value), string.Empty);
        return new EphemerisTableCell(dmsString, GlyphSelector.GetGlyphForSign(sign.Value));
    }

    private static string FormatSexagesimal(double value) =>
        PositionInDegreesConversion.DoubleToDms(value);

    private string T(string key) => _rosetta.GetText(RbFile.Ephemeris, key);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// ── Supporting types ──────────────────────────────────────────────────────────

public sealed class EphemerisColumn(string header, EphemerisCoordinate coord, Factors factor)
{
    public string             Header { get; } = header;
    public EphemerisCoordinate Coord { get; } = coord;
    public Factors             Factor { get; } = factor;
}

/// <summary>One table cell. SignGlyph is non-empty only for longitude (rendered with GlyphFont).</summary>
public sealed class EphemerisTableCell(string dmsText, string signGlyph)
{
    public string DmsText   { get; } = dmsText;
    public string SignGlyph { get; } = signGlyph;
}

public sealed class EphemerisTableRow(string dayText, List<EphemerisTableCell> cells, bool isEven)
{
    public string                   DayText { get; } = dayText;
    public List<EphemerisTableCell> Cells   { get; } = cells;
    public bool                     IsEven  { get; } = isEven;
}

/// <summary>One item in the WPF legend strip: a factor glyph and its line color as a hex string.</summary>
public sealed class EphemerisLegendItem(string glyph, string colorHex)
{
    public string Glyph    { get; } = glyph;
    public string ColorHex { get; } = colorHex;
}
