// BoolToRowBackgroundConverter.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EnigmaWin.Sources.Features.Shared.Converters;

public sealed class BoolToRowBackgroundConverter : IValueConverter
{
    public static readonly BoolToRowBackgroundConverter Instance = new();

    private static readonly SolidColorBrush EvenBrush = new(Colors.White);
    private static readonly SolidColorBrush OddBrush  = new((Color)ColorConverter.ConvertFromString("#F2F2F2"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? EvenBrush : OddBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
