// LongTimeEphemerisOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Periods.LongTimeEphemeris;

/// <summary>Progress snapshot for a running long time ephemeris calculation.</summary>
public sealed record LongTimeEphemerisProgress(int RowsDone, int TotalRows)
{
    public double Fraction => TotalRows > 0 ? (double)RowsDone / TotalRows : 0.0;
}

/// <summary>Runs a long time ephemeris calculation on the thread pool, reporting progress and honouring cancellation.</summary>
public static class LongTimeEphemerisOrchestrator
{
    private const int BatchSize = 500;

    public static Task<List<LongTimeEphemerisRow>> RunAsync(
        double jdStart, double jdEnd, double intervalInDays,
        IReadOnlyList<Factors> factors, LongTimeEphemerisCoordinate coordinate,
        ObserverPositions observerPosition, Ayanamshas ayanamsha,
        IProgress<LongTimeEphemerisProgress> progress, CancellationToken ct) =>
        Task.Run(() => Execute(jdStart, jdEnd, intervalInDays, factors, coordinate,
            observerPosition, ayanamsha, progress, ct), ct);

    private static List<LongTimeEphemerisRow> Execute(
        double jdStart, double jdEnd, double intervalInDays,
        IReadOnlyList<Factors> factors, LongTimeEphemerisCoordinate coordinate,
        ObserverPositions observerPosition, Ayanamshas ayanamsha,
        IProgress<LongTimeEphemerisProgress> progress, CancellationToken ct)
    {
        var jdValues = new List<double>();
        for (var jd = jdStart; jd <= jdEnd + 1e-9; jd += intervalInDays)
            jdValues.Add(jd);

        var total = jdValues.Count;
        var results = new List<LongTimeEphemerisRow>(total);
        if (total == 0) return results;

        var config = new CalculationConfig(
            HouseSystems.NoHouses, ayanamsha, observerPosition,
            ProjectionTypes.TwoDimensional, LunarNodeTypes.MeanNode, LotsTypes.Sect);

        if (ayanamsha != Ayanamshas.Tropical)
            SEWrapper.SetAyanamsha(ayanamsha.SeId());

        var seWrapper = new SEWrapper();

        progress.Report(new LongTimeEphemerisProgress(0, total));

        for (var i = 0; i < total; i++)
        {
            ct.ThrowIfCancellationRequested();

            var jd = jdValues[i];
            var values = LongTimeEphemerisCalculator.CalculateRow(jd, factors, coordinate, config, seWrapper, ayanamsha);
            var dateTime = SEWrapper.DateFromJulianDay(jd, gregorian: true);
            results.Add(new LongTimeEphemerisRow(i, jd, FormatDateTime(dateTime), values));

            if ((i + 1) % BatchSize == 0 || i == total - 1)
                progress.Report(new LongTimeEphemerisProgress(i + 1, total));
        }

        return results;
    }

    private static string FormatDateTime(AstronomicalDateTime dt)
    {
        var y = dt.Date.Year;
        var yearStr = y is >= 0 and <= 9999 ? y.ToString("D4") : y.ToString();
        return $"{yearStr}/{dt.Date.Month:D2}/{dt.Date.Day:D2} {dt.Time.Hour:D2}:{dt.Time.Minute:D2}:{dt.Time.Second:D2}";
    }
}
