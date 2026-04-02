// HarmonicsScreenViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics.UI;

public sealed class HarmonicsScreenViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly IConfigContext _configContext;
    private bool _showAllHarmonics = true;
    private FullChart? _currentChart;
    private double _harmonic = 2.0;
    private string _harmonicNumberText = "2";

    public AllHarmonicsViewModel    AllHarmonicsViewModel    { get; }
    public HarmonicsMatchesViewModel MatchesViewModel         { get; }
    public HarmonicsDrawingViewModel DrawingViewModel         { get; }

    public IRelayCommand ShowAllHarmonicsCommand { get; }
    public IRelayCommand ShowMatchesCommand      { get; }
    public IRelayCommand ShowDrawingCommand      { get; }

    public HarmonicsScreenViewModel(IRosetta rosetta, IConfigContext configContext)
    {
        _rosetta       = rosetta;
        _configContext = configContext;

        AllHarmonicsViewModel = new AllHarmonicsViewModel(rosetta, configContext);
        MatchesViewModel      = new HarmonicsMatchesViewModel(rosetta, configContext);
        DrawingViewModel      = new HarmonicsDrawingViewModel(rosetta, configContext);

        ShowAllHarmonicsCommand = new RelayCommand(() => ActiveTab = Tab.AllHarmonics);
        ShowMatchesCommand      = new RelayCommand(() => ActiveTab = Tab.Matches);
        ShowDrawingCommand      = new RelayCommand(() => ActiveTab = Tab.Drawing);
    }

    // --- Tab switching ---

    private enum Tab { AllHarmonics, Matches, Drawing }

    private Tab _activeTab = Tab.AllHarmonics;
    private Tab ActiveTab
    {
        set
        {
            if (_activeTab == value) return;
            _activeTab = value;
            OnPropertyChanged(nameof(ShowAllHarmonics));
            OnPropertyChanged(nameof(ShowMatches));
            OnPropertyChanged(nameof(ShowDrawing));
        }
    }

    public bool ShowAllHarmonics => _activeTab == Tab.AllHarmonics;
    public bool ShowMatches      => _activeTab == Tab.Matches;
    public bool ShowDrawing      => _activeTab == Tab.Drawing;

    // --- Harmonic number input ---

    /// <summary>
    /// Text bound to the harmonic number input field.
    /// On valid parse the harmonic is updated and all views are recalculated.
    /// </summary>
    public string HarmonicNumberText
    {
        get => _harmonicNumberText;
        set
        {
            if (_harmonicNumberText == value) return;
            _harmonicNumberText = value;
            OnPropertyChanged();

            if (double.TryParse(value, System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out var parsed)
                && parsed > 0)
            {
                _harmonic = parsed;
                Recalculate();
            }
        }
    }

    // --- Chart loading ---

    public void LoadChart(FullChart? chart)
    {
        _currentChart = chart;
        Recalculate();
    }

    private void Recalculate()
    {
        AllHarmonicsViewModel.Load(_currentChart, _harmonic);
        MatchesViewModel.Load(_currentChart, _harmonic);
        DrawingViewModel.Load(_currentChart, _harmonic);
    }

    // --- Localized labels ---

    public string LabelHarmonicNumber  => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.label.harmonicnumber");
    public string LabelBtnAllHarmonics => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.btn.allharmonics");
    public string LabelBtnMatches      => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.btn.matches");
    public string LabelBtnDrawing      => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.btn.drawing");

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
