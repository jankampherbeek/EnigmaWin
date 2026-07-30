// EclipsesModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EnigmaWin.Sources.Features.Periods.Eclipses;

/// <summary>Which kind of eclipses to search for.</summary>
public enum EclipseSearchType
{
    Solar, Lunar, All
}

/// <summary>One eclipse event found by the orchestrator, either global or local to a geographic location.</summary>
/// <param name="Kind">Solar or lunar.</param>
/// <param name="DisplayJD">JD used for date/time display: local eclipse JD when local data matches, else global JD.</param>
/// <param name="Longitude">Ecliptic longitude of the Sun (solar) or Moon (lunar) at maximum eclipse.</param>
/// <param name="IsTotal">True if the eclipse is total.</param>
/// <param name="IsAnnular">True if the eclipse is annular (solar only).</param>
/// <param name="IsHybrid">True if the eclipse is hybrid / annular-total (solar only).</param>
/// <param name="IsPartial">True if the eclipse is partial.</param>
/// <param name="IsPenumbral">True if the eclipse is penumbral (lunar only).</param>
/// <param name="HasLocalData">True when a local eclipse call was made and the result corresponds to this global eclipse.</param>
/// <param name="IsVisible">Solar: SE_ECL_VISIBLE flag; Lunar: trueAltitude &gt; 0. Only meaningful when HasLocalData.</param>
/// <param name="SarosNumber">Saros series number; -99999999 when unavailable.</param>
/// <param name="SarosMemberNumber">Saros series member number; -99999999 when unavailable.</param>
public sealed record EclipseEvent(
    EclipseKind Kind,
    double DisplayJD,
    double Longitude,
    bool IsTotal,
    bool IsAnnular,
    bool IsHybrid,
    bool IsPartial,
    bool IsPenumbral,
    bool HasLocalData,
    bool IsVisible,
    double SarosNumber,
    double SarosMemberNumber);

public enum EclipseKind
{
    Solar, Lunar
}

/// <summary>Shared state between the eclipses input and result views.</summary>
public sealed class EclipsesModel : INotifyPropertyChanged
{
    private List<EclipseEvent> _results = [];
    private bool _hasLocation;

    public List<EclipseEvent> Results
    {
        get => _results;
        set { _results = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasResults)); }
    }

    public bool HasLocation
    {
        get => _hasLocation;
        set { _hasLocation = value; OnPropertyChanged(); }
    }

    public bool HasResults => _results.Count > 0;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
