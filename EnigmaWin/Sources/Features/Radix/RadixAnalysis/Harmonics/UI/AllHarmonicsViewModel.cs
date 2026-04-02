// AllHarmonicsViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics.UI;

public sealed class AllHarmonicsViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly IConfigContext _configContext;
    private bool _hasData;

    public record HarmonicRow(string FactorGlyph, string PositionDms, string SignGlyph);

    public ObservableCollection<HarmonicRow> Rows { get; } = [];

    public AllHarmonicsViewModel(IRosetta rosetta, IConfigContext configContext)
    {
        _rosetta       = rosetta;
        _configContext = configContext;
    }

    public bool HasData
    {
        get => _hasData;
        private set
        {
            if (_hasData == value) return;
            _hasData = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasNoData));
        }
    }

    public bool HasNoData => !HasData;

    public string LabelTitle        => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.title.allharmonics");
    public string LabelFactor       => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.col.factor");
    public string LabelPosition     => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.col.position");
    public string LabelEmpty        => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.allharmonics.nodata");

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Load(FullChart? chart, double harmonic)
    {
        Rows.Clear();

        if (chart == null)
        {
            HasData = false;
            return;
        }

        var positions = HarmonicsOrchestrator.Calculate(chart, _configContext.ActiveConfig, harmonic);

        foreach (var (factor, longitude) in positions)
        {
            var glyph = GlyphSelector.GetGlyphForFactor(factor);
            var (dms, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(longitude);
            var signGlyph = ok && sign.HasValue ? GlyphSelector.GetGlyphForSign(sign.Value) : "";
            Rows.Add(new HarmonicRow(glyph, dms, signGlyph));
        }

        HasData = Rows.Count > 0;
        OnPropertyChanged(nameof(Rows));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
