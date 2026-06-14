// VspCalculator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using Serilog;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.VSP;

/// <summary>
/// Calculates Venus Star Point positions using Jean Meeus, Astronomical Algorithms pp. 249–251.
/// Returns exactly 5 chronologically sorted VSP positions around the birth Julian Day.
/// </summary>
public static class VspCalculator
{
    // SE body IDs
    private const int SeSun   = 0;
    private const int SeVenus = 3;
    private const int SeFlags = 258; // SEFLG_SWIEPH + SEFLG_SPEED

    // Meeus constants for inferior conjunction
    private const double InfA  = 2_451_996.706;
    private const double InfB  = 583.921361;
    private const double InfM0 = 82.7311;
    private const double InfM1 = 215.513058;

    // Meeus constants for superior conjunction
    private const double SupA  = 2_451_704.746;
    private const double SupB  = 583.921361;
    private const double SupM0 = 154.9745;
    private const double SupM1 = 215.513058;

    private const double VenusPeriodMax = 584.0;
    private const double VenusPeriodMin = 583.0;
    private const int    RepeatCount    = 14;
    private const double JdTolerance    = 0.1;  // ~2.4 h — used for dedup

    // Binary-search precision
    private const double Margin       = 1e-6;
    private const double MinInterval  = 1e-12;
    private const int    MaxIter      = 1000;

    public static List<VspPosition> Calculate(double birthJd)
    {
        var all = new List<(double Jd, VenusPhenomena Phen)>();

        var lastInf = birthJd - 2 * VenusPeriodMax;
        var lastSup = lastInf;

        for (var i = 0; i < RepeatCount; i++)
        {
            lastInf = FindNext(lastInf + VenusPeriodMin, VenusPhenomena.InferiorConjunction);
            all.Add((lastInf, VenusPhenomena.InferiorConjunction));
        }
        for (var i = 0; i < RepeatCount; i++)
        {
            lastSup = FindNext(lastSup + VenusPeriodMin, VenusPhenomena.SuperiorConjunction);
            all.Add((lastSup, VenusPhenomena.SuperiorConjunction));
        }

        all.Sort((a, b) => a.Jd.CompareTo(b.Jd));
        all = Deduplicate(all);

        var subset = SelectSubset(all, birthJd);
        return subset
            .Select((item, idx) => new VspPosition(idx + 1, item.Jd, item.Phen, SunLongitude(item.Jd)))
            .ToList();
    }

    private static double FindNext(double afterJd, VenusPhenomena phen)
    {
        var estimated = EstimateConjunction(afterJd, phen);
        return ExactConjunction(estimated, phen);
    }

    private static double EstimateConjunction(double seedJd, VenusPhenomena phen)
    {
        var year = JdToYear(seedJd);
        var (a, b, m0, m1) = phen == VenusPhenomena.InferiorConjunction
            ? (InfA, InfB, InfM0, InfM1)
            : (SupA, SupB, SupM0, SupM1);

        var k  = Math.Round((365.2425 * year + 1_721_060 - a) / b);
        var j0 = a + k * b;
        var m  = m0 + k * m1;
        var t  = (j0 - 2_451_545.0) / 36_525.0;

        return j0 + (phen == VenusPhenomena.InferiorConjunction
            ? CorrInferior(t, m)
            : CorrSuperior(t, m));
    }

    private static double JdToYear(double jd)
    {
        var dt = SEWrapper.DateFromJulianDay(jd, true);
        var jdNewYear = SEWrapper.JulianDay(
            new AstronomicalDate(dt.Date.Year, 1, 1),
            new AstronomicalTime(0.0));
        var yearLength = 365.25;
        return dt.Date.Year + (jd - jdNewYear) / yearLength;
    }

