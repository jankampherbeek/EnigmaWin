// PreNatalOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;

namespace EnigmaWin.Sources.Features.Progressive.PreNatal;

public abstract record PreNatalMomentKind;
public sealed record SolarEclipseKind : PreNatalMomentKind;
public sealed record LunarEclipseKind : PreNatalMomentKind;
public sealed record IngressKind(Factors Factor, Signs Sign) : PreNatalMomentKind;
public sealed record DirectKind(Factors Factor) : PreNatalMomentKind;
public sealed record RetrogradeKind(Factors Factor) : PreNatalMomentKind;
public sealed record AspectKind(Aspects AspectType, Factors Factor1, Factors Factor2) : PreNatalMomentKind;

public sealed record PreNatalMoment(
    double PrenatalJD, double ActualJD,
    string PrenatalDateTxt, string ActualDateTxt,
    PreNatalMomentKind Kind,
    double Longitude1, double? Longitude2);

public static class PreNatalOrchestrator
{
    private const double TropicalYear          = 365.242199074;
    private const double ConceptionPeriod      = 273.217;
    private const double ConversionFactor      = TropicalYear * (70.0 / ConceptionPeriod);
    private const double MarginBeforeConception = 30.0;
    private const double MaxLifetimeInPrenatalDays = 390.31;

    public static readonly Factors[] DefaultFactors =
    [
        Factors.Sun, Factors.Mercury, Factors.Venus, Factors.Mars,
        Factors.Jupiter, Factors.Saturn, Factors.Uranus, Factors.Neptune, Factors.Pluto
    ];

    public static readonly Aspects[] DefaultAspects = [Aspects.Conjunction];

    public static readonly Factors[] SelectableFactors =
    [
        Factors.Sun, Factors.Moon, Factors.Mercury, Factors.Venus, Factors.Mars,
        Factors.Jupiter, Factors.Saturn, Factors.Uranus, Factors.Neptune, Factors.Pluto,
        Factors.NorthNodeMean, Factors.Chiron, Factors.Pholus, Factors.Ceres, Factors.Pallas,
        Factors.Juno, Factors.Vesta,
        Factors.ApogeeMean, Factors.ApogeeCorrected, Factors.ApogeeInterpolated,
        Factors.CupidoUra, Factors.HadesUra, Factors.ZeusUra, Factors.KronosUra,
        Factors.ApollonUra, Factors.AdmetosUra, Factors.VulcanusUra, Factors.PoseidonUra,
        Factors.Isis, Factors.Eris, Factors.Nessus, Factors.Huya, Factors.Varuna,
        Factors.Ixion, Factors.Quaoar, Factors.Haumea, Factors.Orcus, Factors.Makemake,
        Factors.Sedna, Factors.Hygieia, Factors.Astraea
    ];

    public static List<PreNatalMoment> Calculate(
        double natalJD, double conceptionJD,
        Factors[] selectedFactors, Aspects[] selectedAspects,
        bool useAspects, bool useEclipses, bool useIngresses, bool useRetroDirect)
    {
        var startJD = conceptionJD - MarginBeforeConception;
        var baseConceptionJD = natalJD - ConceptionPeriod;
        var endJD = baseConceptionJD + MaxLifetimeInPrenatalDays;

        var moments = new List<PreNatalMoment>();

        if (useEclipses)
            moments.AddRange(FindEclipses(startJD, endJD, conceptionJD, natalJD));
        if (useIngresses)
            moments.AddRange(FindIngresses(selectedFactors, startJD, endJD, conceptionJD, natalJD));
        if (useRetroDirect)
            moments.AddRange(FindRetroDirectMoments(selectedFactors, startJD, endJD, conceptionJD, natalJD));
        if (useAspects)
            moments.AddRange(FindAspects(selectedFactors, selectedAspects, startJD, endJD, conceptionJD, natalJD));

        moments.Sort((a, b) => a.PrenatalJD.CompareTo(b.PrenatalJD));
        return moments;
    }

    private static List<PreNatalMoment> FindEclipses(
        double startJD, double endJD, double conceptionJD, double natalJD)
    {
        var result = new List<PreNatalMoment>();

        var runJD = startJD;
        while (runJD < endJD)
        {
            var eclJD = SEWrapper.NextSolarEclipse(runJD);
            if (eclJD is null || eclJD.Value > endJD) break;
            var lon = CalcLon(Factors.Sun, eclJD.Value);
            result.Add(Make(eclJD.Value, conceptionJD, natalJD, new SolarEclipseKind(), lon, null));
            runJD = eclJD.Value + 1.0;
        }

        runJD = startJD;
        while (runJD < endJD)
        {
            var eclJD = SEWrapper.NextLunarEclipse(runJD);
            if (eclJD is null || eclJD.Value > endJD) break;
            var lon = CalcLon(Factors.Sun, eclJD.Value);
            result.Add(Make(eclJD.Value, conceptionJD, natalJD, new LunarEclipseKind(), lon, null));
            runJD = eclJD.Value + 1.0;
        }

        return result;
    }

