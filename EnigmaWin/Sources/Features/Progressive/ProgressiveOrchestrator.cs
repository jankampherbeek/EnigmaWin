// ProgressiveOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Progressive;

public class ProgressiveOrchestratorException : Exception
{
    public ProgressiveOrchestratorException(string message) : base(message) { }
}

/// <summary>
/// Dispatches progressive calculations to the appropriate technique
/// and returns the resulting positions keyed by factor.
/// </summary>
public sealed class ProgressiveOrchestrator
{
    private const double TropicalYearInDays = 365.242199074;

    private readonly IConfigContext _configContext;

    public ProgressiveOrchestrator(IConfigContext configContext)
    {
        _configContext = configContext;
    }

    // MARK: - Public API

    /// <summary>Calculates progressive positions for the given request.</summary>
    public Dictionary<Factors, ProgressivePosition> ProgressivePositions(ProgressiveCalcRequest request)
    {
        return request.Method switch
        {
            ProgressiveMethods.Transit => CalculateTransits(request),
            ProgressiveMethods.SecDir  => CalculateSecondaryDirections(request),
            ProgressiveMethods.SymDir  => [],   // use ProgressivePositions(request, symbolicKey) for symbolic directions
            _                          => []    // PrimDir, Solar, Profection, Firdaria not yet implemented
        };
    }

    /// <summary>Calculates symbolic direction positions for the given request and arc method.</summary>
    public Dictionary<Factors, ProgressivePosition> ProgressivePositions(
        ProgressiveCalcRequest request,
        SymbolicKeys symbolicKey)
    {
        return CalculateSymbolicDirections(request, symbolicKey);
    }

    // MARK: - Transits

    private Dictionary<Factors, ProgressivePosition> CalculateTransits(ProgressiveCalcRequest request)
    {
        var userConfig = _configContext.ActiveConfig;
        var factors    = userConfig.ProgressionsConfig.Transits.Factors;
        var calcConfig = userConfig.CalculationConfig;
        return ProgressiveCalculator.PerformCalculation(request, calcConfig, factors);
    }

    // MARK: - Secondary directions

    /// <summary>Each year of life equals one day: sdJD = natalJD + (eventJD − natalJD) / TropicalYear.</summary>
    private Dictionary<Factors, ProgressivePosition> CalculateSecondaryDirections(ProgressiveCalcRequest request)
    {
        if (request.NatalJulianDay is null)
            throw new ProgressiveOrchestratorException("NatalJulianDay is required for secondary directions.");

        var natalJD    = request.NatalJulianDay.Value;
        var userConfig = _configContext.ActiveConfig;
        var factors    = userConfig.ProgressionsConfig.SecondaryDirections.Factors;
        var calcConfig = userConfig.CalculationConfig;

        var ageInDays  = request.JulianDay - natalJD;
        var sdJD       = natalJD + ageInDays / TropicalYearInDays;

        var sdRequest  = new ProgressiveCalcRequest(sdJD, ProgressiveMethods.SecDir);
        return ProgressiveCalculator.PerformCalculation(sdRequest, calcConfig, factors);
    }

    // MARK: - Symbolic directions

    private Dictionary<Factors, ProgressivePosition> CalculateSymbolicDirections(
        ProgressiveCalcRequest request,
        SymbolicKeys symbolicKey)
    {
        if (request.NatalJulianDay is null)
            throw new ProgressiveOrchestratorException("NatalJulianDay is required for symbolic directions.");

        var natalJD    = request.NatalJulianDay.Value;
        var userConfig = _configContext.ActiveConfig;
        var factors    = userConfig.ProgressionsConfig.SymbolicDirections.Factors;
        var calcConfig = userConfig.CalculationConfig;

        var oneDegreeArc = (request.JulianDay - natalJD) / TropicalYearInDays;

        double arc;
        switch (symbolicKey)
        {
            case SymbolicKeys.OneDegree:
                arc = oneDegreeArc;
                break;
            case SymbolicKeys.MeanSun:
                arc = (360.0 / TropicalYearInDays) * oneDegreeArc;
                break;
            case SymbolicKeys.TrueSun:
            default:
            {
                var sunAtNatal    = SunLongitude(natalJD,                calcConfig);
                var sunAtDirected = SunLongitude(natalJD + oneDegreeArc, calcConfig);
                var diff          = (sunAtDirected - sunAtNatal) % 360.0;
                if (diff < 0) diff += 360.0;
                arc = diff;
                break;
            }
        }

        var natalRequest   = new ProgressiveCalcRequest(natalJD, ProgressiveMethods.SymDir);
        var natalPositions = ProgressiveCalculator.PerformCalculation(natalRequest, calcConfig, factors);

        var results = new Dictionary<Factors, ProgressivePosition>();
        foreach (var (factor, position) in natalPositions)
        {
            var directed = (position.Longitude + arc) % 360.0;
            if (directed < 0) directed += 360.0;
            results[factor] = new ProgressivePosition(directed, 0.0);
        }
        return results;
    }

    private double SunLongitude(double julianDay, CalculationConfig calcConfig)
    {
        var sunRequest = new ProgressiveCalcRequest(julianDay, ProgressiveMethods.SymDir);
        var result = ProgressiveCalculator.PerformCalculation(
            sunRequest, calcConfig, [Factors.Sun]);
        return result.TryGetValue(Factors.Sun, out var pos) ? pos.Longitude : 0.0;
    }
}
