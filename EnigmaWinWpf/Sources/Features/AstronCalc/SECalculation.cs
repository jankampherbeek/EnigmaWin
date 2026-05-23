// SECalculation.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.AstronCalc;

public static class SECalculation
{
    /// <summary>
    /// Calculates the positions for all factors in the request.
    /// </summary>
    /// <param name="request">The CalcRequest containing calculation parameters.</param>
    /// <returns>A tuple containing a dictionary of factor positions and the obliquity value.</returns>
    public static (Dictionary<Factors, FullFactorPosition> Coordinates, double Obliquity) CalculateFactors(
        CalcRequest request,
        int flagsEcliptical,
        int flagsEquatorial
        )
    {
        var julianDay = request.JulianDay;
        
        
        // Calculate positions for each factor
        var coordinates = new Dictionary<Factors, FullFactorPosition>();
        
        foreach (var factor in request.FactorsToUse)
        {
            var factorId = factor.SeId();
            
            // Calculate ecliptical position
            MainAstronomicalPosition? eclipticalPos = null;
            try
            {
                eclipticalPos = SEWrapper.CalculateFactorPosition(julianDay, factorId, flagsEcliptical);
            }
            catch (Exception)
            {
                // If calculation failed, use zero values
            }
            
            // Calculate equatorial position
            MainAstronomicalPosition? equatorialPos = null;
            try
            {
                equatorialPos = SEWrapper.CalculateFactorPosition(julianDay, factorId, flagsEquatorial);
            }
            catch (Exception)
            {
                // If calculation failed, use zero values
            }
            
            // Create arrays for FullPosition
            // Each FullPosition contains arrays, so we wrap single positions in arrays
            var eclipticalArray = new MainAstronomicalPosition[1];
            if (eclipticalPos != null)
            {
                eclipticalArray[0] = eclipticalPos;
            }
            else
            {
                // If calculation failed, use zero values
                eclipticalArray[0] = new MainAstronomicalPosition(0.0, 0.0, 0.0);
            }
            
            var equatorialArray = new MainAstronomicalPosition[1];
            var horizontalArray = new HorizontalPosition[1];
            
            if (equatorialPos != null)
            {
                equatorialArray[0] = equatorialPos;
                
                // Calculate horizontal position using equatorial coordinates
                var seWrapper = new SEWrapper();
                var horizontalCoords = seWrapper.AzimuthAndAltitude(
                    julianDay,
                    equatorialPos.MainPos,
                    equatorialPos.Deviation,
                    request.Latitude,
                    request.Longitude,
                    0.0  // Using sea level
                );
                horizontalArray[0] = new HorizontalPosition(horizontalCoords[0], horizontalCoords[1]);
            }
            else
            {
                // If calculation failed, use zero values
                equatorialArray[0] = new MainAstronomicalPosition(0.0, 0.0, 0.0);
                horizontalArray[0] = new HorizontalPosition(0.0, 0.0);
            }
            
            var fullPosition = new FullFactorPosition(
                eclipticalArray,
                equatorialArray,
                horizontalArray
            );
            
            coordinates[factor] = fullPosition;
        }
        
        // Calculate obliquity using id -1
        MainAstronomicalPosition? obliquityPosition = null;
        try
        {
            obliquityPosition = SEWrapper.CalculateFactorPosition(julianDay, -1, flagsEcliptical);
        }
        catch (Exception)
        {
            // If calculation failed, use zero
        }
        var obliquity = obliquityPosition?.MainPos ?? 0.0;
        
        return (coordinates, obliquity);
    }
    
    /// <summary>
    /// Calculates house positions (cusps, ascendant, MC, vertex, and eastpoint).
    /// </summary>
    /// <param name="request">The CalcRequest containing calculation parameters.</param>
    /// <param name="obliquity">The obliquity value needed for coordinate conversions.</param>
    /// <returns>A HousePositions struct with all calculated house data.</returns>
    public static HousePositions CalculateHouses(CalcRequest request, double obliquity)
    {
        var julianDay = request.JulianDay;
        
        // Calculate ecliptical positions of houses
        var cusps = new Dictionary<int, double>();
        var mc = 0.0;
        var ascendant = 0.0;
        var vertex = 0.0;
        var eastpoint = 0.0;
        
        try
        {
            var seWrapper = new SEWrapper();
            // Convert house system int to char (71 = 'G' for Gauquelin in ASCII)
            var houseSystemChar = (char)request.HouseSystem;
            const int flags = 0;  // Tropical zodiac
            var houseResult = seWrapper.CalculateHouses(julianDay, flags, request.Latitude, request.Longitude, houseSystemChar);
            
            var houseCusps = houseResult[0];
            var ascmc = houseResult[1];
            
            // Determine number of houses (cusps array includes index 0, so actual houses start at index 1)
            const int gauquelinIndex = 71;
            var nrOfHouses = request.HouseSystem == gauquelinIndex ? 36 : 12;
            
            // Convert cusps array to dictionary with cusp numbers as keys (indices 1-12 or 1-36)
            for (var index = 1; index <= nrOfHouses; index++)
            {
                if (index < houseCusps.Length)
                {
                    cusps[index] = houseCusps[index];
                }
            }
            
            // Extract ascendant, midheaven, vertex, and eastpoint from ascmc array
            if (ascmc.Length > 0) ascendant = ascmc[0];
            if (ascmc.Length > 1) mc = ascmc[1];
            if (ascmc.Length > 3) vertex = ascmc[3];
            if (ascmc.Length > 4) eastpoint = ascmc[4];
        }
        catch (Exception ex)
        {
            // If house calculation fails, use default values (already set above)
            Console.WriteLine($"Warning: House calculation failed: {ex.Message}");
        }

        // Create FullCuspPosition for all cusps
        var fullCusps = new List<FullCuspPosition>();
        var sortedCuspIndices = cusps.Keys.OrderBy(k => k).ToList();
        foreach (var index in sortedCuspIndices)
        {
            if (cusps.TryGetValue(index, out var longitude))
            {
                fullCusps.Add(CreateFullCuspPosition(longitude));
            }
        }
        
        // Create FullCuspPosition for ascendant, midheaven, vertex, and eastpoint
        var fullAscendant = CreateFullCuspPosition(ascendant);
        var fullMidheaven = CreateFullCuspPosition(mc);
        var fullVertex = CreateFullCuspPosition(vertex);
        var fullEastpoint = CreateFullCuspPosition(eastpoint);
        
        // Create HousePositions struct
        return new HousePositions(
            fullCusps.ToArray(),
            fullAscendant,
            fullMidheaven,
            fullEastpoint,
            fullVertex
        );

        // Convert longitude of houses to full housepositions
        FullCuspPosition CreateFullCuspPosition(double eclipticLongitude)
        {
            // Latitude for houses is always zero
            var eclipticCoords = new[] { eclipticLongitude, 0.0 };
            var seWrapper = new SEWrapper();
            var (ra, dec) = seWrapper.EclipticToEquatorial(eclipticCoords, obliquity);
            
            // Calculate horizontal position using equatorial coordinates
            var horizontalCoords = seWrapper.AzimuthAndAltitude(
                julianDay,
                ra,
                dec,
                request.Latitude,
                request.Longitude,
                0.0  // Using sea level for house positions
            );
            
            return new FullCuspPosition(
                eclipticLongitude,
                ra,
                dec,
                new HorizontalPosition(horizontalCoords[0], horizontalCoords[1])
            );
        }
    }
}
