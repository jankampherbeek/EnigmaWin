// PrimDirOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Progressive.PrimDir;

using CA = PrimDirCalcAssist;

public sealed record PrimDirHit(
    double  JulianDay,
    string  DateText,
    Factors Significator,
    Factors Promissor,
    Aspects Aspect,
    double? OrbDays);

public static class PrimDirOrchestrator
{
    private const double TropicalYear = 365.242199074;
    private const double Naibod       = 0.985647358006;

    // SE flags
    private const int SeFlagsSwieph     = 2;
    private const int SeFlagsSpeed      = 256;
    private const int SeFlagsEquatorial = 2048;
    private const int SeSun             = 0;

    public static List<PrimDirHit> Calculate(
        FullChart chart, double geoLat, double natalJD,
        double startJD, double endJD,
        PrimaryDirectionsConfig config)
    {
        return BuildHits(chart, geoLat, natalJD, startJD, endJD, null, config);
    }

    public static List<PrimDirHit> CalculateForEvent(
        FullChart chart, double geoLat, double natalJD,
        double eventJD,
        PrimaryDirectionsConfig config)
    {
        var orbWindow = config.Orb * TropicalYear;
        return BuildHits(chart, geoLat, natalJD,
            eventJD - orbWindow, eventJD + orbWindow, eventJD, config);
    }

    // ---- internal helpers ----

    private static List<PrimDirHit> BuildHits(
        FullChart chart, double geoLat, double natalJD,
        double startJD, double endJD, double? eventJD,
        PrimaryDirectionsConfig config)
    {
        var specBase = BuildSpecBase(chart, geoLat);
        var hits     = new List<PrimDirHit>();

        var promissors    = config.Promissors.Where(f => chart.Coordinates.ContainsKey(f)).ToList();
        var significators = config.Significators.Where(f => chart.Coordinates.ContainsKey(f)).ToList();

        foreach (var promissor in promissors)
        {
            foreach (var significator in significators)
            {
                if (promissor == significator) continue;

                foreach (var aspect in new[] { Aspects.Conjunction, Aspects.Opposition })
                {
                    var arc = CalcArc(chart, specBase, config, promissor, significator, aspect);
                    if (double.IsNaN(arc)) continue;

                    var jd = JdForEvent(natalJD, arc, config.TimeKey);
                    if (jd > startJD && jd <= endJD)
                    {
                        var dt       = SEWrapper.DateFromJulianDay(jd, true);
                        var dateTxt  = $"{dt.Date.Year:D4}/{dt.Date.Month:D2}/{dt.Date.Day:D2}";
                        var orbDays  = eventJD.HasValue ? (double?)(jd - eventJD.Value) : null;
                        hits.Add(new PrimDirHit(jd, dateTxt, significator, promissor, aspect, orbDays));
                    }
                }
            }
        }

        hits.Sort((a, b) => a.JulianDay.CompareTo(b.JulianDay));
        return hits;
    }

    // ---- arc dispatch ----

    private static double CalcArc(
        FullChart chart, SpecBase specBase, PrimaryDirectionsConfig config,
        Factors promissor, Factors significator, Aspects aspect)
    {
        if (!chart.Coordinates.TryGetValue(promissor, out var promPos)) return double.NaN;
        if (!chart.Coordinates.TryGetValue(significator, out var signPos)) return double.NaN;

        var isOpp = aspect == Aspects.Opposition;

        return config.Method switch
        {
            PrimaryMethods.Placidus =>
                PlacidusArc(
                    BuildPlacSpec(promPos, specBase, config, isOpp),
                    BuildPlacSpec(signPos, specBase, config, false),
                    specBase),
            PrimaryMethods.Regiomontanus =>
                RegiomontanusArc(
                    BuildRegSpec(promPos, specBase, config, isOpp),
                    BuildRegSpec(signPos, specBase, config, false)),
            _ => double.NaN
        };
    }

    // ---- SpecBase ----

    private static SpecBase BuildSpecBase(FullChart chart, double geoLat)
    {
        var raMc  = chart.HousePositions.Midheaven.RightAscension;
        var raIc  = raMc <= 180 ? raMc + 180 : raMc - 180;
        var oaAsc = raMc + 90;
        if (oaAsc >= 360) oaAsc -= 360;
        return new SpecBase(
            RaMc:   raMc,
            RaIc:   raIc,
            LonMc:  chart.HousePositions.Midheaven.Longitude,
            RaAsc:  chart.HousePositions.Ascendant.RightAscension,
            LonAsc: chart.HousePositions.Ascendant.Longitude,
            OaAsc:  oaAsc,
            OblEcl: chart.Obliquity,
            GeoLat: geoLat);
    }

    // ---- FactorPointBase ----

