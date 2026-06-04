// NamedHouseSystem.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Progressive.AgePoint.UI;

/// <summary>Display wrapper that pairs a HouseSystems value with a localized name.</summary>
public sealed class NamedHouseSystem(HouseSystems value, string name)
{
    public HouseSystems Value { get; } = value;
    public string       Name  { get; } = name;
}
