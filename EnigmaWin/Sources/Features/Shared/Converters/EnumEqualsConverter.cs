// EnumEqualsConverter.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using System.Windows.Data;

namespace EnigmaWin.Sources.Features.Shared.Converters;

/// <summary>
/// Converts an enum value to bool by comparing against TargetValue.
/// Used to bind RadioButton.IsChecked to an enum property.
/// </summary>
public sealed class EnumEqualsConverter : IValueConverter
{
    public object? TargetValue { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value != null && TargetValue != null && value.Equals(TargetValue);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? TargetValue! : System.Windows.DependencyProperty.UnsetValue;
}
