// ProgressiveCalendarViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Progressive.PreNatal;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Progressive.ProgressiveCalendar.UI;

public partial class ProgressiveCalendarViewModel : ObservableObject
{
    private readonly IChartSession _chartSession;
    private readonly IConfigContext _configContext;
    private readonly IRosetta _rosetta;

    public ProgressiveCalendarInputViewModel Inner { get; }

    // MARK: - Chart selection

    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasCharts))]
    private NamedChart? _selectedChart;

    public bool HasCharts => _chartSession.Charts.Count > 0;
    public IReadOnlyList<NamedChart> Charts => _chartSession.Charts;

    // MARK: - Editable settings

    [ObservableProperty] private bool _useTransits = true;
    [ObservableProperty] private bool _useSecondaryDirections = true;
    [ObservableProperty] private bool _useSymbolicDirections;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(TransitFactorsGlyphs))]
    private IReadOnlyList<Factors> _transitFactors = ProgressiveCalendarConfig.DefaultTransitFactors;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(SecondaryDirectionFactorsGlyphs))]
    private IReadOnlyList<Factors> _secondaryDirectionFactors = ProgressiveCalendarConfig.DefaultSecondaryDirectionFactors;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(SymbolicDirectionFactorsGlyphs))]
    private IReadOnlyList<Factors> _symbolicDirectionFactors = ProgressiveCalendarConfig.DefaultSymbolicDirectionFactors;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(RadixFactorsGlyphs))]
    private IReadOnlyList<Factors> _radixFactors = ProgressiveCalendarConfig.DefaultRadixFactors;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(AspectsGlyphs))]
    private IReadOnlyList<Aspects> _aspects = ProgressiveCalendarConfig.DefaultAspects;

    [ObservableProperty] private int _selectedSymbolicKeyIndex = (int)SymbolicKeys.OneDegree;

    [ObservableProperty] private double _aspectOrb = 1.0;
    [ObservableProperty] private double _parallelOrb = 1.0;
    [ObservableProperty] private double _cuspOrb = 1.0;

    [ObservableProperty] private bool _useAspectsToRadix = true;
    [ObservableProperty] private bool _useParallelsToRadix = true;
    [ObservableProperty] private bool _useAspectsProgToProg;
    [ObservableProperty] private bool _useParallelsProgToProg;
    [ObservableProperty] private bool _useCuspConjunctions = true;
    [ObservableProperty] private bool _useRetrogradeDirectStations = true;
    [ObservableProperty] private bool _useOobEnterExit;
    [ObservableProperty] private bool _useDeclinationExtremes;

    [ObservableProperty] private string _startDateText = string.Empty;
    [ObservableProperty] private string _endDateText = string.Empty;

    public string TransitFactorsGlyphs => FactorGlyphsText(TransitFactors);
    public string SecondaryDirectionFactorsGlyphs => FactorGlyphsText(SecondaryDirectionFactors);
    public string SymbolicDirectionFactorsGlyphs => FactorGlyphsText(SymbolicDirectionFactors);
    public string RadixFactorsGlyphs => FactorGlyphsText(RadixFactors);
    public string AspectsGlyphs => string.Join(" ", Aspects.Select(Shared.Glyphs.GlyphSelector.GetGlyphForAspect));

    private static string FactorGlyphsText(IEnumerable<Factors> factors) =>
        string.Join(" ", factors.Select(Shared.Glyphs.GlyphSelector.GetGlyphForFactor));

    public static IReadOnlyList<Factors> SelectableFactors => PreNatalOrchestrator.SelectableFactors;

    private static readonly SymbolicKeys[] AllSymbolicKeys = Enum.GetValues<SymbolicKeys>();
    public IReadOnlyList<string> SymbolicKeyNames { get; private set; } = [];

    // MARK: - Results

    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasResults))]
    private ObservableCollection<ProgressiveCalendarTechniqueSection> _techniqueSections = [];

    [ObservableProperty] private bool _isCalculating;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _inputErrorMessage = string.Empty;
    [ObservableProperty] private bool _hasInputError;
    [ObservableProperty] private string _resultsSummaryText = string.Empty;

    public bool HasResults => TechniqueSections.Count > 0;

    private double? _lastStartJD;
    private double? _lastEndJD;

    // MARK: - Labels — input screen

    public string LabelTitle => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.title");
    public string LabelNoChart => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.nochart");
    public string LabelNoSession => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.nosession");
    public string LabelChartSection => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.chartsection");
    public string LabelTechniquesHeader => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.techniques.header");
    public string LabelUseTransitsText => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.techniques.transits");
    public string LabelUseSecondaryDirectionsText => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.techniques.secondarydirections");
    public string LabelUseSymbolicDirectionsText => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.techniques.symbolicdirections");
    public string LabelSymbolicKey => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.symbolickey");
    public string LabelTransitFactorsButton => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.button.transitfactors");
    public string LabelSecondaryFactorsButton => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.button.secondaryfactors");
    public string LabelSymbolicFactorsButton => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.button.symbolicfactors");
    public string LabelRadixFactorsButton => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.button.radixfactors");
    public string LabelAspectsButton => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.button.aspects");

    public string LabelEventKindsHeader => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.eventkinds.header");
    public string LabelUseAspectsToRadixText => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.eventkinds.aspectstoradix");
    public string LabelUseParallelsToRadixText => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.eventkinds.parallelstoradix");
    public string LabelUseAspectsProgToProgText => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.eventkinds.aspectsprogtoprog");
    public string LabelUseParallelsProgToProgText => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.eventkinds.parallelsprogtoprog");
    public string LabelUseCuspConjunctionsText => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.eventkinds.cuspconjunctions");
    public string LabelUseStationsText => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.eventkinds.stations");
    public string LabelUseOobEnterExitText => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.eventkinds.oob");
    public string LabelUseDeclinationExtremesText => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.eventkinds.declinationextremes");

    public string LabelOrbsHeader => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.orbs.header");
    public string LabelAspectOrb => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.orbs.aspect");
    public string LabelParallelOrb => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.orbs.parallel");
    public string LabelCuspOrb => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.orbs.cusp");

    public string LabelDateRangeHeader => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.daterange.header");
    public string LabelStartDate => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.daterange.start");
    public string LabelEndDate => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.daterange.end");
    public string LabelDatePlaceholder => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.daterange.placeholder");
    public string LabelMaxRangeNote => string.Format(
        _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.daterange.maxnote"),
        (int)Math.Round(MaxRangeInDays));

    public string LabelSettingsReset => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.settings.reset");
    public string LabelCalculate => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.calculate");
    public string LabelHelp => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.help");
    public string LabelHelpTitle => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.help.title");
    public string LabelHelpClose => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.help.close");

    public string LabelSelectionDone => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.selection.done");
    public string LabelSelectionCancel => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.selection.cancel");
    public string LabelAspectSelTitle => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.aspectsel.title");

    // MARK: - Labels — results screen

    public string LabelResultsTitle => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.results.title");
    public string LabelNoResults => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.results.noresults");
    public string LabelResultsHelp => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.results.help");
    public string LabelCalculating => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.calculating");
    public string LabelSectionDiagram => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.results.section.diagram");
    public string LabelSectionAspectsParallels => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.results.section.aspectsparallels");
    public string LabelSectionOtherEvents => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.results.section.otherevents");
    public string LabelColEnter => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.results.col.enter");
    public string LabelColExact => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.results.col.exact");
    public string LabelColExit => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.results.col.exit");
    public string LabelColOrb => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.results.col.orb");
    public string LabelColDate => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.results.col.date");
    public string LabelColPosition => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.results.col.position");

    public string LabelLegendParallel => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.legend.parallel");
    public string LabelLegendContraParallel => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.legend.contraparallel");
    public string LabelLegendStation => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.legend.station");
    public string LabelLegendOob => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.legend.oob");
    public string LabelLegendDeclination => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.legend.declination");
    public string LabelLegendCuspConjunction => _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.legend.cuspconjunction");

    public ProgressiveCalendarViewModel(
        IChartSession chartSession,
        IConfigContext configContext,
        IRosetta rosetta)
    {
        _chartSession = chartSession;
        _configContext = configContext;
        _rosetta = rosetta;

        Inner = new ProgressiveCalendarInputViewModel(this);

        BuildNameLists();
        SyncFromConfig();

        if (_chartSession.Selected is not null)
            SelectedChart = _chartSession.Selected;

        _chartSession.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(IChartSession.Charts) or nameof(IChartSession.Selected))
            {
                OnPropertyChanged(nameof(Charts));
                OnPropertyChanged(nameof(HasCharts));
                if (_chartSession.Selected is not null && SelectedChart is null)
                    SelectedChart = _chartSession.Selected;
            }
        };
    }

    partial void OnSelectedChartChanged(NamedChart? value)
    {
        ClearResults();
        CalculateCommand.NotifyCanExecuteChanged();
    }

    partial void OnUseTransitsChanged(bool value) => OnPropertyChanged(nameof(LabelMaxRangeNote));
    partial void OnUseSecondaryDirectionsChanged(bool value) => OnPropertyChanged(nameof(LabelMaxRangeNote));
    partial void OnUseSymbolicDirectionsChanged(bool value) => OnPropertyChanged(nameof(LabelMaxRangeNote));
    partial void OnTransitFactorsChanged(IReadOnlyList<Factors> value) => OnPropertyChanged(nameof(LabelMaxRangeNote));
    partial void OnSecondaryDirectionFactorsChanged(IReadOnlyList<Factors> value) => OnPropertyChanged(nameof(LabelMaxRangeNote));
    partial void OnSymbolicDirectionFactorsChanged(IReadOnlyList<Factors> value) => OnPropertyChanged(nameof(LabelMaxRangeNote));

    private void BuildNameLists()
    {
        SymbolicKeyNames = AllSymbolicKeys.Select(k => _rosetta.GetText(RbFile.Localizable, k.LocalizedName())).ToList();
    }

    private ProgressiveCalendarConfig SavedConfig => _configContext.ActiveConfig.ProgressionsConfig.ProgressiveCalendar;

    [RelayCommand]
    public void SyncFromConfig()
    {
        var c = SavedConfig;
        UseTransits = c.UseTransits;
        UseSecondaryDirections = c.UseSecondaryDirections;
        UseSymbolicDirections = c.UseSymbolicDirections;
        TransitFactors = c.TransitFactors;
        SecondaryDirectionFactors = c.SecondaryDirectionFactors;
        SymbolicDirectionFactors = c.SymbolicDirectionFactors;
        SelectedSymbolicKeyIndex = Math.Max(0, Array.IndexOf(AllSymbolicKeys, c.SymbolicKey));
        RadixFactors = c.RadixFactors;
        Aspects = c.Aspects;
        AspectOrb = c.AspectOrb;
        ParallelOrb = c.ParallelOrb;
        CuspOrb = c.CuspOrb;
        UseAspectsToRadix = c.UseAspectsToRadix;
        UseParallelsToRadix = c.UseParallelsToRadix;
        UseAspectsProgToProg = c.UseAspectsProgToProg;
        UseParallelsProgToProg = c.UseParallelsProgToProg;
        UseCuspConjunctions = c.UseCuspConjunctions;
        UseRetrogradeDirectStations = c.UseRetrogradeDirectStations;
        UseOobEnterExit = c.UseOobEnterExit;
        UseDeclinationExtremes = c.UseDeclinationExtremes;
    }

    // MARK: - Derived

    private List<ProgressiveCalendarSelection> ActiveSelections()
    {
        var result = new List<ProgressiveCalendarSelection>();
        if (UseTransits && TransitFactors.Count > 0)
            result.Add(new ProgressiveCalendarSelection(ProgressiveCalendarTechnique.Transit, TransitFactors));
        if (UseSecondaryDirections && SecondaryDirectionFactors.Count > 0)
            result.Add(new ProgressiveCalendarSelection(ProgressiveCalendarTechnique.SecondaryDirection, SecondaryDirectionFactors));
        if (UseSymbolicDirections && SymbolicDirectionFactors.Count > 0)
            result.Add(new ProgressiveCalendarSelection(
                ProgressiveCalendarTechnique.SymbolicDirection, SymbolicDirectionFactors, CurrentSymbolicKey));
        return result;
    }

    private SymbolicKeys CurrentSymbolicKey =>
        AllSymbolicKeys[Math.Clamp(SelectedSymbolicKeyIndex, 0, AllSymbolicKeys.Length - 1)];

    public double MaxRangeInDays => ProgressiveCalendarRangeLimiter.MaxRangeInDays(ActiveSelections());

    // MARK: - Calculate

    [RelayCommand(CanExecute = nameof(CanCalculate), AllowConcurrentExecutions = false)]
    private async Task CalculateAsync()
    {
        if (IsCalculating || SelectedChart is null) return;

        ErrorMessage = string.Empty;
        HasError = false;
        InputErrorMessage = string.Empty;
        HasInputError = false;

        if (!TryParseDate(StartDateText, out var startJD))
        {
            SetInputError(_rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.error.invalidstart"));
            return;
        }
        if (!TryParseDate(EndDateText, out var endJD))
        {
            SetInputError(_rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.error.invalidend"));
            return;
        }
        if (endJD <= startJD)
        {
            SetInputError(_rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.error.endbeforestart"));
            return;
        }

        var selections = ActiveSelections();
        if (selections.Count == 0)
        {
            SetInputError(_rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.error.notechnique"));
            return;
        }

        var limit = ProgressiveCalendarRangeLimiter.MaxRangeInDays(selections);
        var requestedDays = endJD - startJD;
        if (requestedDays > limit)
        {
            SetInputError(string.Format(
                _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.error.rangetoolong"),
                (int)Math.Round(limit), (int)Math.Round(requestedDays)));
            return;
        }

        // Setting IsCalculating here lets WPF actually paint the "calculating" indicator before
        // the (potentially slow) scan starts — the scan itself runs on a background thread via
        // Task.Run below, so the UI thread stays free to render and the command's CanExecute
        // (gated on IsCalculating) disables the button for the duration.
        IsCalculating = true;
        TechniqueSections = [];
        _lastStartJD = startJD;
        _lastEndJD = endJD;

        var natalJD = SelectedChart.Chart.JulianDay;
        var radixChart = SelectedChart.Chart;
        var radixFactors = RadixFactors;
        var aspects = Aspects;
        var aspectOrb = AspectOrb;
        var parallelOrb = ParallelOrb;
        var cuspOrb = CuspOrb;
        var toggles = new ProgressiveCalendarEventKindToggles
        {
            AspectsToRadix = UseAspectsToRadix,
            ParallelsToRadix = UseParallelsToRadix,
            AspectsProgToProg = UseAspectsProgToProg,
            ParallelsProgToProg = UseParallelsProgToProg,
            CuspConjunctions = UseCuspConjunctions,
            RetrogradeDirectStations = UseRetrogradeDirectStations,
            OobEnterExit = UseOobEnterExit,
            DeclinationExtremes = UseDeclinationExtremes
        };

        try
        {
            var result = await Task.Run(() =>
            {
                var orchestrator = new ProgressiveCalendarOrchestrator(natalJD, radixChart);
                return orchestrator.FindEvents(
                    startJD, endJD, selections, radixFactors, aspects,
                    aspectOrb, parallelOrb, cuspOrb, toggles);
            });

            BuildTechniqueSections(result, startJD, endJD);

            if (result.Events.Count == 0 && result.Episodes.Count == 0)
            {
                ErrorMessage = _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.nohits");
                HasError = true;
            }
            else
            {
                ResultsSummaryText = string.Format(
                    _rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.results.summary"),
                    result.Events.Count, result.Episodes.Count);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsCalculating = false;
        }
    }

    private bool CanCalculate() =>
        HasCharts && SelectedChart is not null &&
        !string.IsNullOrWhiteSpace(StartDateText) && !string.IsNullOrWhiteSpace(EndDateText);

    partial void OnStartDateTextChanged(string value) => CalculateCommand.NotifyCanExecuteChanged();
    partial void OnEndDateTextChanged(string value) => CalculateCommand.NotifyCanExecuteChanged();

    private static readonly ProgressiveCalendarTechnique[] OrderedTechniques =
    [
        ProgressiveCalendarTechnique.Transit,
        ProgressiveCalendarTechnique.SecondaryDirection,
        ProgressiveCalendarTechnique.SymbolicDirection
    ];

    private void BuildTechniqueSections(ProgressiveCalendarResult result, double startJD, double endJD)
    {
        var sections = new List<ProgressiveCalendarTechniqueSection>();
        foreach (var technique in OrderedTechniques)
        {
            var events = result.Events.Where(e => e.Technique == technique).OrderBy(e => e.JulianDay).ToList();
            var episodes = result.Episodes.Where(e => e.Technique == technique)
                .OrderBy(e => e.EnterJD ?? e.ExactJD).ToList();
            if (events.Count == 0 && episodes.Count == 0) continue;

            sections.Add(new ProgressiveCalendarTechniqueSection(
                TechniqueTitle(technique), events, episodes, startJD, endJD, _rosetta));
        }
        TechniqueSections = new ObservableCollection<ProgressiveCalendarTechniqueSection>(sections);
    }

    private string TechniqueTitle(ProgressiveCalendarTechnique technique) => technique switch
    {
        ProgressiveCalendarTechnique.Transit => LabelUseTransitsText,
        ProgressiveCalendarTechnique.SecondaryDirection => LabelUseSecondaryDirectionsText,
        ProgressiveCalendarTechnique.SymbolicDirection => LabelUseSymbolicDirectionsText,
        _ => string.Empty
    };

    private void SetInputError(string message)
    {
        InputErrorMessage = message;
        HasInputError = true;
    }

    internal void ClearResults()
    {
        TechniqueSections = [];
        ErrorMessage = string.Empty;
        HasError = false;
        InputErrorMessage = string.Empty;
        HasInputError = false;
        ResultsSummaryText = string.Empty;
        _lastStartJD = null;
        _lastEndJD = null;
    }

    private static bool TryParseDate(string text, out double jd)
    {
        jd = 0;
        var trimmed = text.Trim();
        var parts = trimmed.Split('/', '-', '.');
        if (parts.Length != 3) return false;
        if (!int.TryParse(parts[0], out var y)) return false;
        if (!int.TryParse(parts[1], out var m)) return false;
        if (!int.TryParse(parts[2], out var d)) return false;
        if (m is < 1 or > 12 || d is < 1 or > 31) return false;
        var date = new AstronomicalDate(y, m, d, Gregorian: true);
        var time = new AstronomicalTime(0, 0, 0);
        jd = SEWrapper.JulianDay(date, time);
        return true;
    }
}
