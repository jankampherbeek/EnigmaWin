// IntEqualsConverter.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using System.Windows.Data;

namespace EnigmaWin.Sources.Features.Shared.Converters;

/// <summary>
/// Converts int to bool by comparing against TargetValue.
/// Used to bind RadioButton.IsChecked to an int ActiveTab property.
/// </summary>
public sealed class IntEqualsConverter : IValueConverter
{
    public int TargetValue { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i && i == TargetValue;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? TargetValue : System.Windows.DependencyProperty.UnsetValue;
}
