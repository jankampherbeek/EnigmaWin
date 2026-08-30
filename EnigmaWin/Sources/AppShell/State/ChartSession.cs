// ChartSession.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.AppShell.State;

public sealed partial class ChartSession : ObservableObject, IChartSession
{
    private readonly ObservableCollection<NamedChart> _charts = [];

    public IReadOnlyList<NamedChart> Charts => _charts;

    [ObservableProperty]
    private NamedChart? _selected;

    [ObservableProperty]
    private Horoscope? _editingHoroscope;

    [ObservableProperty]
    private NamedChart? _editingNamedChart;

    public FullChart? SelectedChart => Selected?.Chart;

    public void Add(string name, FullChart chart, double latitude, double longitude, double height)
    {
        var named = new NamedChart(Guid.NewGuid(), name, chart, latitude, longitude, height);
        _charts.Add(named);
        OnPropertyChanged(nameof(Charts));
        Selected = named;
    }

    public void Select(NamedChart namedChart)
    {
        Selected = namedChart;
    }

    public void Replace(NamedChart old, string newName, FullChart newChart)
    {
        var index = _charts.IndexOf(old);
        if (index < 0) return;

        var updated = new NamedChart(old.Id, newName, newChart, old.Latitude, old.Longitude, old.Height);
        _charts[index] = updated;
        OnPropertyChanged(nameof(Charts));
        if (Selected == old)
            Selected = updated;
    }

    public void Remove(NamedChart namedChart)
    {
        _charts.Remove(namedChart);
        OnPropertyChanged(nameof(Charts));
        if (Selected == namedChart)
            Selected = _charts.LastOrDefault();
    }

    public void RemoveByName(string name)
    {
        var toRemove = _charts.Where(c => c.Name == name).ToList();
        foreach (var chart in toRemove)
            _charts.Remove(chart);
        OnPropertyChanged(nameof(Charts));
        if (Selected is not null && !_charts.Contains(Selected))
            Selected = _charts.LastOrDefault();
    }

    public void Clear()
    {
        _charts.Clear();
        OnPropertyChanged(nameof(Charts));
        Selected = null;
    }

    partial void OnSelectedChanged(NamedChart? value)
    {
        OnPropertyChanged(nameof(SelectedChart));
    }
}
