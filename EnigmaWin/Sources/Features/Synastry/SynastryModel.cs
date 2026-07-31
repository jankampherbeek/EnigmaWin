// SynastryModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Synastry;

/// <summary>The birth latitude/longitude behind a selected chart, needed by techniques
/// (Combine/Davison) that derive a midpoint location. Not part of FullChart itself.</summary>
public readonly record struct GeoLocation(double Latitude, double Longitude);

/// <summary>Shared chart-selection state for the Synastry feature, independent of the main chart session.</summary>
public sealed class SynastryModel : INotifyPropertyChanged
{
    private readonly ObservableCollection<NamedChart> _selectedCharts = [];
    private readonly Dictionary<Guid, GeoLocation> _locations = [];

    public IReadOnlyList<NamedChart> SelectedCharts => _selectedCharts;

    public GeoLocation? LocationOf(NamedChart chart) =>
        _locations.TryGetValue(chart.Id, out var loc) ? loc : null;

    /// <summary>Exactly two charts selected: required for Compare, Aspect/Midpoint/Declination Comparison.</summary>
    public bool HasExactlyTwo => _selectedCharts.Count == 2;

    /// <summary>Two or more charts selected: required for Composite and Combine.</summary>
    public bool HasTwoOrMore => _selectedCharts.Count >= 2;

    public NamedChart? ChartA => _selectedCharts.Count > 0 ? _selectedCharts[0] : null;
    public NamedChart? ChartB => _selectedCharts.Count > 1 ? _selectedCharts[1] : null;

    /// <summary>Adds a chart to the selection unless the same name and Julian Day is already present.</summary>
    public void Add(NamedChart chart, GeoLocation? location = null)
    {
        if (_selectedCharts.Any(c => c.Name == chart.Name && c.Chart.JulianDay == chart.Chart.JulianDay))
            return;

        _selectedCharts.Add(chart);
        if (location is { } loc) _locations[chart.Id] = loc;
        OnPropertyChanged(nameof(SelectedCharts));
        OnPropertyChanged(nameof(HasExactlyTwo));
        OnPropertyChanged(nameof(HasTwoOrMore));
        OnPropertyChanged(nameof(ChartA));
        OnPropertyChanged(nameof(ChartB));
    }

    public void Remove(NamedChart chart)
    {
        _selectedCharts.Remove(chart);
        _locations.Remove(chart.Id);
        OnPropertyChanged(nameof(SelectedCharts));
        OnPropertyChanged(nameof(HasExactlyTwo));
        OnPropertyChanged(nameof(HasTwoOrMore));
        OnPropertyChanged(nameof(ChartA));
        OnPropertyChanged(nameof(ChartB));
    }

    public void Clear()
    {
        _selectedCharts.Clear();
        _locations.Clear();
        OnPropertyChanged(nameof(SelectedCharts));
        OnPropertyChanged(nameof(HasExactlyTwo));
        OnPropertyChanged(nameof(HasTwoOrMore));
        OnPropertyChanged(nameof(ChartA));
        OnPropertyChanged(nameof(ChartB));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
