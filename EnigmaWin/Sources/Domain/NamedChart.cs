// NamedChart.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;

namespace EnigmaWin.Sources.Domain;

public sealed record NamedChart(Guid Id, string Name, FullChart Chart, double Latitude, double Longitude, double Height);
