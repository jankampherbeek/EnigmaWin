// FixStarsViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Serilog;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.FixStars.UI;

public sealed record FixStarPositionRow(string DisplayName, string LongitudeDms, string LongitudeGlyph, string Latitude, string Declination);
public sealed record FixStarMatchRow(string StarName, string FactorGlyph, string MatchType, string Orb);

public sealed class FixStarsViewModel : INotifyPropertyChanged
{
    private readonly IRosetta       _rosetta;
    private readonly IChartSession  _chartSession;
    private readonly IConfigContext _configContext;
    private FullChart?              _currentChart;

    private int _orbDeg = 1;
    public int OrbDeg
    {
        get => _orbDeg;
        set { if (_orbDeg == value) return; _orbDeg = Math.Max(0, value); OnPropertyChanged(); }
    }

    private int _orbMin = 0;
    public int OrbMin
    {
        get => _orbMin;
        set { if (_orbMin == value) return; _orbMin = Math.Clamp(value, 0, 59); OnPropertyChanged(); }
    }

    private double OrbOverride => _orbDeg + _orbMin / 60.0;

    public ObservableCollection<FixStarPositionRow> PositionRows { get; } = new();
    public ObservableCollection<FixStarMatchRow>    MatchRows    { get; } = new();

    private bool _hasData;
    public bool HasData   { get => _hasData;   private set { if (_hasData == value) return; _hasData = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoData)); } }
    public bool HasNoData => !HasData;

    private string _statusMessage = string.Empty;
    public string StatusMessage { get => _statusMessage; private set { _statusMessage = value; OnPropertyChanged(); } }

    public FixStarsViewModel(IRosetta rosetta, IConfigContext configContext, IChartSession chartSession)
    {
        _rosetta       = rosetta;
        _configContext = configContext;
        _chartSession  = chartSession;
        _currentChart  = chartSession.SelectedChart;

        if (chartSession is INotifyPropertyChanged notify)
            notify.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != nameof(IChartSession.SelectedChart)) return;
                _currentChart = _chartSession.SelectedChart;
            };
    }

    public void Calculate()
    {
        PositionRows.Clear();
        MatchRows.Clear();
        HasData = false;

        if (_currentChart is null)
        {
            StatusMessage = LabelNoChart;
            return;
        }

        var config = _configContext.ActiveConfig;
        var selectedStars = config.FixStarConfig.FixStarSettings.Where(s => s.IsUsed).ToList();
        if (selectedStars.Count == 0)
        {
            StatusMessage = LabelNoStarsSelected;
            return;
        }

        try
        {
            var calcConfig = config.CalculationConfig;
            var starNames  = selectedStars.Select(s => s.FixStar.AstronomicalName).ToList();
            var results    = FixStarsOrchestrator.GetFixStarSet(starNames, _currentChart.JulianDay,
                calcConfig.ObserverPosition, calcConfig.Ayanamsha);

            if (results.Count == 0)
            {
                StatusMessage = LabelNoResults;
                return;
            }

            var starByAstronomical = StarDefinitions.AllCases
                .ToDictionary(s => s.AstronomicalName, s => s);

            var displayNameByAstronomical = new Dictionary<string, string>();
            var sortedResults = results
                .Select(r =>
                {
                    string dn;
                    if (starByAstronomical.TryGetValue(r.StarName, out var def))
                    {
                        var parts = new[] { def.Name }.Concat(def.Aliases).Where(a => !string.IsNullOrEmpty(a));
                        dn = string.Join(", ", parts) + $" ({r.StarName})";
                    }
                    else
                    {
                        dn = r.StarName;
                    }
                    displayNameByAstronomical[r.StarName] = dn;
                    return (dn, r);
                })
                .OrderBy(x => x.dn)
                .ToList();

            foreach (var (dn, r) in sortedResults)
            {
                var (lonDms, sign, ok) = Features.Shared.Conversion.PositionInDegreesConversion.DoubleToDmsSign(r.Longitude);
                var dmsText   = ok ? lonDms : Features.Shared.Conversion.PositionInDegreesConversion.DoubleToDms(r.Longitude);
                var glyphText = ok && sign.HasValue ? SignGlyph(sign.Value) : string.Empty;
                PositionRows.Add(new FixStarPositionRow(
                    DisplayName:    dn,
                    LongitudeDms:   dmsText,
                    LongitudeGlyph: glyphText,
                    Latitude:       Features.Shared.Conversion.PositionInDegreesConversion.DoubleToDms(r.Latitude),
                    Declination:    Features.Shared.Conversion.PositionInDegreesConversion.DoubleToDms(r.Declination)));
            }

            var usedFactors = config.FactorConfig.Settings.Where(f => f.IsUsed).Select(f => f.Factor).ToList();
            var parallelOrb = config.OrbConfig.ParallelOrb;

            var matches = new List<(double orb, FixStarMatchRow row)>();
            foreach (var (_, r) in sortedResults)
            {
                var starDn = displayNameByAstronomical[r.StarName];
                foreach (var factor in usedFactors)
                {
                    if (!_currentChart.Coordinates.TryGetValue(factor, out var pos)) continue;
                    if (pos.Ecliptical.Length == 0 || pos.Equatorial.Length == 0) continue;

                    var lonDiff = AngularDifference(r.Longitude, pos.Ecliptical[0].MainPos);
                    if (lonDiff <= OrbOverride)
                    {
                        matches.Add((lonDiff, new FixStarMatchRow(
                            StarName:   starDn,
                            FactorGlyph: FactorGlyph(factor),
                            MatchType:  LabelMatchConjunction,
                            Orb:        Features.Shared.Conversion.PositionInDegreesConversion.DoubleToDms(lonDiff))));
                    }

                    var declDiff = Math.Abs(r.Declination - pos.Equatorial[0].Deviation);
                    if (declDiff <= parallelOrb)
                    {
                        matches.Add((declDiff, new FixStarMatchRow(
                            StarName:   starDn,
                            FactorGlyph: FactorGlyph(factor),
                            MatchType:  LabelMatchParallel,
                            Orb:        Features.Shared.Conversion.PositionInDegreesConversion.DoubleToDms(declDiff))));
                    }
                }
            }

            foreach (var (_, row) in matches.OrderBy(m => m.orb))
                MatchRows.Add(row);

            StatusMessage = string.Empty;
            HasData = true;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "FixStars calculation failed.");
            StatusMessage = ex.Message;
            HasData = false;
        }
    }

    private static double AngularDifference(double a, double b)
    {
        var diff = Math.Abs(a - b);
        if (diff > 180) diff = 360 - diff;
        return diff;
    }

    private static string SignGlyph(Signs sign) => GlyphSelector.GetGlyphForSign(sign);

    private static string FactorGlyph(Factors factor)
    {
        return GlyphSelector.GetGlyphForFactor(factor);
    }

    // i18n labels
    public string LabelTitle            => _rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.title");
    public string LabelOrbSection       => _rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.orbsection");
    public string LabelOrbLabel         => _rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.orblabel");
    public string LabelCalculate        => _rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.calculate");
    public string LabelNoChart          => _rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.nochart");
    public string LabelNoResults        => _rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.noresults");
    public string LabelNoStarsSelected  => _rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.nostarsselected");
    public string LabelTabPositions     => _rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.tab.positions");
    public string LabelTabMatches       => _rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.tab.matches");
    public string LabelColName          => _rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.col.name");
    public string LabelColLongitude     => _rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.col.longitude");
    public string LabelColLatitude      => _rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.col.latitude");
    public string LabelColDeclination   => _rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.col.declination");
    public string LabelColFactor        => _rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.col.factor");
    public string LabelColType          => _rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.col.type");
    public string LabelColOrb           => _rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.col.orb");
    public string LabelMatchConjunction => _rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.match.conjunction");
    public string LabelMatchParallel    => _rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.match.parallel");

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
