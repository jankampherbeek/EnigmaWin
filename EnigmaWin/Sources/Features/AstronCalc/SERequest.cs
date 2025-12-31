// SERequest.cs
// EnigmaWin
// Created by Jan Kampherbeek on 31-12-2025

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.AstronCalc;

public struct SERequest
{
    public double JulianDay { get; }
    public List<Factors> FactorsToUse { get; }
    public int HouseSystem { get; }
    public int SEFlags { get; }
    public double Latitude { get; }
    public double Longitude { get; }
    
    public SERequest(double julianDay, List<Factors> factorsToUse, int houseSystem, int seFlags, double latitude, double longitude)
    {
        JulianDay = julianDay;
        FactorsToUse = factorsToUse;
        HouseSystem = houseSystem;
        SEFlags = seFlags;
        Latitude = latitude;
        Longitude = longitude;
    }
}