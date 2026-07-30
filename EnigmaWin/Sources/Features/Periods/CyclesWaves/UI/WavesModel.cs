// WavesModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Periods.CyclesWaves.UI;

/// <summary>Shared result state for waves calculations, passed between input and chart screens.</summary>
public sealed class WavesModel : INotifyPropertyChanged
{
    private Dictionary<Factors, List<(double JulianDay, double WaveValue)>> _allResults = [];
    private List<Factors> _selectedFactors = [];

    public Dictionary<Factors, List<(double JulianDay, double WaveValue)>> AllResults
    {
        get => _allResults;
        set { _allResults = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasResults)); }
    }

    public List<Factors> SelectedFactors
    {
        get => _selectedFactors;
        set { _selectedFactors = value; OnPropertyChanged(); }
    }

    public bool HasResults => _allResults.Count > 0;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
