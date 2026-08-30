// BlaSchemaViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Serilog;

namespace EnigmaWin.Sources.Features.BlaSchema.UI;

/// <summary>ViewModel for the BLA schema ("Invisible Luminaries Astrology") screen. Recomputes a local,
/// isolated chart for the BLA point set on every option change, using its own CalcRequest — it never
/// touches the shared chart's original house system or factor configuration.</summary>
public sealed class BlaSchemaViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly IConfigContext _configContext;
    private readonly IChartSession _chartSession;
    private readonly BlaSchemaModel _model = new();

    private NamedChart? _currentChart;
    private bool _hasData;
    private string _chartName = "";

    private static readonly HouseSystems[] HouseSystemOptions =
    [
        HouseSystems.Alcabitius, HouseSystems.Campanus, HouseSystems.Koch, HouseSystems.Krusinski,
        HouseSystems.Placidus, HouseSystems.Porphyri, HouseSystems.Regiomontanus, HouseSystems.TopoCentric
    ];
    private int _houseSystemIndex = 4; // Placidus
    private int _correctionTypeIndex; // Koch/"Corrected SE"
    private bool _useTrueNode;
    private bool _useChiron;
    private bool _useCeres;
    private bool _useDecanates;

    private enum Section { ConfigPositions, Counts, Dispositors, DetailsCycles, Reinforcements, Receptions }
    private Section _activeSection = Section.ConfigPositions;

    public BlaSchemaViewModel(IRosetta rosetta, IConfigContext configContext, IChartSession chartSession)
    {
        _rosetta = rosetta;
        _configContext = configContext;
        _chartSession = chartSession;

        ShowConfigPositionsCommand = new RelayCommand(() => SetSection(Section.ConfigPositions));
        ShowCountsCommand = new RelayCommand(() => SetSection(Section.Counts));
        ShowDispositorsCommand = new RelayCommand(() => SetSection(Section.Dispositors));
        ShowDetailsCyclesCommand = new RelayCommand(() => SetSection(Section.DetailsCycles));
        ShowReinforcementsCommand = new RelayCommand(() => SetSection(Section.Reinforcements));
        ShowReceptionsCommand = new RelayCommand(() => SetSection(Section.Receptions));

        LoadChart(chartSession.Selected);
    }

    // ── Section switching ───────────────────────────────────────────────────

    public IRelayCommand ShowConfigPositionsCommand { get; }
    public IRelayCommand ShowCountsCommand { get; }
    public IRelayCommand ShowDispositorsCommand { get; }
    public IRelayCommand ShowDetailsCyclesCommand { get; }
    public IRelayCommand ShowReinforcementsCommand { get; }
    public IRelayCommand ShowReceptionsCommand { get; }

    public bool ShowConfigPositions => _activeSection == Section.ConfigPositions;
    public bool ShowCounts => _activeSection == Section.Counts;
    public bool ShowDispositors => _activeSection == Section.Dispositors;
    public bool ShowDetailsCycles => _activeSection == Section.DetailsCycles;
    public bool ShowReinforcements => _activeSection == Section.Reinforcements;
    public bool ShowReceptions => _activeSection == Section.Receptions;

    public string CurrentSectionTitle => _activeSection switch
    {
        Section.ConfigPositions => LabelSectionConfigPositions,
        Section.Counts => LabelSectionCounts,
        Section.Dispositors => LabelSectionDispositors,
        Section.DetailsCycles => LabelSectionDetailsCycles,
        Section.Reinforcements => LabelSectionReinforcements,
        Section.Receptions => LabelSectionReceptions,
        _ => LabelSectionConfigPositions
    };

    public string CurrentSectionHelpText => _activeSection switch
    {
        Section.ConfigPositions => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.help.configpositions"),
        Section.Counts => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.help.counts"),
        Section.Dispositors => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.help.dispositors"),
        Section.DetailsCycles => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.help.detailscycles"),
        Section.Reinforcements => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.help.reinforcements"),
        Section.Receptions => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.help.receptions"),
        _ => ""
    };

    private void SetSection(Section section)
    {
        if (_activeSection == section) return;
        _activeSection = section;
        OnPropertyChanged(nameof(ShowConfigPositions));
        OnPropertyChanged(nameof(ShowCounts));
        OnPropertyChanged(nameof(ShowDispositors));
        OnPropertyChanged(nameof(ShowDetailsCycles));
        OnPropertyChanged(nameof(ShowReinforcements));
        OnPropertyChanged(nameof(ShowReceptions));
    }

    // ── Options ─────────────────────────────────────────────────────────────

    public List<string> HouseSystemNames { get; } = BuildHouseSystemNames();

    public int HouseSystemIndex
    {
        get => _houseSystemIndex;
        set { if (_houseSystemIndex == value) return; _houseSystemIndex = value; OnPropertyChanged(); Recalculate(); }
    }

    public List<string> CorrectionTypeNames => [LabelCorrectionKoch, LabelCorrectionDuval, LabelCorrectionInterpolated];

    public int CorrectionTypeIndex
    {
        get => _correctionTypeIndex;
        set { if (_correctionTypeIndex == value) return; _correctionTypeIndex = value; OnPropertyChanged(); Recalculate(); }
    }

    public bool UseTrueNode
    {
        get => _useTrueNode;
        set { if (_useTrueNode == value) return; _useTrueNode = value; OnPropertyChanged(); Recalculate(); }
    }

    public bool UseChiron
    {
        get => _useChiron;
        set { if (_useChiron == value) return; _useChiron = value; OnPropertyChanged(); Recalculate(); }
    }

    public bool UseCeres
    {
        get => _useCeres;
        set { if (_useCeres == value) return; _useCeres = value; OnPropertyChanged(); Recalculate(); }
    }

    public bool UseDecanates
    {
        get => _useDecanates;
        set { if (_useDecanates == value) return; _useDecanates = value; OnPropertyChanged(); Recalculate(); }
    }

    // ── State ───────────────────────────────────────────────────────────────

    public bool HasData { get => _hasData; private set { if (_hasData == value) return; _hasData = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoData)); } }
    public bool HasNoData => !HasData;
    public string ChartName { get => _chartName; private set { if (_chartName == value) return; _chartName = value; OnPropertyChanged(); } }

    // ── Results ─────────────────────────────────────────────────────────────

    public ObservableCollection<BlaSchemaModel.PositionRow> Positions { get; } = [];
    public ObservableCollection<BlaSchemaModel.HousePositionRow> HousePositions { get; } = [];
    public ObservableCollection<BlaSchemaModel.CountRow> ElementsCounts { get; } = [];
    public ObservableCollection<BlaSchemaModel.CountRow> CrossesCounts { get; } = [];
    public ObservableCollection<BlaSchemaModel.QuadrantRow> QuadrantCounts { get; } = [];
    public ObservableCollection<BlaSchemaModel.DispositorRow> Dispositors { get; } = [];
    public ObservableCollection<BlaSchemaModel.DetailRow> Details { get; } = [];
    public ObservableCollection<BlaSchemaModel.CycleRow> Cycles { get; } = [];
    public ObservableCollection<BlaSchemaModel.CycleRow> ShortenedCycles { get; } = [];
    public ObservableCollection<BlaSchemaModel.ReinforcementRow> FactorsInOwnSigns { get; } = [];
    public ObservableCollection<BlaSchemaModel.ReinforcementRow> FactorsInOwnHouses { get; } = [];
    public ObservableCollection<BlaSchemaModel.ReinforcementRow> FactorsInOwnMundaneHouses { get; } = [];
    public ObservableCollection<BlaSchemaModel.ReinforcementRow> HouseLordsInAnalogSigns { get; } = [];
    public ObservableCollection<BlaSchemaModel.PairAnalogRow> PairsAnalogHouseSign { get; } = [];
    public ObservableCollection<BlaSchemaModel.ReceptionRow> ReceptionsInSigns { get; } = [];
    public ObservableCollection<BlaSchemaModel.ReceptionRow> ReceptionsInHouses { get; } = [];
    public ObservableCollection<BlaSchemaModel.ReceptionRow> ReceptionsInMundaneHouses { get; } = [];

    // ── Chart loading ───────────────────────────────────────────────────────

    public void LoadChart(NamedChart? namedChart)
    {
        _currentChart = namedChart;
        ChartName = namedChart?.Name ?? "";
        Recalculate();
    }

    private void Recalculate()
    {
        Positions.Clear();
        HousePositions.Clear();
        ElementsCounts.Clear();
        CrossesCounts.Clear();
        QuadrantCounts.Clear();
        Dispositors.Clear();
        Details.Clear();
        Cycles.Clear();
        ShortenedCycles.Clear();
        FactorsInOwnSigns.Clear();
        FactorsInOwnHouses.Clear();
        FactorsInOwnMundaneHouses.Clear();
        HouseLordsInAnalogSigns.Clear();
        PairsAnalogHouseSign.Clear();
        ReceptionsInSigns.Clear();
        ReceptionsInHouses.Clear();
        ReceptionsInMundaneHouses.Clear();

        if (_currentChart == null) { HasData = false; return; }

        try
        {
            var correctionType = (BlaApogeeCorrectionType)_correctionTypeIndex;
            var houseSystem = HouseSystemOptions[_houseSystemIndex];

            var result = _model.Calculate(
                _currentChart, _configContext, _rosetta, houseSystem, correctionType,
                _useTrueNode, _useChiron, _useCeres, _useDecanates);

            foreach (var row in result.Positions) Positions.Add(row);
            foreach (var row in result.HousePositions) HousePositions.Add(row);
            foreach (var row in result.ElementsCounts) ElementsCounts.Add(row);
            foreach (var row in result.CrossesCounts) CrossesCounts.Add(row);
            foreach (var row in result.QuadrantCounts) QuadrantCounts.Add(row);
            foreach (var row in result.Dispositors) Dispositors.Add(row);
            foreach (var row in result.Details) Details.Add(row);
            foreach (var row in result.Cycles) Cycles.Add(row);
            foreach (var row in result.ShortenedCycles) ShortenedCycles.Add(row);
            foreach (var row in result.FactorsInOwnSigns) FactorsInOwnSigns.Add(row);
            foreach (var row in result.FactorsInOwnHouses) FactorsInOwnHouses.Add(row);
            foreach (var row in result.FactorsInOwnMundaneHouses) FactorsInOwnMundaneHouses.Add(row);
            foreach (var row in result.HouseLordsInAnalogSigns) HouseLordsInAnalogSigns.Add(row);
            foreach (var row in result.PairsAnalogHouseSign) PairsAnalogHouseSign.Add(row);
            foreach (var row in result.ReceptionsInSigns) ReceptionsInSigns.Add(row);
            foreach (var row in result.ReceptionsInHouses) ReceptionsInHouses.Add(row);
            foreach (var row in result.ReceptionsInMundaneHouses) ReceptionsInMundaneHouses.Add(row);

            HasData = true;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "BLA schema calculation failed.");
            HasData = false;
        }
    }

    private static List<string> BuildHouseSystemNames() =>
        [.. Array.ConvertAll(HouseSystemOptions, hs => hs.ToString())];

    // ── Labels ──────────────────────────────────────────────────────────────

    public string LabelTitle => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.title");
    public string LabelNoChart => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.nochart");
    public string TooltipHelp => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.help.tooltip");
    public string TooltipFactsheet => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.factsheet.tooltip");
    public string LabelSectionConfigPositions => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.section.configpositions");
    public string LabelSectionCounts => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.section.counts");
    public string LabelSectionDispositors => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.section.dispositors");
    public string LabelSectionDetailsCycles => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.section.detailscycles");
    public string LabelSectionReinforcements => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.section.reinforcements");
    public string LabelSectionReceptions => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.section.receptions");

    public string LabelHouseSystem => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.housesystem");
    public string LabelCorrectionType => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.correctiontype");
    public string LabelCorrectionKoch => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.correction.koch");
    public string LabelCorrectionDuval => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.correction.duval");
    public string LabelCorrectionInterpolated => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.correction.interpolated");
    public string LabelUseTrueNode => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.usetruenode");
    public string LabelUseChiron => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.usechiron");
    public string LabelUseCeres => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.useceres");
    public string LabelUseDecanates => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.usedecanates");

    public string LabelPositionsTitle => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.positions");
    public string LabelHousePositionsTitle => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.housepositions");
    public string LabelColFactor => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.col.factor");
    public string LabelColPosition => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.col.position");
    public string LabelColSign => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.col.sign");
    public string LabelColHouse => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.col.house");
    public string LabelColDecanate => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.col.decanate");
    public string LabelColCusp => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.col.cusp");

    public string LabelElementsTitle => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.elements");
    public string LabelCrossesTitle => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.crosses");
    public string LabelQuadrantsTitle => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.quadrants");
    public string LabelColSum => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.col.sum");
    public string LabelColHCusp => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.col.hcusp");
    public string LabelColTotal => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.col.total");
    public string LabelColCount => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.col.count");

    public string LabelDispositorsTitle => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.dispositors");
    public string LabelColRulers => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.col.rulers");
    public string LabelColCombined => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.col.combined");
    public string LabelColIndirect => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.col.indirect");
    public string LabelColMain => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.col.main");
    public string LabelColDecanateShort => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.col.decanateshort");

    public string LabelDetailsTitle => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.details");
    public string LabelCyclesTitle => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.cycles");
    public string LabelShortenedCyclesTitle => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.shortenedcycles");

    public string LabelInOwnSignTitle => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.inownsign");
    public string LabelInOwnHouseTitle => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.inownhouse");
    public string LabelInOwnMundaneHouseTitle => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.inownmundanehouse");
    public string LabelLordInAnalogSignTitle => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.lordinanalogsign");
    public string LabelPairsAnalogTitle => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.pairsanalog");

    public string LabelReceptionsInSignsTitle => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.receptionsinsigns");
    public string LabelReceptionsInHousesTitle => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.receptionsinhouses");
    public string LabelReceptionsInMundaneHousesTitle => _rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.label.receptionsinmundanehouses");

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
