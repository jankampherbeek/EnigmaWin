// DisplayItem.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Shared.I18n;

/// <summary>Wraps an enum value with its localized display text for use in UI pickers.</summary>
public sealed record DisplayItem<T>(T Value, string DisplayText)
{
    public override string ToString() => DisplayText;
}
