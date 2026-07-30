// AstronomicalCyclesModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Periods.CyclesAstronomical.UI;

/// <summary>Shared result state for astronomical cycles, passed between input and chart screens.</summary>
public sealed class AstronomicalCyclesModel : INotifyPropertyChanged
{
    private List<(Factors Factor, List<(double JulianDay, double Position)> Series)> _singleResults = [];
    private List<List<(double JulianDay, double Difference)>> _pairResults = [];
    private List<(Factors Factor1, Factors Factor2)> _factorPairs = [];
    private Coordinates _coordinate = Coordinates.Longitude;
    private bool _isPairs;

    public List<(Factors Factor, List<(double JulianDay, double Position)> Series)> SingleResults
    {
        get => _singleResults;
        set { _singleResults = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasResults)); }
    }

    public List<List<(double JulianDay, double Difference)>> PairResults
    {
        get => _pairResults;
        set { _pairResults = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasResults)); }
    }

    public List<(Factors Factor1, Factors Factor2)> FactorPairs
    {
        get => _factorPairs;
        set { _factorPairs = value; OnPropertyChanged(); }
    }

    public Coordinates Coordinate
    {
        get => _coordinate;
        set { _coordinate = value; OnPropertyChanged(); }
    }

    public bool IsPairs
    {
        get => _isPairs;
        set { _isPairs = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasResults)); }
    }

    public bool HasResults => _isPairs ? _pairResults.Count > 0 : _singleResults.Count > 0;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
