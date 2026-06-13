// StartupChartBuilder.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Linq;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Shared;

/// <summary>
/// Calculates a chart for the current moment using the active configuration and adds it
/// to IChartSession without persisting it to the database.
/// Location defaults to 0°N 0°E when the system locale cannot provide coordinates —
/// planetary positions are correct regardless; only house cusps and angles are affected.
/// </summary>
public static class StartupChartBuilder
{
    public static void Build(IChartSession chartSession, IConfigContext configContext, IRosetta rosetta)
    {
        var config = configContext.ActiveConfig;
        var factorConfig = config.FactorConfig;
        var calcConfig = config.CalculationConfig;

        var utcNow = DateTime.UtcNow;
        var date = new AstronomicalDate(utcNow.Year, utcNow.Month, utcNow.Day);
        var time = new AstronomicalTime(utcNow.Hour, utcNow.Minute, utcNow.Second);
        var julianDay = SEWrapper.JulianDay(date, time);

        var factorsToUse = factorConfig.Settings
            .Where(s => s.IsUsed)
            .Select(s => s.Factor)
            .ToList();

        var request = new CalcRequest(
            julianDay: julianDay,
            factorsToUse: factorsToUse,
            houseSystem: calcConfig.HouseSystem.SeId(),
            seFlags: 258,
            latitude: calcConfig.HomeLatitude,
            longitude: calcConfig.HomeLongitude,
            height: 0.0,
            configData: calcConfig);

        var chart = AstronCalcOrchestrator.PerformCalculation(request);
        var name = rosetta.GetText(RbFile.Localizable, "startup.chart.name");
        chartSession.Add(name, chart);
    }
}
