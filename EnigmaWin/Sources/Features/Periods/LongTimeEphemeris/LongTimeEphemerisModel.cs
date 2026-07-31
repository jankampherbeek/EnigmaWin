// LongTimeEphemerisModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Periods.LongTimeEphemeris;

/// <summary>Shared state between the long time ephemeris input and result views.</summary>
public sealed class LongTimeEphemerisModel : INotifyPropertyChanged
{
    public const int MaxRows = 500_000;

    private List<LongTimeEphemerisRow> _rows = [];
    private List<Factors> _factors = [];
    private LongTimeEphemerisCoordinate _selectedCoordinate = LongTimeEphemerisCoordinate.Longitude;
    private LongTimeEphemerisDisplayFormat _displayFormat = LongTimeEphemerisDisplayFormat.Dms;

    public List<LongTimeEphemerisRow> Rows
    {
        get => _rows;
        set { _rows = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasResults)); }
    }

    public List<Factors> Factors
    {
        get => _factors;
        set { _factors = value; OnPropertyChanged(); }
    }

    public LongTimeEphemerisCoordinate SelectedCoordinate
    {
        get => _selectedCoordinate;
        set { _selectedCoordinate = value; OnPropertyChanged(); }
    }

    public LongTimeEphemerisDisplayFormat DisplayFormat
    {
        get => _displayFormat;
        set { _displayFormat = value; OnPropertyChanged(); }
    }

    public bool HasResults => _rows.Count > 0;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
