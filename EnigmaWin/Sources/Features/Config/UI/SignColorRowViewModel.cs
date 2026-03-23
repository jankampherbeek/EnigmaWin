// SignColorRowViewModel.cs
// EnigmaWin
// Created by Jan Kampherbeek on 22-03-2026

using System;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Config.UI;

/// <summary>Row VM for one zodiac sign's background color in the Display section editor.</summary>
public sealed partial class SignColorRowViewModel : ObservableObject
{
    private readonly Action _onChanged;
    private bool _resetting;

    public Signs Sign     { get; }
    public string SignName { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewBrush))]
    private Color _currentColor;

    /// <summary>Brush derived from <see cref="CurrentColor"/> — used to paint the swatch button.</summary>
    public IBrush PreviewBrush => new SolidColorBrush(CurrentColor);

    public SignColorRowViewModel(Signs sign, string signName, ColorConfig color, Action onChanged)
    {
        Sign          = sign;
        SignName      = signName;
        _onChanged    = onChanged;
        _currentColor = ColorFromConfig(color);   // set backing field directly — skips the partial callback
    }

    partial void OnCurrentColorChanged(Color value)
    {
        if (!_resetting) _onChanged();
    }

    private static Color ColorFromConfig(ColorConfig c) =>
        Color.FromRgb(
            (byte)Math.Clamp((int)Math.Round(c.Red   * 255), 0, 255),
            (byte)Math.Clamp((int)Math.Round(c.Green * 255), 0, 255),
            (byte)Math.Clamp((int)Math.Round(c.Blue  * 255), 0, 255));

    /// <summary>Converts the current color back to a <see cref="ColorConfig"/> for persistence.</summary>
    public ColorConfig ToColorConfig() => new(
        Math.Clamp(CurrentColor.R / 255.0, 0, 1),
        Math.Clamp(CurrentColor.G / 255.0, 0, 1),
        Math.Clamp(CurrentColor.B / 255.0, 0, 1));

    /// <summary>Resets to the given color without firing the dirty callback.</summary>
    public void ResetColor(ColorConfig color)
    {
        _resetting = true;
        CurrentColor = ColorFromConfig(color);
        _resetting = false;
    }
}
