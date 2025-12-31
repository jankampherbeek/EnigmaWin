// AstronCalcOrchestrator.cs
// EnigmaWin
// Created by Jan Kampherbeek on 31-12-2025

using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.AstronCalc;

public static class AstronCalcOrchestrator
{
    /// <summary>
    /// Performs a full chart calculation based on the provided request.
    /// </summary>
    /// <param name="request">The SERequest containing calculation parameters.</param>
    /// <returns>A FullChart with all calculated positions and house data.</returns>
    public static FullChart PerformCalculation(SERequest request)
    {
        var julianDay = request.JulianDay;
        
        // Group factors by calculation type
        var factorsByType = request.FactorsToUse
            .GroupBy(factor => factor.CalculationType())
            .ToDictionary(g => g.Key, g => g.ToList());
        
        // Calculate factors for each calculation type
        var allCoordinates = new Dictionary<Factors, FullFactorPosition>();
        var obliquity = 0.0;
        
        // Handle CommonSe factors first (these need to calculate obliquity)
        if (factorsByType.TryGetValue(CalculationTypes.CommonSe, out var commonSeFactors) && commonSeFactors.Count > 0)
        {
            // Create a temporary request with only CommonSe factors
            var commonSeRequest = new SERequest(
                request.JulianDay,
                commonSeFactors,
                request.HouseSystem,
                request.SEFlags,
                request.Latitude,
                request.Longitude
            );
            var (commonSeCoordinates, calculatedObliquity) = SECalculation.CalculateFactors(commonSeRequest);
            foreach (var kvp in commonSeCoordinates)
            {
                allCoordinates[kvp.Key] = kvp.Value;
            }
            obliquity = calculatedObliquity;
        }
        
        // Handle other calculation types (placeholders for now)
        if (factorsByType.TryGetValue(CalculationTypes.CommonElements, out var commonElementsFactors) && commonElementsFactors.Count > 0)
        {
            var commonElementsCoordinates = CalculateCommonElementsFactors(commonElementsFactors, request);
            foreach (var kvp in commonElementsCoordinates)
            {
                allCoordinates[kvp.Key] = kvp.Value;
            }
        }
        
        if (factorsByType.TryGetValue(CalculationTypes.CommonFormulaLongitude, out var commonFormulaLongitudeFactors) && commonFormulaLongitudeFactors.Count > 0)
        {
            var commonFormulaLongitudeCoordinates = CalculateCommonFormulaLongitudeFactors(commonFormulaLongitudeFactors, request);
            foreach (var kvp in commonFormulaLongitudeCoordinates)
            {
                allCoordinates[kvp.Key] = kvp.Value;
            }
        }
        
        if (factorsByType.TryGetValue(CalculationTypes.CommonFormulaFull, out var commonFormulaFullFactors) && commonFormulaFullFactors.Count > 0)
        {
            var commonFormulaFullCoordinates = CalculateCommonFormulaFullFactors(commonFormulaFullFactors, request);
            foreach (var kvp in commonFormulaFullCoordinates)
            {
                allCoordinates[kvp.Key] = kvp.Value;
            }
        }
        
        if (factorsByType.TryGetValue(CalculationTypes.Mundane, out var mundaneFactors) && mundaneFactors.Count > 0)
        {
            var mundaneCoordinates = CalculateMundaneFactors(mundaneFactors, request);
            foreach (var kvp in mundaneCoordinates)
            {
                allCoordinates[kvp.Key] = kvp.Value;
            }
        }
        
        if (factorsByType.TryGetValue(CalculationTypes.Lots, out var lotsFactors) && lotsFactors.Count > 0)
        {
            var lotsCoordinates = CalculateLotsFactors(lotsFactors, request);
            foreach (var kvp in lotsCoordinates)
            {
                allCoordinates[kvp.Key] = kvp.Value;
            }
        }
        
        if (factorsByType.TryGetValue(CalculationTypes.ZodiacFixed, out var zodiacFixedFactors) && zodiacFixedFactors.Count > 0)
        {
            var zodiacFixedCoordinates = CalculateZodiacFixedFactors(zodiacFixedFactors, request);
            foreach (var kvp in zodiacFixedCoordinates)
            {
                allCoordinates[kvp.Key] = kvp.Value;
            }
        }
        
        if (factorsByType.TryGetValue(CalculationTypes.Apsides, out var apsidesFactors) && apsidesFactors.Count > 0)
        {
            var apsidesCoordinates = CalculateApsidesFactors(apsidesFactors, request);
            foreach (var kvp in apsidesCoordinates)
            {
                allCoordinates[kvp.Key] = kvp.Value;
            }
        }
        
        if (factorsByType.TryGetValue(CalculationTypes.Unknown, out var unknownFactors) && unknownFactors.Count > 0)
        {
            var unknownCoordinates = CalculateUnknownFactors(unknownFactors, request);
            foreach (var kvp in unknownCoordinates)
            {
                allCoordinates[kvp.Key] = kvp.Value;
            }
        }
        
        // Always calculate houses
        var housePositions = SECalculation.CalculateHouses(request, obliquity);
        
        // Calculate sidereal time
        var siderealTime = SEWrapper.SiderealTime(julianDay);   // TODO use RAMC instead
        
        return new FullChart(
            allCoordinates,
            housePositions,
            siderealTime,
            julianDay,
            obliquity
        );
    }
    
    
    /// <summary>
    /// Placeholder for CommonElements calculation.
    /// </summary>
    private static Dictionary<Factors, FullFactorPosition> CalculateCommonElementsFactors(
        List<Factors> factors,
        SERequest request)
    {
        // TODO: Implement CommonElements calculation
        var coordinates = new Dictionary<Factors, FullFactorPosition>();
        foreach (var factor in factors)
        {
            // Placeholder: return zero positions
            var zeroPosition = new FullFactorPosition(
                [new MainAstronomicalPosition(0.0, 0.0, 0.0)],
                [new MainAstronomicalPosition(0.0, 0.0, 0.0)],
                [new HorizontalPosition(0.0, 0.0)]
            );
            coordinates[factor] = zeroPosition;
        }
        return coordinates;
    }
    
