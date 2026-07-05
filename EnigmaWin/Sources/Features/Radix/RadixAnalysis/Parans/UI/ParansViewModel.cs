// ParansViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Serilog;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Parans.UI;

public sealed record ParanTimesRow(
    string BodyGlyph, string BodyName,
    string Rising, string Setting, string Culmination, string AntiCulmination,
    bool IsEvenRow);

public sealed record ParanMatchRow(
    string Body1Glyph, string Body1Name, string Type1, string Time1,
    string Body2Glyph, string Body2Name, string Type2, string Time2,
    string Orb, bool IsEvenRow);

public sealed class ParansViewModel : INotifyPropertyChanged
{
    private readonly IRosetta              _rosetta;
    private readonly IChartSession         _chartSession;
    private readonly IConfigContext        _configContext;
    private readonly IHoroscopeRepository  _horoscopeRepository;

    private int _orbMin = 10;
    public int OrbMin
    {
        get => _orbMin;
        set { if (_orbMin == value) return; _orbMin = Math.Max(0, value); OnPropertyChanged(); }
    }

    private int _orbSec = 0;
    public int OrbSec
    {
        get => _orbSec;
        set { if (_orbSec == value) return; _orbSec = Math.Clamp(value, 0, 59); OnPropertyChanged(); }
    }

    private double OrbMinutes => _orbMin + _orbSec / 60.0;

    public ObservableCollection<ParanTimesRow> PositionRows { get; } = new();
    public ObservableCollection<ParanMatchRow> MatchRows    { get; } = new();

