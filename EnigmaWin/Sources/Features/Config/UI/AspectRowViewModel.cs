// AspectRowViewModel.cs
// EnigmaWin
// Created by Jan Kampherbeek on 23-03-2026

using System;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Config.UI;

/// <summary>Row VM for one aspect in the Aspect section editor.</summary>
public sealed partial class AspectRowViewModel : ObservableObject
{
    private readonly Action _onChanged;

    public Aspects Aspect     { get; }
    public string  AspectName { get; }
    public string  Glyph      { get; }

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private bool _isUsed;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private bool _isDrawn;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private decimal? _orbPercentage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewBrush))]
    [NotifyPropertyChangedFor(nameof(IsDirty))]
    private Color _currentColor;

    private bool    _origIsUsed;
    private bool    _origIsDrawn;
    private decimal _origOrbPercentage;
    private Color   _origColor;

    public bool   IsDirty     => IsUsed != _origIsUsed || IsDrawn != _origIsDrawn ||
                                 (OrbPercentage ?? 100) != _origOrbPercentage || CurrentColor != _origColor;
    public IBrush PreviewBrush => new SolidColorBrush(CurrentColor);

    public AspectRowViewModel(Aspects aspect, string aspectName, AspectSettings settings, Action onChanged)
    {
        Aspect        = aspect;
        AspectName    = aspectName;
        Glyph         = GlyphCandidates.ForAspect(aspect)[0];
        _onChanged    = onChanged;
        _isUsed           = settings.IsUsed;
        _isDrawn          = settings.IsDrawn;
        _orbPercentage    = settings.OrbPercentage;
        _currentColor     = ColorFromConfig(settings.Color);
        SaveOriginals();
    }

    partial void OnIsUsedChanged(bool value)           { if (IsDirty) _onChanged(); }
    partial void OnIsDrawnChanged(bool value)          { if (IsDirty) _onChanged(); }
    partial void OnOrbPercentageChanged(decimal? value){ if (IsDirty) _onChanged(); }
    partial void OnCurrentColorChanged(Color value)    { if (IsDirty) _onChanged(); }

    /// <summary>Resets to the given settings and clears the dirty state.</summary>
    public void ResetTo(AspectSettings settings)
    {
        IsUsed        = settings.IsUsed;
        IsDrawn       = settings.IsDrawn;
        OrbPercentage = settings.OrbPercentage;
        CurrentColor  = ColorFromConfig(settings.Color);
        SaveOriginals();
        OnPropertyChanged(nameof(IsDirty));
    }

    /// <summary>Converts the current values back to an <see cref="AspectSettings"/> for persistence.</summary>
    public AspectSettings ToAspectSettings() =>
        new(Aspect, IsUsed, IsDrawn, (int)Math.Round(OrbPercentage ?? 100), ColorToConfig(CurrentColor));

    private void SaveOriginals()
    {
        _origIsUsed        = IsUsed;
        _origIsDrawn       = IsDrawn;
        _origOrbPercentage = OrbPercentage ?? 100;
        _origColor         = CurrentColor;
    }

    private static Color ColorFromConfig(ColorConfig c) =>
        Color.FromRgb(
            (byte)Math.Clamp((int)Math.Round(c.Red   * 255), 0, 255),
            (byte)Math.Clamp((int)Math.Round(c.Green * 255), 0, 255),
            (byte)Math.Clamp((int)Math.Round(c.Blue  * 255), 0, 255));

    private static ColorConfig ColorToConfig(Color c) =>
        new(Math.Clamp(c.R / 255.0, 0, 1),
            Math.Clamp(c.G / 255.0, 0, 1),
            Math.Clamp(c.B / 255.0, 0, 1));
}