    private static List<PreNatalMoment> FindIngresses(
        Factors[] factors, double startJD, double endJD, double conceptionJD, double natalJD)
    {
        var result = new List<PreNatalMoment>();
        foreach (var factor in factors)
            result.AddRange(FindIngressesFor(factor, startJD, endJD, conceptionJD, natalJD));
        return result;
    }

    private static List<PreNatalMoment> FindIngressesFor(
        Factors factor, double startJD, double endJD, double conceptionJD, double natalJD)
    {
        const double margin = 1e-6;
        var result = new List<PreNatalMoment>();

        void Search(double jdA, double jdB, double lonA, double lonB, double step)
        {
            if (step < margin)
            {
                var signIdx = (int)(lonB / 30.0);
                var sign = (Signs)(signIdx + 1);
                result.Add(Make(jdB, conceptionJD, natalJD, new IngressKind(factor, sign), lonB, null));
                return;
            }
            var fineStep = step / 10.0;
            var curJD  = jdA;
            var curLon = lonA;
            while (curJD < jdB)
            {
                var nxtJD  = Math.Min(curJD + fineStep, jdB);
                var nxtLon = CalcLon(factor, nxtJD);
                if ((int)(curLon / 30.0) != (int)(nxtLon / 30.0))
                    Search(curJD, nxtJD, curLon, nxtLon, fineStep);
                curJD  = nxtJD;
                curLon = nxtLon;
            }
        }

        var curJD  = startJD;
        var curLon = CalcLon(factor, curJD);
        while (curJD < endJD)
        {
            var nxtJD  = Math.Min(curJD + 1.0, endJD);
            var nxtLon = CalcLon(factor, nxtJD);
            if ((int)(curLon / 30.0) != (int)(nxtLon / 30.0))
                Search(curJD, nxtJD, curLon, nxtLon, 1.0);
            curJD  = nxtJD;
            curLon = nxtLon;
        }
        return result;
    }

    private static List<PreNatalMoment> FindRetroDirectMoments(
        Factors[] factors, double startJD, double endJD, double conceptionJD, double natalJD)
    {
        var result = new List<PreNatalMoment>();
        foreach (var factor in factors.Where(f => f != Factors.Sun && f != Factors.Moon))
            result.AddRange(FindRetroDirectFor(factor, startJD, endJD, conceptionJD, natalJD));
        return result;
    }

    private static List<PreNatalMoment> FindRetroDirectFor(
        Factors factor, double startJD, double endJD, double conceptionJD, double natalJD)
    {
        const double tolerance = 1e-7;
        var result   = new List<PreNatalMoment>();
        var curJD    = startJD;
        var curSpeed = CalcSpeed(factor, curJD);

        while (curJD < endJD)
        {
            var nxtJD    = Math.Min(curJD + 1.0, endJD);
            var nxtSpeed = CalcSpeed(factor, nxtJD);

            if (curSpeed * nxtSpeed < 0)
            {
                var lo = curJD; var hi = nxtJD;
                var loSpeed = curSpeed;
                while (hi - lo > tolerance)
                {
                    var mid      = (lo + hi) / 2.0;
                    var midSpeed = CalcSpeed(factor, mid);
                    if (loSpeed * midSpeed <= 0) { hi = mid; }
                    else { lo = mid; loSpeed = midSpeed; }
                }
                var exactJD = (lo + hi) / 2.0;
                var lon     = CalcLon(factor, exactJD);
                PreNatalMomentKind kind = curSpeed > 0
                    ? new RetrogradeKind(factor)
                    : new DirectKind(factor);
                result.Add(Make(exactJD, conceptionJD, natalJD, kind, lon, null));
            }

            curJD    = nxtJD;
            curSpeed = nxtSpeed;
        }
        return result;
    }

    private static List<PreNatalMoment> FindAspects(
        Factors[] factors, Aspects[] aspects,
        double startJD, double endJD, double conceptionJD, double natalJD)
    {
        var result = new List<PreNatalMoment>();
        for (var i = 0; i < factors.Length; i++)
        for (var j = i + 1; j < factors.Length; j++)
            result.AddRange(FindAspectsForPair(factors[i], factors[j], aspects, startJD, endJD, conceptionJD, natalJD));
        return result;
    }