    private static double ExactConjunction(double estimated, VenusPhenomena phen)
    {
        var lo = estimated - 1.0;
        var hi = estimated + 1.0;
        var best = (lo + hi) / 2.0;
        var bestDiff = double.MaxValue;

        for (var iter = 0; iter < MaxIter; iter++)
        {
            if (hi - lo < MinInterval) break;

            var mid  = (lo + hi) / 2.0;
            var diff = LongitudeDiff(mid);

            if (Math.Abs(diff) < Math.Abs(bestDiff)) { bestDiff = diff; best = mid; }
            if (Math.Abs(diff) <= Margin) return mid;

            if (diff > 0)
            {
                if (phen == VenusPhenomena.InferiorConjunction) hi = mid;
                else lo = mid;
            }
            else
            {
                if (phen == VenusPhenomena.InferiorConjunction) lo = mid;
                else hi = mid;
            }
        }

        Log.Warning("VspCalculator: max iterations reached, returning best approximation");
        return best;
    }

    private static double LongitudeDiff(double jd)
    {
        var d = SunLongitude(jd) - VenusLongitude(jd);
        while (d >  180) d -= 360;
        while (d < -180) d += 360;
        return d;
    }

    private static double SunLongitude(double jd)
    {
        var pos = SEWrapper.CalculateFactorPosition(jd, SeSun, SeFlags);
        return pos?.MainPos ?? 0.0;
    }

    private static double VenusLongitude(double jd)
    {
        var pos = SEWrapper.CalculateFactorPosition(jd, SeVenus, SeFlags);
        return pos?.MainPos ?? 0.0;
    }

    private static List<(double Jd, VenusPhenomena Phen)> Deduplicate(List<(double Jd, VenusPhenomena Phen)> list)
    {
        if (list.Count <= 1) return list;
        var result = new List<(double, VenusPhenomena)> { list[0] };
        for (var i = 1; i < list.Count; i++)
        {
            if (Math.Abs(list[i].Jd - list[i - 1].Jd) > JdTolerance)
                result.Add(list[i]);
        }
        return result;
    }

    private static List<(double Jd, VenusPhenomena Phen)> SelectSubset(
        List<(double Jd, VenusPhenomena Phen)> list, double birthJd)
    {
        if (list.Count == 0) return list;

        var lastBefore = -1;
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].Jd < birthJd) lastBefore = i;
            else break;
        }

        var start = lastBefore == -1 ? 0 : Math.Max(0, lastBefore - 2);
        var end   = Math.Min(list.Count, start + 5);
        return list.GetRange(start, end - start);
    }

    // Meeus correction terms
    private static double CorrInferior(double t, double m)
    {
        var r = DegToRad(m);
        var c = -0.0096 + 0.0002 * t - 0.00001 * t * t;
        c += ( 2.0009 - 0.0033 * t - 0.00001 * t * t) * Math.Sin(r);
        c += ( 0.5980 - 0.0104 * t + 0.00001 * t * t) * Math.Cos(r);
        c += ( 0.0967 - 0.0018 * t - 0.00003 * t * t) * Math.Sin(r * 2);
        c += ( 0.0913 + 0.0009 * t - 0.00002 * t * t) * Math.Cos(r * 2);
        c += ( 0.0046 - 0.0002 * t)                    * Math.Sin(r * 3);
        c += ( 0.0079 + 0.0001 * t)                    * Math.Cos(r * 3);
        return c;
    }

    private static double CorrSuperior(double t, double m)
    {
        var r = DegToRad(m);
        var c =  0.0099 - 0.0002 * t - 0.00001 * t * t;
        c += ( 4.1991 - 0.0121 * t - 0.00003 * t * t) * Math.Sin(r);
        c += (-0.6095 + 0.0102 * t - 0.00002 * t * t) * Math.Cos(r);
        c += ( 0.2500 - 0.0028 * t - 0.00003 * t * t) * Math.Sin(r * 2);
        c += ( 0.0063 - 0.0025 * t - 0.00002 * t * t) * Math.Cos(r * 2);
        c += ( 0.0232 - 0.0005 * t - 0.00001 * t * t) * Math.Sin(r * 3);
        c += ( 0.0031 + 0.0004 * t)                    * Math.Cos(r * 3);
        return c;
    }

    private static double DegToRad(double deg) => deg * Math.PI / 180.0;
}