    private static FactorPointBase BuildPointBase(
        FullFactorPosition pos, SpecBase specBase, PrimaryDirectionsConfig config, bool isOpp)
    {
        var lon      = pos.Ecliptical.Length > 0 ? pos.Ecliptical[0].MainPos   : 0.0;
        var lat      = pos.Ecliptical.Length > 0 ? pos.Ecliptical[0].Deviation : 0.0;
        var ra       = pos.Equatorial.Length > 0 ? pos.Equatorial[0].MainPos   : 0.0;
        var decl     = pos.Equatorial.Length > 0 ? pos.Equatorial[0].Deviation : 0.0;
        var azimuth  = pos.Horizontal.Length > 0 ? pos.Horizontal[0].Azimuth   : 0.0;
        var altitude = pos.Horizontal.Length > 0 ? pos.Horizontal[0].Altitude  : 0.0;

        if (config.Approach == PrimaryApproaches.Ecliptical)
        {
            decl = CA.DeclFromLongNoLat(lon, specBase.OblEcl);
            ra   = CA.RightAscFromLongNoLat(lon, specBase.OblEcl);
        }

        bool chartLeft, chartTop;
        if (config.Approach == PrimaryApproaches.Mundane)
        {
            chartLeft = CA.IsChartLeft(ra,  specBase.RaMc);
            chartTop  = CA.IsChartTop(ra,  specBase.RaAsc);
        }
        else
        {
            chartLeft = CA.IsChartLeft(lon, specBase.LonMc);
            chartTop  = CA.IsChartTop(lon, specBase.LonAsc);
        }

        if (isOpp)
        {
            return new FactorPointBase(
                Lon:       CA.ValueToRange(lon + 180, 0, 360),
                Lat:       -lat,
                Ra:        CA.ValueToRange(ra  + 180, 0, 360),
                Decl:      -decl,
                Azimuth:   CA.ValueToRange(azimuth + 180, 0, 360),
                Altitude:  -altitude,
                ChartLeft: !chartLeft,
                ChartTop:  !chartTop);
        }
        return new FactorPointBase(lon, lat, ra, decl, azimuth, altitude, chartLeft, chartTop);
    }

    // ---- Placidus ----

    private static FactorSpecPlac BuildPlacSpec(
        FullFactorPosition pos, SpecBase specBase, PrimaryDirectionsConfig config, bool isOpp)
    {
        var b      = BuildPointBase(pos, specBase, config, isOpp);
        var north  = specBase.GeoLat >= 0;
        var ad     = CA.AscensionalDifference(b.Decl, specBase.GeoLat);
        var oad    = CA.ObliqueAscDesc(b.Ra, ad, b.ChartLeft, north);
        var horDist = CA.HorizontalDistance(oad, specBase.OaAsc, b.ChartLeft, north);
        var merDist = CA.MeridianDistance(b.Ra, specBase.RaMc, specBase.RaIc, b.ChartTop);
        var semiArc = b.ChartTop ? (90 + ad) : (90 - ad);
        return new FactorSpecPlac(b, ad, oad, horDist, merDist, semiArc);
    }

    private static double PlacidusArc(FactorSpecPlac prom, FactorSpecPlac sign, SpecBase specBase)
    {
        var isSignTop  = sign.Base.ChartTop;
        var isSignLeft = sign.Base.ChartLeft;
        var quadrCorr  = ((isSignLeft && isSignTop) || (!isSignLeft && !isSignTop)) ? -1.0 : 1.0;
        var r          = isSignTop ? specBase.RaMc : specBase.RaIc;
        var horCorr    = isSignTop ? 1.0 : -1.0;
        if (Math.Abs(sign.SemiArc) < 0.000001) return double.NaN;
        var arc = prom.Base.Ra - r + quadrCorr * (90 + horCorr * prom.Ad) * sign.MerDist / sign.SemiArc;
        return CA.ValueToRange(arc, 0, 360);
    }

    // ---- Regiomontanus ----

    private static FactorSpecReg BuildRegSpec(
        FullFactorPosition pos, SpecBase specBase, PrimaryDirectionsConfig config, bool isOpp)
    {
        var b      = BuildPointBase(pos, specBase, config, isOpp);
        var geoLat = specBase.GeoLat;
        var isTop  = config.Approach == PrimaryApproaches.Mundane
            ? CA.IsChartTop(b.Ra,  specBase.RaAsc)
            : CA.IsChartTop(b.Lon, specBase.LonAsc);
        var merDist = CA.MeridianDistance(b.Ra, specBase.RaMc, specBase.RaIc, isTop);
        var zd      = CA.ZenithDistReg(b.Decl, merDist, geoLat, isTop);
        var poleReg = CA.Rad2Deg(Math.Asin(Math.Sin(CA.Deg2Rad(geoLat)) * Math.Sin(CA.Deg2Rad(zd))));
        var tanProduct = Math.Tan(CA.Deg2Rad(b.Decl)) * Math.Tan(CA.Deg2Rad(poleReg));
        if (Math.Abs(tanProduct) > 1.0)
            return new FactorSpecReg(b, merDist, zd, poleReg, 0, 0, Invalid: true);
        var factorQ = CA.Rad2Deg(Math.Asin(tanProduct));
        var factorW = b.ChartLeft ? (b.Ra - factorQ) : (b.Ra + factorQ);
        return new FactorSpecReg(b, merDist, zd, poleReg, factorQ, factorW, Invalid: false);
    }

