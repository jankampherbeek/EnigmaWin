// CalcRequest.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.AstronCalc;

/// <summary>Calculation request</summary>
public readonly struct CalcRequest(
    double julianDay,
    List<Factors> factorsToUse,
    int houseSystem,
    int seFlags,
    double latitude,
    double longitude,
    double height,
    ConfigData configData)
{
    public double JulianDay { get; } = julianDay;
    public List<Factors> FactorsToUse { get; } = factorsToUse;
    public int HouseSystem { get; } = houseSystem;
    public int SEFlags { get; } = seFlags;
    public double Latitude { get; } = latitude;
    public double Longitude { get; } = longitude;
    public double Height { get; } = height;
    public ConfigData ConfigData { get; } = configData;

}
