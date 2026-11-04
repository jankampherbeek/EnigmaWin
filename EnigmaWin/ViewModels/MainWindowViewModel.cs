// MainWindowViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace EnigmaWin.ViewModels;

/// <summary>
/// Menu, sidebar and pane contents of the main window. The structure follows the Apple version:
/// the sidebar lists all work modes and, below them, the sub-items of the active mode; the menu offers
/// the same modes and sub-items plus a separate Analysis menu. Both are built from <see cref="AppNavigation"/>
/// and <see cref="AnalysisCatalog"/>, so they stay identical.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IChartSession _chartSession;
    private readonly IConfigContext _configContext;
    private readonly IRouteViewModelFactory _routeViewModelFactory;
    private readonly IRosetta _rosetta;
    private readonly IUnsavedChangesGuard _unsavedChanges;
    private string _labelsLanguage;

    // Last opened sub-item per mode, reopened when the mode is activated again.
    private readonly Dictionary<AppMode, NavSection> _lastSection = new();

    public MainWindowLabels Labels { get; private set; }

    public IRelayCommand<AppMode> ActivateModeCommand { get; }
    public IRelayCommand<NavSection> OpenSectionCommand { get; }
    public IRelayCommand BackToAnalysisCommand { get; }

    /// <summary>All work modes, the upper part of the sidebar.</summary>
    public IReadOnlyList<SidebarEntry> SidebarModes { get; private set; } = [];

    /// <summary>Sub-items of the active mode, the lower part of the sidebar.</summary>
    public IReadOnlyList<SidebarEntry> SidebarSections { get; private set; } = [];

    /// <summary>Entries of the Analysis menu: group headers, analyses and separators.</summary>
    public IReadOnlyList<AnalysisMenuEntry> AnalysisMenu { get; private set; } = [];

    [ObservableProperty]
    private object? currentMainViewModel;

    [ObservableProperty]
    private object? currentDetailViewModel;

    [ObservableProperty]
    private AppMode activeMode = AppMode.Radix;

    private NavSection? _currentSection;

    public MainWindowViewModel(
        INavigationService navigationService,
        IChartSession chartSession,
        IConfigContext configContext,
        IRouteViewModelFactory routeViewModelFactory,
        IRosetta rosetta,
        IUnsavedChangesGuard unsavedChanges)
    {
        _unsavedChanges = unsavedChanges;
        _navigationService = navigationService;
        _chartSession = chartSession;
        _configContext = configContext;
        _routeViewModelFactory = routeViewModelFactory;
        _rosetta = rosetta;
        _labelsLanguage = rosetta.GetLanguage();
        Labels = new MainWindowLabels(rosetta);

        ActivateModeCommand   = new RelayCommand<AppMode>(ActivateMode);
        OpenSectionCommand    = new RelayCommand<NavSection>(OpenSection);
        BackToAnalysisCommand = new RelayCommand(() => OpenSection(NavSection.RadixAnalysis));

        _navigationService.PropertyChanged += OnNavigationServicePropertyChanged;

        // Start in Radix: with a selected chart show chart and positions, otherwise the overview.
        if (_chartSession.SelectedChart is not null)
        {
            _currentSection = NavSection.RadixPositions;
            _navigationService.NavigateMain(AppRoutes.RadixChart);
            _navigationService.NavigateDetail(AppRoutes.RadixPositions);
        }
        else
        {
            _currentSection = NavSection.RadixOverview;
            _navigationService.NavigateMain(AppRoutes.RadixOverview);
            _navigationService.NavigateDetail(AppRoutes.RadixChart);
        }
        _lastSection[AppMode.Radix] = _currentSection.Value;

        BuildNavigationItems();
        UpdateCurrentMainViewModel();
        UpdateCurrentDetailViewModel();
    }

    public bool ShowDetailPane       => ActiveMode != AppMode.Research;
    public int  MainViewColumnSpan   => ShowDetailPane ? 1 : 3;
    public bool ShowView1Placeholder => CurrentMainViewModel == null;
    public bool ShowView2Placeholder => CurrentDetailViewModel == null;

    /// <summary>Title above the sub-items in the sidebar: the name of the active mode.</summary>
    public string SidebarSectionsTitle => Labels.Modes[ActiveMode.ToString()];
    public bool HasSidebarSections     => SidebarSections.Count > 0;

    /// <summary>True when an analysis was opened from the analysis overview; shows the button back to the overview.</summary>
    public bool ShowBackToAnalysis =>
        ActiveMode == AppMode.Radix && AnalysisCatalog.IsAnalysisDetailRoute(_navigationService.CurrentDetailRoute);

    // ---------------------------------------------------------------- navigation

    /// <summary>Switches to a work mode and opens its last used sub-item. Radix always starts with the overview.</summary>
    private void ActivateMode(AppMode mode)
    {
        if (mode == ActiveMode) return;
        _unsavedChanges.Perform(() =>
        {
            NavSection? section = mode == AppMode.Radix
                ? NavSection.RadixOverview
                : _lastSection.TryGetValue(mode, out var last) ? last : AppNavigation.DefaultSection(mode);
            SetMode(mode);
            if (section is { } s) NavigateSection(s);
            else NavigateModeWithoutSections(mode);
        });
    }

    /// <summary>Opens a sub-item, switching to its mode when needed.</summary>
    private void OpenSection(NavSection section)
    {
        _unsavedChanges.Perform(() =>
        {
            SetMode(AppNavigation.ModeOf(section));
            NavigateSection(section);
        });
    }

    /// <summary>Opens an analysis from the Analysis menu, switching to Radix when needed.</summary>
    private void OpenAnalysis(AnalysisItem item)
    {
        _unsavedChanges.Perform(() =>
        {
            SetMode(AppMode.Radix);
            _currentSection = NavSection.RadixAnalysis;
            _lastSection[AppMode.Radix] = NavSection.RadixAnalysis;
            AnalysisCatalog.Open(_navigationService, item);
            UpdateSelection();
        });
    }

    private void SetMode(AppMode mode)
    {
        if (ActiveMode == mode) return;
        ActiveMode = mode;
    }

    partial void OnActiveModeChanged(AppMode value)
    {
        _currentSection = null;
        BuildSidebarSections();
        OnPropertyChanged(nameof(ShowDetailPane));
        OnPropertyChanged(nameof(MainViewColumnSpan));
        OnPropertyChanged(nameof(ShowBackToAnalysis));
    }

    private void NavigateSection(NavSection section)
    {
        _currentSection = section;
        _lastSection[AppNavigation.ModeOf(section)] = section;

        switch (section)
        {
            // Selection screens go in the center pane, the chart wheel in the right pane.
            case NavSection.RadixOverview: NavigateRadixSelection(AppRoutes.RadixOverview); break;
            case NavSection.RadixAnalysis: NavigateRadixSelection(AppRoutes.RadixAnalysis); break;
            case NavSection.RadixSearch:   NavigateRadixSelection(AppRoutes.RadixSearch);   break;
            case NavSection.RadixPositions:
                NavigateRadixMain();
                _navigationService.NavigateDetail(AppRoutes.RadixPositions);
                break;

            case NavSection.ProgressiveEvents:
                Navigate(AppRoutes.MainProgressiveHome, AppRoutes.ProgressiveEventsOverview); break;
            case NavSection.ProgressivePrimary:
                Navigate(AppRoutes.ProgressivePrimDirInput, AppRoutes.ProgressivePrimDir); break;
            case NavSection.ProgressiveSecondary:
                Navigate(AppRoutes.ProgressiveSecondaryInput, AppRoutes.ProgressiveSecondary); break;
            case NavSection.ProgressiveTransit:
                Navigate(AppRoutes.ProgressiveTransitInput, AppRoutes.ProgressiveTransit); break;
            case NavSection.ProgressiveSymbolic:
                Navigate(AppRoutes.ProgressiveSymbolicInput, AppRoutes.ProgressiveSymbolic); break;
            case NavSection.ProgressiveSolar:
                Navigate(AppRoutes.ProgressiveSolarInput, AppRoutes.ProgressiveSolar); break;
            case NavSection.ProgressivePrenatal:
                // Results go in the center pane, the input in the right pane.
                Navigate(AppRoutes.ProgressivePreNatal, AppRoutes.ProgressivePreNatalInput); break;
            case NavSection.ProgressiveLogTimeScale:
                Navigate(AppRoutes.ProgressiveLogTimeScaleInput, AppRoutes.ProgressiveLogTimeScale); break;
            case NavSection.ProgressiveAgePoint:
                Navigate(AppRoutes.ProgressiveAgePointInput, AppRoutes.ProgressiveAgePoint); break;
            case NavSection.ProgressiveCalendar:
                Navigate(AppRoutes.ProgressiveCalendarInput, AppRoutes.ProgressiveCalendar); break;

            case NavSection.CyclesAstronomical:
                Navigate(AppRoutes.CyclesAstronomicalInput, AppRoutes.CyclesAstronomical); break;
            case NavSection.CyclesWaves:
                Navigate(AppRoutes.CyclesWavesInput, AppRoutes.CyclesWaves); break;
            case NavSection.CyclesMonthlyEphemeris:
                Navigate(AppRoutes.MonthlyEphemerisInput, AppRoutes.MonthlyEphemeris); break;
            case NavSection.CyclesLongTimeEphemeris:
                Navigate(AppRoutes.LongTimeEphemerisInput, AppRoutes.LongTimeEphemeris); break;
            case NavSection.CyclesEclipses:
                Navigate(AppRoutes.EclipsesInput, AppRoutes.Eclipses); break;

            case NavSection.ResearchProjects:
                Navigate(AppRoutes.ResearchProjects, AppRoutes.None); break;

            case NavSection.CalculatorsJulianDay:
                Navigate(AppRoutes.MainCalculatorsHome, AppRoutes.CalculatorsJulianDay); break;
            case NavSection.CalculatorsObliquity:
                Navigate(AppRoutes.MainCalculatorsHome, AppRoutes.CalculatorsObliquity); break;

            case NavSection.ImportExportEnigma:
                Navigate(AppRoutes.MainImportExportHome, AppRoutes.ImportExportEnigma); break;
            case NavSection.ImportExportQuickChart:
                Navigate(AppRoutes.MainImportExportHome, AppRoutes.ImportExportQuickChart); break;
            case NavSection.ImportExportAaf:
                Navigate(AppRoutes.MainImportExportHome, AppRoutes.ImportExportAaf); break;
        }
        UpdateSelection();
    }

    /// <summary>Navigation for the modes without sub-items.</summary>
    private void NavigateModeWithoutSections(AppMode mode)
    {
        switch (mode)
        {
            case AppMode.Config:
                var activeConfig = _configContext.EditingConfig ?? _configContext.ActiveConfig;
                _configContext.EditingConfig = activeConfig;
                Navigate(AppRoutes.MainConfigHome, AppRoutes.ConfigEdit, new ConfigEditNavigationParameter(activeConfig.Id));
                break;
            case AppMode.Synastry:
                Navigate(AppRoutes.SynastryInput, AppRoutes.None);
                break;
        }
        UpdateSelection();
    }

    private void Navigate(string mainRoute, string detailRoute, INavigationParameter? detailParameter = null)
    {
        _navigationService.NavigateMain(mainRoute);
        _navigationService.NavigateDetail(detailRoute, detailParameter);
    }

    private void NavigateRadixMain()
    {
        var route = _chartSession.SelectedChart is not null
            ? AppRoutes.RadixChart
            : AppRoutes.MainRadixHome;
        _navigationService.NavigateMain(route);
    }

    private void NavigateRadixSelection(string selectionRoute) =>
        Navigate(selectionRoute, AppRoutes.RadixChart);

    public void ShowRadixPositionsFromCalculation(string name, FullChart chart, double latitude = 0.0, double longitude = 0.0, double height = 0.0)
    {
        _chartSession.Add(name, chart, latitude, longitude, height);
        OpenSection(NavSection.RadixPositions);
    }

    // ---------------------------------------------------------------- sidebar and menu items

    private void BuildNavigationItems()
    {
        SidebarModes = Enum.GetValues<AppMode>()
            .Select(mode => new SidebarEntry(
                Labels.Modes[mode.ToString()],
                AppNavigation.Glyph(mode),
                new RelayCommand(() => ActivateMode(mode)),
                mode,
                null))
            .ToList();
        OnPropertyChanged(nameof(SidebarModes));

        var analysisMenu = new List<AnalysisMenuEntry>();
        foreach (var group in AnalysisCatalog.Groups)
        {
            if (analysisMenu.Count > 0) analysisMenu.Add(AnalysisMenuEntry.Separator());
            analysisMenu.Add(AnalysisMenuEntry.Header(_rosetta.GetText(RbFile.RadixAnalysis, AnalysisCatalog.RbKey(group))));
            analysisMenu.AddRange(AnalysisCatalog.ItemsOf(group).Select(item => AnalysisMenuEntry.Item(
                _rosetta.GetText(RbFile.RadixAnalysis, AnalysisCatalog.RbKey(item)),
                new RelayCommand(() => OpenAnalysis(item)))));
        }
        AnalysisMenu = analysisMenu;
        OnPropertyChanged(nameof(AnalysisMenu));

        BuildSidebarSections();
    }

    private void BuildSidebarSections()
    {
        SidebarSections = AppNavigation.SectionsFor(ActiveMode)
            .Select(section => new SidebarEntry(
                Labels.Sections[section.ToString()],
                null,
                new RelayCommand(() => OpenSection(section)),
                null,
                section))
            .ToList();
        OnPropertyChanged(nameof(SidebarSections));
        OnPropertyChanged(nameof(HasSidebarSections));
        OnPropertyChanged(nameof(SidebarSectionsTitle));
        UpdateSelection();
    }

    /// <summary>The selected row shows the current location: the sub-item of the active mode, or the mode
    /// itself when that mode has no sub-items. An open analysis selects Analysis.</summary>
    private void UpdateSelection()
    {
        var current = ShowBackToAnalysis ? NavSection.RadixAnalysis : _currentSection;
        var modeHasSections = AppNavigation.SectionsFor(ActiveMode).Count > 0;
        foreach (var entry in SidebarModes)
            entry.IsSelected = !modeHasSections && entry.Mode == ActiveMode;
        foreach (var entry in SidebarSections)
            entry.IsSelected = entry.Section == current;
    }

    /// <summary>Rebuilds the labels when the language was changed in the configuration.</summary>
    private void RefreshLabelsIfLanguageChanged()
    {
        var language = _rosetta.GetLanguage();
        if (language == _labelsLanguage) return;
        _labelsLanguage = language;
        Labels = new MainWindowLabels(_rosetta);
        OnPropertyChanged(nameof(Labels));
        BuildNavigationItems();
    }

    // ---------------------------------------------------------------- panes

    private void OnNavigationServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        RefreshLabelsIfLanguageChanged();

        if (e.PropertyName == NavigationService.MainNavigatedProperty)
        {
            UpdateCurrentMainViewModel();
            return;
        }
        if (e.PropertyName != NavigationService.DetailNavigatedProperty) return;
        UpdateCurrentDetailViewModel();
        OnPropertyChanged(nameof(ShowBackToAnalysis));
        UpdateSelection();
    }

    private void UpdateCurrentMainViewModel()
    {
        CurrentMainViewModel = _routeViewModelFactory.CreateMain(
            _navigationService.CurrentMainRoute,
            _navigationService.CurrentMainParameter);
        OnPropertyChanged(nameof(ShowView1Placeholder));
    }

    private void UpdateCurrentDetailViewModel()
    {
        CurrentDetailViewModel = _routeViewModelFactory.CreateDetail(
            _navigationService.CurrentDetailRoute,
            _navigationService.CurrentDetailParameter);
        _unsavedChanges.Track(CurrentDetailViewModel);
        OnPropertyChanged(nameof(ShowView2Placeholder));
    }
}

/// <summary>A row in the sidebar: a work mode (with icon) or a sub-item of the active mode.</summary>
public sealed partial class SidebarEntry(string label, string? glyph, ICommand command, AppMode? mode, NavSection? section)
    : ObservableObject
{
    public string Label { get; } = label;
    public string? Glyph { get; } = glyph;
    public bool HasGlyph => Glyph is not null;
    public ICommand Command { get; } = command;
    public AppMode? Mode { get; } = mode;
    public NavSection? Section { get; } = section;

    [ObservableProperty]
    private bool isSelected;
}

/// <summary>An entry in the Analysis menu.</summary>
public sealed record AnalysisMenuEntry(string HeaderText, ICommand? Command, bool IsGroupHeader, bool IsSeparator)
{
    public static AnalysisMenuEntry Header(string text) => new(text, null, true, false);
    public static AnalysisMenuEntry Item(string text, ICommand command) => new(text, command, false, false);
    public static AnalysisMenuEntry Separator() => new("", null, false, true);
}