    /// <summary>
    /// Placeholder for CommonFormulaLongitude calculation.
    /// </summary>
    private static Dictionary<Factors, FullFactorPosition> CalculateCommonFormulaLongitudeFactors(
        List<Factors> factors,
        SERequest request)
    {
        // TODO: Implement CommonFormulaLongitude calculation
        var coordinates = new Dictionary<Factors, FullFactorPosition>();
        foreach (var factor in factors)
        {
            // Placeholder: return zero positions
            var zeroPosition = new FullFactorPosition(
                [new MainAstronomicalPosition(0.0, 0.0, 0.0)],
                [new MainAstronomicalPosition(0.0, 0.0, 0.0)],
                [new HorizontalPosition(0.0, 0.0)]
            );
            coordinates[factor] = zeroPosition;
        }
        return coordinates;
    }
    
    /// <summary>
    /// Placeholder for CommonFormulaFull calculation.
    /// </summary>
    private static Dictionary<Factors, FullFactorPosition> CalculateCommonFormulaFullFactors(
        List<Factors> factors,
        SERequest request)
    {
        // TODO: Implement CommonFormulaFull calculation
        var coordinates = new Dictionary<Factors, FullFactorPosition>();
        foreach (var factor in factors)
        {
            // Placeholder: return zero positions
            var zeroPosition = new FullFactorPosition(
                [new MainAstronomicalPosition(0.0, 0.0, 0.0)],
                [new MainAstronomicalPosition(0.0, 0.0, 0.0)],
                [new HorizontalPosition(0.0, 0.0)]
            );
            coordinates[factor] = zeroPosition;
        }
        return coordinates;
    }
    
