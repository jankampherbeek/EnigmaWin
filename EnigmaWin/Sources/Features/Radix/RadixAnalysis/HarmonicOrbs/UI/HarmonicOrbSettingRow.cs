// HarmonicOrbSettingRow.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.HarmonicOrbs.UI;

/// <summary>A single bindable row in the Harmonic Orbs aspect-selection list: a checkbox, the
/// aspect's glyph/name, and its effective orb (recomputed whenever the maximum orb changes).</summary>
public sealed class HarmonicOrbSettingRow : INotifyPropertyChanged
{
    private readonly Action _onToggled;
    private bool _isSelected = true;
    private string _effectiveOrbText = "";

    public HarmonicOrbSettingRow(Domain.Aspects aspect, int harmonicNumber, string glyph, string name, bool isEvenRow, Action onToggled)
    {
        Aspect = aspect;
        HarmonicNumber = harmonicNumber;
        Glyph = glyph;
        Name = name;
        IsEvenRow = isEvenRow;
        _onToggled = onToggled;
    }

    public Domain.Aspects Aspect { get; }
    public int HarmonicNumber { get; }
    public string Glyph { get; }
    public string Name { get; }
    public bool IsEvenRow { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
            _onToggled();
        }
    }

    public string EffectiveOrbText
    {
        get => _effectiveOrbText;
        private set { if (_effectiveOrbText == value) return; _effectiveOrbText = value; OnPropertyChanged(); }
    }

    /// <summary>Recomputes the displayed effective orb ("D°MM'SS\"") from the given maximum orb.</summary>
    public void UpdateEffectiveOrb(double maxOrbDegrees)
    {
        var degrees = maxOrbDegrees / HarmonicNumber;
        var totalSeconds = (int)Math.Round(Math.Abs(degrees) * 3600.0);
        var deg = totalSeconds / 3600;
        var min = totalSeconds % 3600 / 60;
        var sec = totalSeconds % 60;
        EffectiveOrbText = $"{deg}°{min:D2}'{sec:D2}\"";
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