    private static List<PreNatalMoment> FindAspectsForPair(
        Factors f1, Factors f2, Aspects[] aspects,
        double startJD, double endJD, double conceptionJD, double natalJD)
    {
        var step   = StepSize(f1, f2);
        var result = new List<PreNatalMoment>();
        var curJD  = startJD;
        var lon1   = CalcLon(f1, curJD);
        var lon2   = CalcLon(f2, curJD);

        while (curJD < endJD)
        {
            var nxtJD  = Math.Min(curJD + step, endJD);
            var nLon1  = CalcLon(f1, nxtJD);
            var nLon2  = CalcLon(f2, nxtJD);

            foreach (var aspect in aspects)
            {
                var dCur = Dist(lon1, lon2, aspect);
                var dNxt = Dist(nLon1, nLon2, aspect);
                if (dCur * dNxt < 0 && (Math.Abs(dCur) + Math.Abs(dNxt)) < 90.0)
                    result.AddRange(RefineAspect(f1, f2, aspect, curJD, nxtJD,
                        lon1, lon2, nLon1, nLon2, conceptionJD, natalJD));
            }

            curJD = nxtJD; lon1 = nLon1; lon2 = nLon2;
        }
        return result;
    }

    private static List<PreNatalMoment> RefineAspect(
        Factors f1, Factors f2, Aspects aspect,
        double jdA, double jdB,
        double lon1A, double lon2A, double lon1B, double lon2B,
        double conceptionJD, double natalJD)
    {
        if (jdB - jdA < 1e-7)
            return [Make((jdA + jdB) / 2.0, conceptionJD, natalJD,
                new AspectKind(aspect, f1, f2), lon1A, lon2A)];

        var mid   = (jdA + jdB) / 2.0;
        var lon1M = CalcLon(f1, mid);
        var lon2M = CalcLon(f2, mid);
        var dA    = Dist(lon1A, lon2A, aspect);
        var dM    = Dist(lon1M, lon2M, aspect);

        return dA * dM <= 0
            ? RefineAspect(f1, f2, aspect, jdA, mid, lon1A, lon2A, lon1M, lon2M, conceptionJD, natalJD)
            : RefineAspect(f1, f2, aspect, mid, jdB, lon1M, lon2M, lon1B, lon2B, conceptionJD, natalJD);
    }

    private static double CalcLon(Factors factor, double jd)
        => SEWrapper.CalculateFactorPosition(jd, factor.SeId(), 2)?.MainPos ?? 0.0;

    private static double CalcSpeed(Factors factor, double jd)
        => SEWrapper.CalculateFactorPosition(jd, factor.SeId(), 258)?.MainPosSpeed ?? 0.0;

    private static double Dist(double lon1, double lon2, Aspects aspect)
    {
        var diff = (lon1 - lon2) % 360.0;
        if (diff < 0) diff += 360.0;
        var angle = aspect.Angle();
        var d1 = diff - angle;
        var d2 = diff - (360.0 - angle);
        return Math.Abs(d1) <= Math.Abs(d2) ? d1 : d2;
    }

    private static double StepSize(Factors f1, Factors f2)
    {
        if (f1 == Factors.Moon || f2 == Factors.Moon) return 0.1;
        var inners = new HashSet<Factors> { Factors.Sun, Factors.Mercury, Factors.Venus, Factors.Mars };
        return (inners.Contains(f1) || inners.Contains(f2)) ? 0.5 : 1.0;
    }

    private static PreNatalMoment Make(
        double prenatalJD, double conceptionJD, double natalJD,
        PreNatalMomentKind kind, double lon1, double? lon2)
    {
        var actualJD = ToActual(prenatalJD, conceptionJD, natalJD);
        return new PreNatalMoment(
            PrenatalJD:      prenatalJD,
            ActualJD:        actualJD,
            PrenatalDateTxt: MakeDateTime(prenatalJD),
            ActualDateTxt:   MakeDate(actualJD),
            Kind:            kind,
            Longitude1:      lon1,
            Longitude2:      lon2);
    }

    private static double ToActual(double prenatalJD, double conceptionJD, double natalJD)
        => (prenatalJD - conceptionJD) * ConversionFactor + natalJD;

    private static string MakeDate(double jd)
    {
        var dt = SEWrapper.DateFromJulianDay(jd, true);
        return $"{dt.Date.Year:D4}/{dt.Date.Month:D2}/{dt.Date.Day:D2}";
    }

    private static string MakeDateTime(double jd)
    {
        var dt  = SEWrapper.DateFromJulianDay(jd, true);
        var sec = Math.Min(dt.Time.Second, 59);
        return $"{dt.Date.Year:D4}/{dt.Date.Month:D2}/{dt.Date.Day:D2} " +
               $"{dt.Time.Hour:D2}:{dt.Time.Minute:D2}:{sec:D2}";
    }
}