    private bool _hasData;
    public bool HasData
    {
        get => _hasData;
        private set { if (_hasData == value) return; _hasData = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoData)); }
    }
    public bool HasNoData => !HasData;

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        private set { _statusMessage = value; OnPropertyChanged(); }
    }

    public ParansViewModel(
        IRosetta rosetta,
        IConfigContext configContext,
        IChartSession chartSession,
        IHoroscopeRepository horoscopeRepository)
    {
        _rosetta              = rosetta;
        _configContext        = configContext;
        _chartSession         = chartSession;
        _horoscopeRepository  = horoscopeRepository;

        var config = _configContext.ActiveConfig;
        var totalSec = (int)(config.FixStarConfig.ParanTimeOrb * 60.0);
        _orbMin = totalSec / 60;
        _orbSec = totalSec % 60;
        if (_orbMin == 0 && _orbSec == 0) _orbMin = 10;

        if (chartSession is INotifyPropertyChanged notify)
            notify.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(IChartSession.SelectedChart))
                    OnPropertyChanged(nameof(StatusMessage));
            };
    }

    public void Calculate()
    {
        PositionRows.Clear();
        MatchRows.Clear();
        HasData = false;

        var chart = _chartSession.SelectedChart;
        var selected = _chartSession.Selected;
        if (chart is null || selected is null)
        {
            StatusMessage = LabelNoChart;
            return;
        }

        try
        {
            var geoLon = 0.0;
            var geoLat = 0.0;

            var horoscopes = _horoscopeRepository.FetchAllAsync().GetAwaiter().GetResult();
            var horoscope = horoscopes.FirstOrDefault(h => h.Name == selected.Name);
            if (horoscope != null)
            {
                geoLon = horoscope.Longitude ?? 0.0;
                geoLat = horoscope.Latitude  ?? 0.0;
            }

            var config = _configContext.ActiveConfig;
            var result = ParansOrchestrator.Calculate(
                chart,
                geoLon, geoLat, 0.0,
                config.FactorConfig,
                config.FixStarConfig,
                config.CalculationConfig,
                Math.Max(1.0 / 60.0, OrbMinutes));

            if (result.AllTimes.Count == 0)
            {
                StatusMessage = LabelNoResults;
                return;
            }

            var posIndex = 0;
            foreach (var entry in result.AllTimes)
            {
                PositionRows.Add(new ParanTimesRow(
                    BodyGlyph:       entry.Kind == ParanBodyKind.Factor && entry.Factor.HasValue
                                         ? GlyphSelector.GetGlyphForFactor(entry.Factor.Value) : string.Empty,
                    BodyName:        entry.DisplayName,
                    Rising:          FormatTime(entry.Rising),
                    Setting:         FormatTime(entry.Setting),
                    Culmination:     FormatTime(entry.Culmination),
                    AntiCulmination: FormatTime(entry.AntiCulmination),
                    IsEvenRow:       posIndex++ % 2 == 0));
            }

            var matchIndex = 0;
            foreach (var match in result.Matches)
            {
                MatchRows.Add(new ParanMatchRow(
                    Body1Glyph: match.Body1.Kind == ParanBodyKind.Factor && match.Body1.Factor.HasValue
                                    ? GlyphSelector.GetGlyphForFactor(match.Body1.Factor.Value) : string.Empty,
                    Body1Name:  match.Body1.DisplayName,
                    Type1:      TypeLabel(match.Type1),
                    Time1:      FormatTime(match.Time1),
                    Body2Glyph: match.Body2.Kind == ParanBodyKind.Factor && match.Body2.Factor.HasValue
                                    ? GlyphSelector.GetGlyphForFactor(match.Body2.Factor.Value) : string.Empty,
                    Body2Name:  match.Body2.DisplayName,
                    Type2:      TypeLabel(match.Type2),
                    Time2:      FormatTime(match.Time2),
                    Orb:        FormatOrb(match.OrbMinutes),
                    IsEvenRow:  matchIndex++ % 2 == 0));
            }

            StatusMessage = string.Empty;
            HasData = true;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Parans calculation failed.");
            StatusMessage = ex.Message;
            HasData = false;
        }
    }

    private string TypeLabel(ParanType t) => t switch
    {
        ParanType.Rising          => LabelTypeRising,
        ParanType.Setting         => LabelTypeSetting,
        ParanType.Culmination     => LabelTypeCulmination,
        ParanType.AntiCulmination => LabelTypeAntiCulm,
        _                         => string.Empty
    };

    private static string FormatTime(double? jd)
    {
        if (jd is null) return "—";
        var dt = SEWrapper.DateFromJulianDay(jd.Value, true);
        return $"{dt.Time.Hour:D2}:{dt.Time.Minute:D2}:{dt.Time.Second:D2}";
    }

    private static string FormatTime(double jd)
    {
        var dt = SEWrapper.DateFromJulianDay(jd, true);
        return $"{dt.Time.Hour:D2}:{dt.Time.Minute:D2}:{dt.Time.Second:D2}";
    }

    private static string FormatOrb(double minutes)
    {
        var wholeMin = (int)minutes;
        var secs     = (int)((minutes - wholeMin) * 60.0);
        return $"{wholeMin}'{secs:D2}\"";
    }

    // i18n labels
    public string LabelTitle           => _rosetta.GetText(RbFile.RadixParans, "view.parans.title");
    public string LabelOrbSection      => _rosetta.GetText(RbFile.RadixParans, "view.parans.orb.section");
    public string LabelOrbLabel        => _rosetta.GetText(RbFile.RadixParans, "view.parans.orb.label");
    public string LabelCalculate       => _rosetta.GetText(RbFile.RadixParans, "view.parans.calculate");
    public string LabelNoChart         => _rosetta.GetText(RbFile.RadixParans, "view.parans.nochart");
    public string LabelNoResults       => _rosetta.GetText(RbFile.RadixParans, "view.parans.noresults");
    public string LabelNoMatches       => _rosetta.GetText(RbFile.RadixParans, "view.parans.nomatches");
    public string LabelTabPositions    => _rosetta.GetText(RbFile.RadixParans, "view.parans.tab.positions");
    public string LabelTabMatches      => _rosetta.GetText(RbFile.RadixParans, "view.parans.tab.matches");
    public string LabelColName         => _rosetta.GetText(RbFile.RadixParans, "view.parans.col.name");
    public string LabelColRising       => _rosetta.GetText(RbFile.RadixParans, "view.parans.col.rising");
    public string LabelColSetting      => _rosetta.GetText(RbFile.RadixParans, "view.parans.col.setting");
    public string LabelColCulmination  => _rosetta.GetText(RbFile.RadixParans, "view.parans.col.culmination");
    public string LabelColAntiCulm     => _rosetta.GetText(RbFile.RadixParans, "view.parans.col.anticulm");
    public string LabelColBody1        => _rosetta.GetText(RbFile.RadixParans, "view.parans.col.body1");
    public string LabelColType1        => _rosetta.GetText(RbFile.RadixParans, "view.parans.col.type1");
    public string LabelColBody2        => _rosetta.GetText(RbFile.RadixParans, "view.parans.col.body2");
    public string LabelColType2        => _rosetta.GetText(RbFile.RadixParans, "view.parans.col.type2");
    public string LabelColTime         => _rosetta.GetText(RbFile.RadixParans, "view.parans.col.time");
    public string LabelColOrb          => _rosetta.GetText(RbFile.RadixParans, "view.parans.col.orb");
    public string LabelTypeRising      => _rosetta.GetText(RbFile.RadixParans, "view.parans.type.rising");
    public string LabelTypeSetting     => _rosetta.GetText(RbFile.RadixParans, "view.parans.type.setting");
    public string LabelTypeCulmination => _rosetta.GetText(RbFile.RadixParans, "view.parans.type.culmination");
    public string LabelTypeAntiCulm    => _rosetta.GetText(RbFile.RadixParans, "view.parans.type.anticulm");
    public string TooltipFactsheet     => _rosetta.GetText(RbFile.RadixParans, "view.parans.tooltip.factsheet");
    public string TooltipHelp          => _rosetta.GetText(RbFile.RadixParans, "view.parans.tooltip.help");

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
