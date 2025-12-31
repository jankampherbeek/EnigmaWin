// SERequest.cs
// EnigmaWin
// Created by Jan Kampherbeek on 31-12-2025

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.AstronCalc;

public readonly struct SERequest(
    double julianDay,
    List<Factors> factorsToUse,
    int houseSystem,
    int seFlags,
    double latitude,
    double longitude)
{
    public double JulianDay { get; } = julianDay;
    public List<Factors> FactorsToUse { get; } = factorsToUse;
    public int HouseSystem { get; } = houseSystem;
    public int SEFlags { get; } = seFlags;
    public double Latitude { get; } = latitude;
    public double Longitude { get; } = longitude;
}