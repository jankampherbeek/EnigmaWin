// LongTimeEphemerisResultViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Periods.LongTimeEphemeris.UI;

public sealed class LongTimeEphemerisResultViewModel : INotifyPropertyChanged
{
    private static readonly IReadOnlyList<string> SignAbbreviations =
        ["AR", "TA", "GE", "CN", "LE", "VI", "LI", "SC", "SA", "CP", "AQ", "PI"];

    private readonly IRosetta _rosetta;
    private readonly LongTimeEphemerisModel _model;

    public LongTimeEphemerisResultViewModel(IRosetta rosetta, LongTimeEphemerisModel model)
    {
        _rosetta = rosetta;
        _model   = model;

        _model.PropertyChanged += (_, _) =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(HasResults));
                OnPropertyChanged(nameof(Columns));
                OnPropertyChanged(nameof(TableRows));
            });
        };
    }

    public bool HasResults => _model.HasResults;

    public IReadOnlyList<Factors> Columns => _model.Factors;

    public string ColumnHeader(Factors f) => _rosetta.GetText(RbFile.Localizable, f.LocalizedName());

    public List<LongTimeEphemerisDisplayRow> TableRows =>
        _model.Rows.Select(ToDisplayRow).ToList();

    private LongTimeEphemerisDisplayRow ToDisplayRow(LongTimeEphemerisRow row)
    {
        var cells = _model.Factors.ToDictionary(
            f => f,
            f => row.Values.TryGetValue(f, out var v) ? FormatValue(v) : "—");
        return new LongTimeEphemerisDisplayRow(row.Id, row.DateTimeText, cells);
    }

    private string FormatValue(double value)
    {
        if (_model.DisplayFormat == LongTimeEphemerisDisplayFormat.Decimal)
            return value.ToString("F4");

        return _model.SelectedCoordinate switch
        {
            LongTimeEphemerisCoordinate.Longitude => FormatLongitude(value),
            LongTimeEphemerisCoordinate.Distance  => value.ToString("F5"),
            _                                      => PositionInDegreesConversion.DoubleToDms(value)
        };
    }

    private static string FormatLongitude(double value)
    {
        var (dmsText, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(value);
        if (!ok || sign is null) return PositionInDegreesConversion.DoubleToDms(value);
        return $"{dmsText} {SignAbbreviations[(int)sign.Value - 1]}";
    }

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelNoResults  => T("view.ltephemeris.noresults");
    public string LabelCalculating => T("view.ltephemeris.calculating");
    public string LabelExportCsv  => T("view.ltephemeris.exportcsv");
    public string LabelDateHeader => T("view.ltephemeris.date.header");

    private string T(string key) => _rosetta.GetText(RbFile.LongTimeEphemeris, key);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>One formatted row for the results DataGrid: date/time text plus one formatted cell per factor.</summary>
public sealed class LongTimeEphemerisDisplayRow(int id, string dateTimeText, IReadOnlyDictionary<Factors, string> cells)
{
    public int Id { get; } = id;
    public string DateTimeText { get; } = dateTimeText;
    public IReadOnlyDictionary<Factors, string> Cells { get; } = cells;
}
