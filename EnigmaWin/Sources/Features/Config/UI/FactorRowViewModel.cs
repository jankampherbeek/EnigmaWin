// FactorRowViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Config.UI;

/// <summary>Row VM for one celestial factor in the Factor section editor.</summary>
public sealed partial class FactorRowViewModel : ObservableObject
{
    private readonly Action _onChanged;

    public Factors Factor     { get; }
    public string  FactorName { get; }

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private bool _isUsed;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private bool _isDrawn;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _orbPercentage;

    private bool    _origIsUsed;
    private bool    _origIsDrawn;
    private decimal _origOrbPercentage;

    public bool IsDirty =>
        IsUsed != _origIsUsed || IsDrawn != _origIsDrawn || (OrbPercentage ?? 100) != _origOrbPercentage;

    public FactorRowViewModel(Factors factor, string factorName, FactorSettings settings, Action onChanged)
    {
        Factor        = factor;
        FactorName    = factorName;
        _onChanged    = onChanged;
        _isUsed           = settings.IsUsed;
        _isDrawn          = settings.IsDrawn;
        _orbPercentage    = settings.OrbPercentage;
        SaveOriginals();
    }

    partial void OnIsUsedChanged(bool value)           { if (IsDirty) _onChanged(); }
    partial void OnIsDrawnChanged(bool value)          { if (IsDirty) _onChanged(); }
    partial void OnOrbPercentageChanged(decimal? value) { if (IsDirty) _onChanged(); }

    /// <summary>Resets to the given settings and clears the dirty state.</summary>
    public void ResetTo(FactorSettings settings)
    {
        IsUsed        = settings.IsUsed;
        IsDrawn       = settings.IsDrawn;
        OrbPercentage = settings.OrbPercentage;
        SaveOriginals();
        OnPropertyChanged(nameof(IsDirty));
    }

    /// <summary>Converts the current values back to a <see cref="FactorSettings"/> for persistence.</summary>
    public FactorSettings ToFactorSettings() =>
        new(Factor, IsUsed, IsDrawn, (int)Math.Round(OrbPercentage ?? 100));

    private void SaveOriginals()
    {
        _origIsUsed         = IsUsed;
        _origIsDrawn        = IsDrawn;
        _origOrbPercentage  = OrbPercentage ?? 100;
    }
}