    private static double RegiomontanusArc(FactorSpecReg prom, FactorSpecReg sign)
    {
        if (sign.Invalid) return double.NaN;
        var x = Math.Tan(CA.Deg2Rad(prom.Base.Decl)) * Math.Tan(CA.Deg2Rad(sign.PoleReg));
        if (Math.Abs(x) > 1.0) return double.NaN;
        var qp = CA.Rad2Deg(Math.Asin(x));
        var wp = sign.Base.ChartLeft ? (prom.Base.Ra - qp) : (prom.Base.Ra + qp);
        return CA.ValueToRange(wp - sign.FactorW, 0, 360);
    }

    // ---- Time keys ----

    private static double JdForEvent(double natalJD, double arc, PrimaryTimeKeys key)
    {
        switch (key)
        {
            case PrimaryTimeKeys.Ptolemy:
                return natalJD + arc * TropicalYear;

            case PrimaryTimeKeys.Brahe:
            {
                var ra0    = CalcSunEquatorial(natalJD - 0.5);
                var ra1    = CalcSunEquatorial(natalJD + 0.5);
                var raDiff = CA.ValueToRange(ra1 - ra0, 0, 360);
                return raDiff > 0.001
                    ? natalJD + arc / raDiff * TropicalYear
                    : natalJD + arc / Naibod * TropicalYear;
            }

            case PrimaryTimeKeys.Placidus:
            {
                var sunRANatal  = CalcSunEquatorial(natalJD);
                var targetRA    = CA.ValueToRange(sunRANatal + arc, 0, 360);
                var estimatedJD = natalJD + arc * (360.0 / TropicalYear);
                var secJD       = FindJdForSunPos(targetRA, estimatedJD, equatorial: true);
                return (secJD - natalJD) * TropicalYear + natalJD;
            }

            case PrimaryTimeKeys.VanDam:
            {
                var sunLonNatal = CalcSunEcliptical(natalJD);
                var targetLon   = CA.ValueToRange(sunLonNatal + arc, 0, 360);
                var estimatedJD = natalJD + arc * (360.0 / TropicalYear);
                var secJD       = FindJdForSunPos(targetLon, estimatedJD, equatorial: false);
                return (secJD - natalJD) * TropicalYear + natalJD;
            }

            default: // Naibod
                return natalJD + arc / Naibod * TropicalYear;
        }
    }

    private static double CalcSunEquatorial(double jd)
    {
        var flags = SeFlagsSwieph + SeFlagsSpeed + SeFlagsEquatorial;
        return SEWrapper.CalculateFactorPosition(jd, SeSun, flags)?.MainPos ?? 0;
    }

    private static double CalcSunEcliptical(double jd)
    {
        var flags = SeFlagsSwieph + SeFlagsSpeed;
        return SEWrapper.CalculateFactorPosition(jd, SeSun, flags)?.MainPos ?? 0;
    }

    private static double FindJdForSunPos(double target, double estimated, bool equatorial)
    {
        var flags = equatorial
            ? SeFlagsSwieph + SeFlagsSpeed + SeFlagsEquatorial
            : SeFlagsSwieph + SeFlagsSpeed;
        var tempJD = estimated;
        for (var i = 0; i < 50; i++)
        {
            var result = SEWrapper.CalculateFactorPosition(tempJD, SeSun, flags);
            if (result is null) break;
            var delta = CA.ValueToRange(result.MainPos - target, -180, 180);
            if (Math.Abs(delta) < 0.0001) break;
            tempJD -= delta;
        }
        return tempJD;
    }

    private sealed record SpecBase(
        double RaMc, double RaIc, double LonMc,
        double RaAsc, double LonAsc, double OaAsc,
        double OblEcl, double GeoLat);

    private sealed record FactorPointBase(
        double Lon, double Lat, double Ra, double Decl,
        double Azimuth, double Altitude,
        bool ChartLeft, bool ChartTop);

    private sealed record FactorSpecPlac(
        FactorPointBase Base, double Ad, double Oad,
        double HorDist, double MerDist, double SemiArc);

    private sealed record FactorSpecReg(
        FactorPointBase Base, double MerDist, double ZenithDist,
        double PoleReg, double FactorQ, double FactorW, bool Invalid);
}
