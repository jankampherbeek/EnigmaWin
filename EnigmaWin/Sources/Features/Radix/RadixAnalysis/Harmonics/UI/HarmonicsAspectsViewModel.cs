// HarmonicsAspectsViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects.UI;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics.UI;

/// <summary>Shows all aspects between the harmonic positions, with orb and exactness.</summary>
public sealed class HarmonicsAspectsViewModel : INotifyPropertyChanged
{
    private readonly RadixAspectsModel _model = new();
    private readonly IRosetta          _rosetta;
    private readonly IConfigContext    _configContext;
    private bool                       _hasData;

    public ObservableCollection<RadixAspectsModel.AspectRow> AspectRows { get; } = [];

    public HarmonicsAspectsViewModel(IRosetta rosetta, IConfigContext configContext)
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

    public string LabelTitle       => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.title.aspects");
    public string TooltipFactsheet => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.factsheet.tooltip");
    public string TooltipHelp      => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.help.tooltip");
    public string LabelFactor1     => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.col.factor1");
    public string LabelAspect      => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.col.aspect");
    public string LabelFactor2     => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.col.factor2");
    public string LabelOrb         => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.col.orb");
    public string LabelExactness   => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.col.exactness");
    public string LabelEmpty       => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.aspects.nodata");

    public void Load(FullChart? chart, double harmonic)
    {
        AspectRows.Clear();

        if (chart == null)
        {
            HasData = false;
            return;
        }

        var aspects = HarmonicsOrchestrator.Aspects(chart, _configContext.ActiveConfig, harmonic);
        foreach (var row in _model.BuildRows(aspects, _rosetta))
            AspectRows.Add(row);

        HasData = AspectRows.Count > 0;
        OnPropertyChanged(nameof(AspectRows));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
