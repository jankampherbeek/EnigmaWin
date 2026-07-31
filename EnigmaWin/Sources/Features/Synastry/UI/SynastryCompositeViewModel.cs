// SynastryCompositeViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Synastry.UI;

public sealed class SynastryCompositeViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly SynastryModel _synastryModel;
    private readonly IConfigContext _configContext;

    private bool _useReferenceLocation;
    private FullChart? _resultChart;

    public SynastryCompositeViewModel(IRosetta rosetta, SynastryModel synastryModel, IConfigContext configContext, SynastryDerivedChartViewModel chartViewModel)
    {
        _rosetta       = rosetta;
        _synastryModel = synastryModel;
        _configContext = configContext;
        ChartViewModel = chartViewModel;
        LocationPicker = new SynastryLocationPickerViewModel(rosetta);

        ShowChartCommand = new RelayCommand(ShowChart, () => HasTwoOrMore && (!_useReferenceLocation || LocationPicker.HasLocation));
        LocationPicker.PropertyChanged += (_, _) => ShowChartCommand.NotifyCanExecuteChanged();
    }

    public SynastryDerivedChartViewModel ChartViewModel { get; }
    public SynastryLocationPickerViewModel LocationPicker { get; }
    public IRelayCommand ShowChartCommand { get; }

    public bool HasTwoOrMore => _synastryModel.HasTwoOrMore;
    public bool HasResult    => _resultChart is not null;

    public bool UseReferenceLocation
    {
        get => _useReferenceLocation;
        set { _useReferenceLocation = value; OnPropertyChanged(); ShowChartCommand.NotifyCanExecuteChanged(); }
    }

    public bool UseMidpointsOnly
    {
        get => !_useReferenceLocation;
        set { UseReferenceLocation = !value; }
    }

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelTitle          => T("view.synastry.results.title.composite");
    public string LabelMethod         => T("view.synastry.composite.methodlabel");
    public string LabelMethodMidpointsOnly => T("view.synastry.composite.method.midpointsonly");
    public string LabelMethodReferenceLocation => T("view.synastry.composite.method.referencelocation");
    public string LabelShowChart      => T("view.synastry.composite.showchart");
    public string LabelHelp           => T("view.synastry.help.composite");
    public string LabelFactsheet      => T("view.synastry.composite.factsheet");

    private void ShowChart()
    {
        if (!HasTwoOrMore) return;
        if (_useReferenceLocation && !LocationPicker.HasLocation) return;

        var charts = _synastryModel.SelectedCharts.Select(c => c.Chart).ToList();
        var config = _configContext.ActiveConfig;
        var houseSystem = (int)config.CalculationConfig.HouseSystem.SeId();

        CompositeHouseMethod method = _useReferenceLocation
            ? new CompositeHouseMethod.ReferenceLocation(LocationPicker.Latitude, LocationPicker.Longitude)
            : new CompositeHouseMethod.MidpointsOnly();

        _resultChart = CompositeOrchestrator.Calculate(charts, houseSystem, method);
        ChartViewModel.SetChart(_resultChart);

        OnPropertyChanged(nameof(HasResult));
    }

    private string T(string key) => _rosetta.GetText(RbFile.Synastry, key);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
