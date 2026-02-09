// AstronCalcOrchestrator.cs
// EnigmaWin
// Created by Jan Kampherbeek on 31-12-2025

using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.AstronCalc;

public static class AstronCalcOrchestrator
{
    /// <summary>
    /// Performs a full chart calculation based on the provided request.
    /// Closely mirrors the Swift AstronCalcOrchestrator by delegating to the
    /// various calculation classes (SECalculation, ElementsCalc, FormulaCalc,
    /// FormulaFullCalc, LotsCalc, ZodiacFixedCalc, ApsidesCalc, ObliqueLongitudeCalc).
    /// </summary>
    /// <param name="request">The CalcRequest containing calculation parameters.</param>
    /// <returns>A FullChart with all calculated positions and house data.</returns>
    public static FullChart PerformCalculation(CalcRequest request)
    {
        var julianDay = request.JulianDay;

        // Define flags for SE calculations (based on config and coordinate system)
        var seFlagsEcliptical = SEFlags.DefineFlags(request.ConfigData, CoordinateSystems.Ecliptical);
        var seFlagsEquatorial = SEFlags.DefineFlags(request.ConfigData, CoordinateSystems.Equatorial);

        var seWrapper = new SEWrapper();

        // Set topocentric if requested
        if (request.ConfigData.ObserverPosition == ObserverPositions.Topocentric)
        {
            SEWrapper.SetTopocentric(request.Longitude, request.Latitude, request.Height);
        }

        // Calculate obliquity using id -1
        var obliquityPosition = SEWrapper.CalculateFactorPosition(
            julianDay,
            -1,
            2   // use SEFLG_SWIEPH, no speed
        );
        var obliquity = obliquityPosition?.MainPos ?? 0.0;

        // Calculate ayanamsha offset if needed
        var ayanamshaOffset = 0.0;
        if (request.ConfigData.Ayanamsha != Ayanamshas.Tropical)
        {
            SEWrapper.SetAyanamsha(request.ConfigData.Ayanamsha.SeId());
            ayanamshaOffset = SEWrapper.GetAyanamshaOffset(julianDay);
        }

        // Calculate houses first (needed for lots and oblique longitudes)
        var housePositions = SECalculation.CalculateHouses(request, obliquity);

        // Sidereal time from RA of MC (RAMC) as in Swift version
        var siderealTime = housePositions.Midheaven.RightAscension / 15.0;

        // Group factors by calculation type
        var factorsByType = request.FactorsToUse
            .GroupBy(factor => factor.CalculationType())
            .ToDictionary(g => g.Key, g => g.ToList());

        // Calculate factors for each calculation type
        var allCoordinates = new Dictionary<Factors, FullFactorPosition>();

        // Handle CommonSe factors
        var longitudeSun = -1.0;
        var longitudeMoon = -1.0;

        if (factorsByType.TryGetValue(CalculationTypes.CommonSe, out var commonSeFactors) &&
            commonSeFactors.Count > 0)
        {
            // Create a temporary request with only CommonSe factors
            var commonSeRequest = new CalcRequest(
                request.JulianDay,
                commonSeFactors,
                request.HouseSystem,
                request.SEFlags,
                request.Latitude,
                request.Longitude,
                request.Height,
                request.ConfigData
            );

            var (commonSeCoordinates, _) = SECalculation.CalculateFactors(
                commonSeRequest, 
                seFlagsEcliptical, 
                seFlagsEquatorial);
            foreach (var kvp in commonSeCoordinates)
            {
                allCoordinates[kvp.Key] = kvp.Value;
            }

            if (commonSeCoordinates.TryGetValue(Factors.Sun, out var sunPos) &&
                sunPos.Ecliptical.Length > 0)
            {
                longitudeSun = sunPos.Ecliptical[0].MainPos;
            }

            if (commonSeCoordinates.TryGetValue(Factors.Moon, out var moonPos) &&
                moonPos.Ecliptical.Length > 0)
            {
                longitudeMoon = moonPos.Ecliptical[0].MainPos;
            }
        }

        // Handle CommonElements factors (orbital elements based)
        if (factorsByType.TryGetValue(CalculationTypes.CommonElements, out var commonElementsFactors) &&
            commonElementsFactors.Count > 0)
        {
            var elementsRequest = new CalcRequest(
                request.JulianDay,
                commonElementsFactors,
                request.HouseSystem,
                request.SEFlags,
                request.Latitude,
                request.Longitude,
                request.Height,
                request.ConfigData
            );

            var commonElementsCoordinates = ElementsCalc.CalculateElementsFactors(
                elementsRequest,
                seWrapper,
                ayanamshaOffset
            );

            foreach (var kvp in commonElementsCoordinates)
            {
                allCoordinates[kvp.Key] = kvp.Value;
            }
        }

        // Handle CommonFormulaLongitude factors (formula-based longitude only)
        if (factorsByType.TryGetValue(CalculationTypes.CommonFormulaLongitude, out var commonFormulaLongitudeFactors) &&
            commonFormulaLongitudeFactors.Count > 0)
        {
            var commonFormulaLongitudeRequest = new CalcRequest(
                request.JulianDay,
                commonFormulaLongitudeFactors,
                request.HouseSystem,
                request.SEFlags,
                request.Latitude,
                request.Longitude,
                request.Height,
                request.ConfigData
            );

            var commonFormulaLongitudeCoordinates = FormulaCalc.CalculateFormulaFactors(
                seWrapper,
                commonFormulaLongitudeRequest,
                ayanamshaOffset
            );

            foreach (var kvp in commonFormulaLongitudeCoordinates)
            {
                allCoordinates[kvp.Key] = kvp.Value;
            }
        }

        // Handle CommonFormulaFull factors (formula-based full coordinates)
        if (factorsByType.TryGetValue(CalculationTypes.CommonFormulaFull, out var commonFormulaFullFactors) &&
            commonFormulaFullFactors.Count > 0)
        {
            var formulaFullRequest = new CalcRequest(
                request.JulianDay,
                commonFormulaFullFactors,
                request.HouseSystem,
                request.SEFlags,
                request.Latitude,
                request.Longitude,
                request.Height,
                request.ConfigData
            );

            var commonFormulaFullCoordinates = FormulaFullCalc.CalculateFormulaFullFactors(
                seWrapper,
                formulaFullRequest,
                obliquity,
                ayanamshaOffset
            );

            foreach (var kvp in commonFormulaFullCoordinates)
            {
                allCoordinates[kvp.Key] = kvp.Value;
            }
        }

        // Mundane factors are currently handled via SE houses / positions; keep placeholder for now
        if (factorsByType.TryGetValue(CalculationTypes.Mundane, out var mundaneFactors) &&
            mundaneFactors.Count > 0)
        {
            var mundaneCoordinates = CalculateMundaneFactors(mundaneFactors, request);
            foreach (var kvp in mundaneCoordinates)
            {
                allCoordinates[kvp.Key] = kvp.Value;
            }
        }

        // Handle Lots factors
        if (factorsByType.TryGetValue(CalculationTypes.Lots, out var lotsFactors) &&
            lotsFactors.Count > 0)
        {
            var ascendantLongitude = housePositions.Ascendant.Longitude;
            var sunLongitude = longitudeSun > 0.0
                ? longitudeSun
                : (allCoordinates.TryGetValue(Factors.Sun, out var sunPos) &&
                   sunPos.Ecliptical.Length > 0
                    ? sunPos.Ecliptical[0].MainPos
                    : -1.0);

            var moonLongitude = longitudeMoon > 0.0
                ? longitudeMoon
                : (allCoordinates.TryGetValue(Factors.Moon, out var moonPos) &&
                   moonPos.Ecliptical.Length > 0
                    ? moonPos.Ecliptical[0].MainPos
                    : -1.0);

            var isDayChart = allCoordinates.TryGetValue(Factors.Sun, out var sunFullPos) &&
                             sunFullPos.Horizontal.Length > 0 &&
                             sunFullPos.Horizontal[0].Altitude >= 0.0;

            if (sunLongitude > 0.0 && moonLongitude > 0.0)
            {
                var lotsRequest = new CalcRequest(
                    request.JulianDay,
                    lotsFactors,
                    request.HouseSystem,
                    request.SEFlags,
                    request.Latitude,
                    request.Longitude,
                    request.Height,
                    request.ConfigData
                );

                var lotsCoordinates = LotsCalc.CalculateLotsFactors(
                    seWrapper,
                    lotsRequest,
                    obliquity,
                    ascendantLongitude,
                    sunLongitude,
                    moonLongitude,
                    isDayChart,
                    ayanamshaOffset
                );

                foreach (var kvp in lotsCoordinates)
                {
                    allCoordinates[kvp.Key] = kvp.Value;
                }
            }
        }

        // Handle ZodiacFixed factors
        if (factorsByType.TryGetValue(CalculationTypes.ZodiacFixed, out var zodiacFixedFactors) &&
            zodiacFixedFactors.Count > 0)
        {
            var zodiacRequest = new CalcRequest(
                request.JulianDay,
                zodiacFixedFactors,
                request.HouseSystem,
                request.SEFlags,
                request.Latitude,
                request.Longitude,
                request.Height,
                request.ConfigData
            );

            var zodiacFixedCoordinates = ZodiacFixedCalc.ZodiacFixedFactors(
                zodiacRequest,
                obliquity,
                seWrapper
            );

            foreach (var kvp in zodiacFixedCoordinates)
            {
                allCoordinates[kvp.Key] = kvp.Value;
            }
        }

        // Handle Apsides factors
        if (factorsByType.TryGetValue(CalculationTypes.Apsides, out var apsidesFactors) &&
            apsidesFactors.Count > 0)
        {
            var apsidesRequest = new CalcRequest(
                request.JulianDay,
                apsidesFactors,
                request.HouseSystem,
                request.SEFlags,
                request.Latitude,
                request.Longitude,
                request.Height,
                request.ConfigData
            );

            var apsidesCoordinates = ApsidesCalc.CalculateApsidesFactors(
                apsidesRequest,
                obliquity,
                ayanamshaOffset,
                seFlagsEcliptical,
                seWrapper
            );

            foreach (var kvp in apsidesCoordinates)
            {
                allCoordinates[kvp.Key] = kvp.Value;
            }
        }

        // Handle Unknown factors (still placeholder)
        if (factorsByType.TryGetValue(CalculationTypes.Unknown, out var unknownFactors) &&
            unknownFactors.Count > 0)
        {
            var unknownCoordinates = CalculateUnknownFactors(unknownFactors, request);
            foreach (var kvp in unknownCoordinates)
            {
                allCoordinates[kvp.Key] = kvp.Value;
            }
        }

        // Optional oblique longitude projection
        if (request.ConfigData.ProjectionType == ProjectionTypes.ObliqueLongitude)
        {
            var armc = housePositions.Midheaven.RightAscension;

            // Build array of NamedEclipticCoordinates from allCoordinates
            var factorCoordinates = new List<NamedEclipticCoordinates>();
            foreach (var (factor, position) in allCoordinates)
            {
                if (position.Ecliptical.Length > 0)
                {
                    var ecl = position.Ecliptical[0];
                    factorCoordinates.Add(new NamedEclipticCoordinates(
                        factor,
                        ecl.MainPos,
                        ecl.Deviation
                    ));
                }
            }

            var obliqueLongitudes = ObliqueLongitudeCalc.ObliqueLongitudeForFactor(
                armc,
                obliquity,
                request.Latitude,
                factorCoordinates.ToArray(),
                ayanamshaOffset
            );

            var obliqueLongitudeMap = obliqueLongitudes.ToDictionary(
                x => x.Factor,
                x => x.ObliqueLongitude
            );

            var updatedCoordinates = new Dictionary<Factors, FullFactorPosition>();
            foreach (var (factor, position) in allCoordinates)
            {
                if (obliqueLongitudeMap.TryGetValue(factor, out var obliqueLongitude) &&
                    position.Ecliptical.Length > 0)
                {
                    var updatedEcliptical = new MainAstronomicalPosition[position.Ecliptical.Length];
                    for (var i = 0; i < position.Ecliptical.Length; i++)
                    {
                        var ecl = position.Ecliptical[i];
                        if (i == 0)
                        {
                            updatedEcliptical[i] = new MainAstronomicalPosition(
                                MainPos: obliqueLongitude,
                                Deviation: ecl.Deviation,
                                Distance: ecl.Distance,
                                MainPosSpeed: ecl.MainPosSpeed,
                                DeviationSpeed: ecl.DeviationSpeed,
                                DistanceSpeed: ecl.DistanceSpeed
                            );
                        }
                        else
                        {
                            updatedEcliptical[i] = ecl;
                        }
                    }

                    var updatedPosition = new FullFactorPosition(
                        Ecliptical: updatedEcliptical,
                        Equatorial: position.Equatorial,
                        Horizontal: position.Horizontal
                    );

                    updatedCoordinates[factor] = updatedPosition;
                }
                else
                {
                    updatedCoordinates[factor] = position;
                }
            }

            allCoordinates = updatedCoordinates;
        }

        return new FullChart(
            allCoordinates,
            housePositions,
            siderealTime,
            julianDay,
            obliquity
        );
    }


    /// <summary>
    /// Placeholder for Mundane calculation.
    /// </summary>
    private static Dictionary<Factors, FullFactorPosition> CalculateMundaneFactors(
        List<Factors> factors,
        CalcRequest request)
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
    /// Placeholder for Unknown calculation.
    /// </summary>
    private static Dictionary<Factors, FullFactorPosition> CalculateUnknownFactors(
        List<Factors> factors,
        CalcRequest request)
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