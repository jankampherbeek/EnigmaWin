// EphemerisModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Periods.Ephemeris;

/// <summary>Which single coordinate to display in the ephemeris.</summary>
public enum EphemerisCoordinate
{
    Longitude, Latitude, RightAscension, Declination, Distance, SpeedLongitude, SpeedDeclination
}

/// <summary>Shared state between the ephemeris input and result views.</summary>
public sealed class EphemerisModel : INotifyPropertyChanged
{
    private List<EphemerisRow> _rows = [];
    private List<Factors> _factors = [];
    private EphemerisCoordinate _selectedCoord = EphemerisCoordinate.Longitude;
    private bool _isHeliocentric = false;
    private Ayanamshas _ayanamsha = Ayanamshas.Tropical;

    public List<EphemerisRow> Rows
    {
        get => _rows;
        set { _rows = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasResults)); }
    }

    public List<Factors> Factors
    {
        get => _factors;
        set { _factors = value; OnPropertyChanged(); }
    }

    public EphemerisCoordinate SelectedCoord
    {
        get => _selectedCoord;
        set { _selectedCoord = value; OnPropertyChanged(); }
    }

    public bool IsHeliocentric
    {
        get => _isHeliocentric;
        set { _isHeliocentric = value; OnPropertyChanged(); }
    }

    public Ayanamshas Ayanamsha
    {
        get => _ayanamsha;
        set { _ayanamsha = value; OnPropertyChanged(); }
    }

    public bool HasResults => _rows.Count > 0;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