    /// <summary>
    /// Placeholder for Mundane calculation.
    /// </summary>
    private static Dictionary<Factors, FullFactorPosition> CalculateMundaneFactors(
        List<Factors> factors,
        SERequest request)
    {
        // TODO: Implement Mundane calculation
        var coordinates = new Dictionary<Factors, FullFactorPosition>();
        foreach (var factor in factors)
        {
            // Placeholder: return zero positions
            var zeroPosition = new FullFactorPosition(
                [new MainAstronomicalPosition(0.0, 0.0, 0.0)],
                [new MainAstronomicalPosition(0.0, 0.0, 0.0)],
                [new HorizontalPosition(0.0, 0.0)]
            );
            coordinates[factor] = zeroPosition;
        }
        return coordinates;
    }
    
    /// <summary>
    /// Placeholder for Lots calculation.
    /// </summary>
    private static Dictionary<Factors, FullFactorPosition> CalculateLotsFactors(
        List<Factors> factors,
        SERequest request)
    {
        // TODO: Implement Lots calculation
        var coordinates = new Dictionary<Factors, FullFactorPosition>();
        foreach (var factor in factors)
        {
            // Placeholder: return zero positions
            var zeroPosition = new FullFactorPosition(
                [new MainAstronomicalPosition(0.0, 0.0, 0.0)],
                [new MainAstronomicalPosition(0.0, 0.0, 0.0)],
                [new HorizontalPosition(0.0, 0.0)]
            );
            coordinates[factor] = zeroPosition;
        }
        return coordinates;
    }
    
    /// <summary>
    /// Placeholder for ZodiacFixed calculation.
    /// </summary>
    private static Dictionary<Factors, FullFactorPosition> CalculateZodiacFixedFactors(
        List<Factors> factors,
        SERequest request)
    {
        // TODO: Implement ZodiacFixed calculation
        var coordinates = new Dictionary<Factors, FullFactorPosition>();
        foreach (var factor in factors)
        {
            // Placeholder: return zero positions
            var zeroPosition = new FullFactorPosition(
                [new MainAstronomicalPosition(0.0, 0.0, 0.0)],
                [new MainAstronomicalPosition(0.0, 0.0, 0.0)],
                [new HorizontalPosition(0.0, 0.0)]
            );
            coordinates[factor] = zeroPosition;
        }
        return coordinates;
    }
    
    /// <summary>
    /// Placeholder for Apsides calculation.
    /// </summary>
    private static Dictionary<Factors, FullFactorPosition> CalculateApsidesFactors(
        List<Factors> factors,
        SERequest request)
    {
        // TODO: Implement Apsides calculation
        var coordinates = new Dictionary<Factors, FullFactorPosition>();
        foreach (var factor in factors)
        {
            // Placeholder: return zero positions
            var zeroPosition = new FullFactorPosition(
                [new MainAstronomicalPosition(0.0, 0.0, 0.0)],
                [new MainAstronomicalPosition(0.0, 0.0, 0.0)],
                [new HorizontalPosition(0.0, 0.0)]
            );
            coordinates[factor] = zeroPosition;
        }
        return coordinates;
    }
    
    /// <summary>
    /// Placeholder for Unknown calculation.
    /// </summary>
    private static Dictionary<Factors, FullFactorPosition> CalculateUnknownFactors(
        List<Factors> factors,
        SERequest request)
    {
        // TODO: Handle Unknown calculation type
        var coordinates = new Dictionary<Factors, FullFactorPosition>();
        foreach (var factor in factors)
        {
            // Placeholder: return zero positions
            var zeroPosition = new FullFactorPosition(
                [new MainAstronomicalPosition(0.0, 0.0, 0.0)],
                [new MainAstronomicalPosition(0.0, 0.0, 0.0)],
                [new HorizontalPosition(0.0, 0.0)]
            );
            coordinates[factor] = zeroPosition;
        }
        return coordinates;
    }
}