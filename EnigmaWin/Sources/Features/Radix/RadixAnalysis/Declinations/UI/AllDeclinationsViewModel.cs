// AllDeclinationsViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public sealed class AllDeclinationsViewModel : INotifyPropertyChanged
{
    private readonly IRosetta      _rosetta;
    private readonly IConfigContext _configContext;
    private bool    _hasData;
    private bool    _isBlackWhite;
    private double  _obliquity = 23.45;

    public record DeclRow(string FactorGlyph, string LongitudeDms, string SignGlyph, string DeclinationDms, bool IsEvenRow);

    public ObservableCollection<DeclRow>       Rows      { get; } = [];
    public ObservableCollection<DeclStripItem> StripItems { get; } = [];

    public IRelayCommand ToggleBlackWhiteCommand { get; }

    public AllDeclinationsViewModel(IRosetta rosetta, IConfigContext configContext)
    {
        _rosetta       = rosetta;
        _configContext = configContext;

        ToggleBlackWhiteCommand = new RelayCommand(() =>
        {
            _isBlackWhite = !_isBlackWhite;
            OnPropertyChanged(nameof(IsBlackWhite));
        });
    }

    // ── State ─────────────────────────────────────────────────────────────

    public bool IsBlackWhite => _isBlackWhite;
    public double Obliquity  => _obliquity;

    public bool HasData   { get => _hasData;   private set { if (_hasData == value) return; _hasData = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoData)); } }
    public bool HasNoData => !HasData;

    // ── Localized labels ──────────────────────────────────────────────────

    public string LabelTitle          => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.all.title");
    public string LabelBtnBlackWhite  => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.btn.blackwhite");
    public string LabelBtnExport      => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.btn.export");
    public string TooltipFactsheet    => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.factsheet.tooltip");
    public string TooltipHelp         => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.help.tooltip");
    public string LabelColFactor      => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.col.factor");
    public string LabelColLongitude   => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.col.longitude");
    public string LabelColDeclination => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.col.declination");
    public string LabelEmpty          => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.nodata");

    // ── Data loading ──────────────────────────────────────────────────────

    public void LoadChart(FullChart? chart)
    {
        Rows.Clear();
        StripItems.Clear();

        if (chart is null)
        {
            HasData = false;
            OnPropertyChanged(nameof(StripItems));
            OnPropertyChanged(nameof(Rows));
            return;
        }

        _obliquity = chart.Obliquity;
        OnPropertyChanged(nameof(Obliquity));

        var factorSettings = _configContext.ActiveConfig.FactorConfig.Settings
            .Where(s => s.IsUsed)
            .ToList();

        var rowIndex = 0;
        foreach (var setting in factorSettings.OrderBy(s => (int)s.Factor))
        {
            double declination;
            double longitude;

            // Mundane factors are stored in HousePositions with accurate values;
            // chart.Coordinates contains placeholder zeros for them.
            FullCuspPosition? cusp = setting.Factor switch
            {
                Factors.Ascendant => chart.HousePositions.Ascendant,
                Factors.Mc        => chart.HousePositions.Midheaven,
                Factors.EastPoint => chart.HousePositions.Eastpoint,
                Factors.Vertex    => chart.HousePositions.Vertex,
                _                 => null
            };

            if (cusp is not null)
            {
                longitude   = cusp.Longitude;
                declination = cusp.Declination;
            }
            else
            {
                if (!chart.Coordinates.TryGetValue(setting.Factor, out var pos)) continue;
                if (pos.Equatorial.Length == 0) continue;
                declination = pos.Equatorial[0].Deviation;
                longitude   = pos.Ecliptical.Length > 0 ? pos.Ecliptical[0].MainPos : 0.0;
            }
            var glyph       = GlyphSelector.GetGlyphForFactor(setting.Factor);

            // Strip item for canvas
            StripItems.Add(new DeclStripItem(setting.Factor, glyph, declination, longitude));

            // Table row
            var declDms = PositionInDegreesConversion.DoubleToDms(declination);
            var (lonDms, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(longitude);
            var signGlyph = ok && sign.HasValue ? GlyphSelector.GetGlyphForSign(sign.Value) : "";
            Rows.Add(new DeclRow(glyph, lonDms, signGlyph, declDms, rowIndex++ % 2 == 0));
        }

        HasData = Rows.Count > 0;
        OnPropertyChanged(nameof(StripItems));
        OnPropertyChanged(nameof(Rows));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
