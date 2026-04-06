// BoolToRowBackgroundConverter.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace EnigmaWin.Sources.Features.Shared.Converters;

/// <summary>
/// Converts a bool (IsEvenRow) to an alternating row background brush:
/// true  → white (#FFFFFF)
/// false → very light gray (#F2F2F2)
/// </summary>
public sealed class BoolToRowBackgroundConverter : IValueConverter
{
    public static readonly BoolToRowBackgroundConverter Instance = new();

    private static readonly SolidColorBrush EvenBrush = new(Colors.White);
    private static readonly SolidColorBrush OddBrush  = new(Color.Parse("#F2F2F2"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? EvenBrush : OddBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
