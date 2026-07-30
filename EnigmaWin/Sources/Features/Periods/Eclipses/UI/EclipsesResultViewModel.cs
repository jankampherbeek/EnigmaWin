// EclipsesResultViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Periods.Eclipses.UI;

public sealed class EclipsesResultViewModel : INotifyPropertyChanged
{
    private const double NoSarosValue = -99999998;
    private const string SolarEclipseGlyph = "";
    private const string LunarEclipseGlyph = "";

    private readonly IRosetta _rosetta;
    private readonly EclipsesModel _model;

    public EclipsesResultViewModel(IRosetta rosetta, EclipsesModel model)
    {
        _rosetta = rosetta;
        _model   = model;

        _model.PropertyChanged += (_, _) =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(HasResults));
                OnPropertyChanged(nameof(HasLocation));
                OnPropertyChanged(nameof(Rows));
            });
        };
    }

    public bool HasResults   => _model.HasResults;
    public bool HasLocation  => _model.HasLocation;

    public List<EclipseResultRow> Rows =>
        _model.Results.Select(ToRow).ToList();

    private EclipseResultRow ToRow(EclipseEvent e)
    {
        var dt = SEWrapper.DateFromJulianDay(e.DisplayJD, gregorian: true);
        var hour = (int)dt.Time.HourDecimal;
        var minute = (int)((dt.Time.HourDecimal - hour) * 60);
        var dateText = $"{dt.Date.Year}-{dt.Date.Month:00}-{dt.Date.Day:00} {hour:00}:{minute:00}";

        var (dmsText, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(e.Longitude);
        var signGlyph = ok && sign.HasValue ? GlyphSelector.GetGlyphForSign(sign.Value) : string.Empty;
        var posText = ok ? dmsText : PositionInDegreesConversion.DoubleToDms(e.Longitude);

        var eclipseGlyph = e.Kind == EclipseKind.Solar ? SolarEclipseGlyph : LunarEclipseGlyph;
        var typeLabel = TypeLabel(e);
        var sarosText = e.SarosNumber > NoSarosValue
            ? $"{(int)e.SarosNumber}-{(int)e.SarosMemberNumber}"
            : "—";

        return new EclipseResultRow(
            eclipseGlyph: eclipseGlyph,
            dateText: dateText,
            positionText: posText,
            signGlyph: signGlyph,
            typeText: typeLabel,
            hasLocalData: e.HasLocalData,
            isVisible: e.IsVisible,
            sarosText: sarosText);
    }

    private string TypeLabel(EclipseEvent e)
    {
        if (e.IsTotal)     return T("view.eclipses.results.type.total");
        if (e.IsHybrid)    return T("view.eclipses.results.type.hybrid");
        if (e.IsAnnular)   return T("view.eclipses.results.type.annular");
        if (e.IsPenumbral) return T("view.eclipses.results.type.penumbral");
        if (e.IsPartial)   return T("view.eclipses.results.type.partial");
        return "—";
    }

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelResultsTitle => T("view.eclipses.results.title");
    public string LabelNoResults    => T("view.eclipses.results.noresults");
    public string LabelExportPdf    => T("view.eclipses.exportpdf");
    public string LabelColDate      => T("view.eclipses.results.col.date");
    public string LabelColPosition  => T("view.eclipses.results.col.position");
    public string LabelColType      => T("view.eclipses.results.col.type");
    public string LabelColVisible   => T("view.eclipses.results.col.visible");
    public string LabelColSaros     => T("view.eclipses.results.col.saros");

    private string T(string key) => _rosetta.GetText(RbFile.Eclipses, key);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>One formatted row for the eclipses results table.</summary>
public sealed class EclipseResultRow(
    string eclipseGlyph, string dateText, string positionText, string signGlyph,
    string typeText, bool hasLocalData, bool isVisible, string sarosText)
{
    public string EclipseGlyph { get; } = eclipseGlyph;
    public string DateText     { get; } = dateText;
    public string PositionText { get; } = positionText;
    public string SignGlyph    { get; } = signGlyph;
    public string TypeText     { get; } = typeText;
    public bool   HasLocalData { get; } = hasLocalData;
    public bool   IsVisible    { get; } = isVisible;
    public string SarosText    { get; } = sarosText;
}
