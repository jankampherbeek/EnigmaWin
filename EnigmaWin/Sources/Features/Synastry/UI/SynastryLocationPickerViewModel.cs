// SynastryLocationPickerViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Location;
using EnigmaWin.Sources.Features.Shared.I18n;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Synastry.UI;

/// <summary>
/// Country + city search picker producing a final (Latitude, Longitude), reusing the same
/// debounced country -> city cascading search as RadixInputViewModel, plus the same DMS
/// degree/minute/second/hemisphere ComboBox display of the resulting coordinates.
/// </summary>
public sealed class SynastryLocationPickerViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IRosetta _rosetta;
    private readonly LocationOrchestrator _locationOrchestrator = new();

    private CancellationTokenSource? _countrySearchCts;
    private CancellationTokenSource? _citySearchCts;
    private bool _suppressCountrySearch;
    private bool _suppressCitySearch;

    private string _countryQuery = string.Empty;
    private string _cityQuery = string.Empty;
    private IReadOnlyList<LocationCountry> _filteredCountries = [];
    private IReadOnlyList<LocationCity> _filteredCities = [];
    private LocationCountry? _selectedCountry;
    private LocationCity? _selectedCity;
    private bool _countryDropdownVisible;
    private bool _cityDropdownVisible;

    private int _longitudeDegree;
    private int _longitudeMinute;
    private int _longitudeSecond;
    private DisplayItem<LongitudeHemisphere> _longitudeDirection;
    private int _latitudeDegree;
    private int _latitudeMinute;
    private int _latitudeSecond;
    private DisplayItem<LatitudeHemisphere> _latitudeDirection;

    public SynastryLocationPickerViewModel(IRosetta rosetta)
    {
        _rosetta = rosetta;

        LongitudeDirectionValues = ToDisplayList<LongitudeHemisphere>(v => EnumKeySelector.Key(v));
        LatitudeDirectionValues  = ToDisplayList<LatitudeHemisphere>(v => EnumKeySelector.Key(v));
        _longitudeDirection = LongitudeDirectionValues.First(v => v.Value == LongitudeHemisphere.East);
        _latitudeDirection  = LatitudeDirectionValues.First(v => v.Value == LatitudeHemisphere.North);
    }

    public bool HasLocation => _selectedCity is not null;
    public double Latitude => ComputeDecimal(_latitudeDegree, _latitudeMinute, _latitudeSecond, _latitudeDirection.Value == LatitudeHemisphere.South);
    public double Longitude => ComputeDecimal(_longitudeDegree, _longitudeMinute, _longitudeSecond, _longitudeDirection.Value == LongitudeHemisphere.West);

    public string CountryQuery
    {
        get => _countryQuery;
        set { _countryQuery = value; OnPropertyChanged(); OnCountryQueryChanged(value); }
    }

    public string CityQuery
    {
        get => _cityQuery;
        set { _cityQuery = value; OnPropertyChanged(); OnCityQueryChanged(value); }
    }

    public IReadOnlyList<LocationCountry> FilteredCountries
    {
        get => _filteredCountries;
        private set { _filteredCountries = value; OnPropertyChanged(); }
    }

    public IReadOnlyList<LocationCity> FilteredCities
    {
        get => _filteredCities;
        private set { _filteredCities = value; OnPropertyChanged(); }
    }

    public LocationCountry? SelectedCountry
    {
        get => _selectedCountry;
        private set { _selectedCountry = value; OnPropertyChanged(); }
    }

    public LocationCity? SelectedCity
    {
        get => _selectedCity;
        private set { _selectedCity = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasLocation)); }
    }

    public bool CountryDropdownVisible
    {
        get => _countryDropdownVisible;
        private set { _countryDropdownVisible = value; OnPropertyChanged(); }
    }

    public bool CityDropdownVisible
    {
        get => _cityDropdownVisible;
        private set { _cityDropdownVisible = value; OnPropertyChanged(); }
    }

    // ── Coordinates (DMS, editable, matching RadixInputViewModel) ──────────────

    public IReadOnlyList<int> DegreeValues { get; } = Enumerable.Range(0, 181).ToList();
    public IReadOnlyList<int> LatitudeDegreeValues { get; } = Enumerable.Range(0, 90).ToList();
    public IReadOnlyList<int> MinuteSecondValues { get; } = Enumerable.Range(0, 60).ToList();
    public IReadOnlyList<DisplayItem<LongitudeHemisphere>> LongitudeDirectionValues { get; }
    public IReadOnlyList<DisplayItem<LatitudeHemisphere>> LatitudeDirectionValues { get; }

    public int LongitudeDegree
    {
        get => _longitudeDegree;
        set { _longitudeDegree = value; OnPropertyChanged(); OnPropertyChanged(nameof(Longitude)); }
    }

    public int LongitudeMinute
    {
        get => _longitudeMinute;
        set { _longitudeMinute = value; OnPropertyChanged(); OnPropertyChanged(nameof(Longitude)); }
    }

    public int LongitudeSecond
    {
        get => _longitudeSecond;
        set { _longitudeSecond = value; OnPropertyChanged(); OnPropertyChanged(nameof(Longitude)); }
    }

    public DisplayItem<LongitudeHemisphere> LongitudeDirection
    {
        get => _longitudeDirection;
        set { _longitudeDirection = value; OnPropertyChanged(); OnPropertyChanged(nameof(Longitude)); }
    }

    public int LatitudeDegree
    {
        get => _latitudeDegree;
        set { _latitudeDegree = value; OnPropertyChanged(); OnPropertyChanged(nameof(Latitude)); }
    }

    public int LatitudeMinute
    {
        get => _latitudeMinute;
        set { _latitudeMinute = value; OnPropertyChanged(); OnPropertyChanged(nameof(Latitude)); }
    }

    public int LatitudeSecond
    {
        get => _latitudeSecond;
        set { _latitudeSecond = value; OnPropertyChanged(); OnPropertyChanged(nameof(Latitude)); }
    }

    public DisplayItem<LatitudeHemisphere> LatitudeDirection
    {
        get => _latitudeDirection;
        set { _latitudeDirection = value; OnPropertyChanged(); OnPropertyChanged(nameof(Latitude)); }
    }

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelCountry   => T("view.synastry.location.country");
    public string LabelCity      => T("view.synastry.location.city");
    public string LabelLongitude => T("view.synastry.location.longitude");
    public string LabelLatitude  => T("view.synastry.location.latitude");

    private string T(string key) => _rosetta.GetText(RbFile.Synastry, key);

    // ── Search ────────────────────────────────────────────────────────────────

    private void OnCountryQueryChanged(string value)
    {
        if (_suppressCountrySearch) return;
        _countrySearchCts?.Cancel();
        _countrySearchCts = new CancellationTokenSource();
        var token = _countrySearchCts.Token;
        var q = value.Trim();
        if (string.IsNullOrEmpty(q) || q == "*") { FilteredCountries = []; CountryDropdownVisible = false; return; }
        _ = Task.Run(async () =>
        {
            await Task.Delay(400, token);
            if (token.IsCancellationRequested) return;
            var results = _locationOrchestrator.Countries(_rosetta.GetLanguage(), q);
            Application.Current.Dispatcher.Invoke(() =>
            {
                FilteredCountries = results;
                CountryDropdownVisible = results.Count > 0;
            });
        }, token);
    }

    private void OnCityQueryChanged(string value)
    {
        if (_suppressCitySearch) return;
        _citySearchCts?.Cancel();
        _citySearchCts = new CancellationTokenSource();
        var token = _citySearchCts.Token;
        var q = value.Trim();
        if (SelectedCountry is null || string.IsNullOrEmpty(q)) { FilteredCities = []; CityDropdownVisible = false; return; }
        var countryCode = SelectedCountry.Code;
        _ = Task.Run(async () =>
        {
            await Task.Delay(400, token);
            if (token.IsCancellationRequested) return;
            var results = _locationOrchestrator.Cities(countryCode, q);
            Application.Current.Dispatcher.Invoke(() =>
            {
                FilteredCities = results;
                CityDropdownVisible = results.Count > 0;
            });
        }, token);
    }

    public void SelectCountry(LocationCountry country)
    {
        _countrySearchCts?.Cancel();
        _citySearchCts?.Cancel();
        _suppressCountrySearch = true;
        _suppressCitySearch = true;
        SelectedCountry = country;
        CountryQuery = country.Name;
        FilteredCountries = [];
        CountryDropdownVisible = false;
        CityQuery = string.Empty;
        SelectedCity = null;
        FilteredCities = [];
        CityDropdownVisible = false;
        _suppressCountrySearch = false;
        _suppressCitySearch = false;
    }

    public void SelectCity(LocationCity city)
    {
        _citySearchCts?.Cancel();
        _suppressCitySearch = true;
        SelectedCity = city;
        CityQuery = city.Name;
        FilteredCities = [];
        CityDropdownVisible = false;
        _suppressCitySearch = false;

        ApplyCity(city);
    }

    private void ApplyCity(LocationCity city)
    {
        var (latDeg, latMin, latSec, latNeg) = DecimalToDms(city.Latitude);
        LatitudeDegree    = latDeg;
        LatitudeMinute    = latMin;
        LatitudeSecond    = latSec;
        LatitudeDirection = LatitudeDirectionValues.First(v => v.Value == (latNeg ? LatitudeHemisphere.South : LatitudeHemisphere.North));

        var (lonDeg, lonMin, lonSec, lonNeg) = DecimalToDms(city.Longitude);
        LongitudeDegree    = lonDeg;
        LongitudeMinute    = lonMin;
        LongitudeSecond    = lonSec;
        LongitudeDirection = LongitudeDirectionValues.First(v => v.Value == (lonNeg ? LongitudeHemisphere.West : LongitudeHemisphere.East));
    }

    private static (int deg, int min, int sec, bool negative) DecimalToDms(double value)
    {
        var negative = value < 0;
        var abs      = Math.Abs(value);
        var deg      = (int)abs;
        var minRaw   = (abs - deg) * 60.0;
        var min      = (int)minRaw;
        var sec      = (int)Math.Round((minRaw - min) * 60.0);
        return (deg, min, sec, negative);
    }

    private static double ComputeDecimal(int deg, int min, int sec, bool negative)
    {
        var value = deg + min / 60.0 + sec / 3600.0;
        return negative ? -value : value;
    }

    private IReadOnlyList<DisplayItem<T>> ToDisplayList<T>(Func<T, string> keySelector) where T : struct, Enum =>
        Enum.GetValues<T>()
            .Select(v => new DisplayItem<T>(v, _rosetta.GetText(RbFile.Localizable, keySelector(v))))
            .ToList();

    public void Dispose() => _locationOrchestrator.Dispose();

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
