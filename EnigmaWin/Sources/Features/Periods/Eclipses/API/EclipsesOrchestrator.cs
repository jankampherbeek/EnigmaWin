// EclipsesOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;

namespace EnigmaWin.Sources.Features.Periods.Eclipses.API;

/// <summary>Searches for solar and/or lunar eclipses within a given period, optionally enriched
/// with local visibility/type/Saros data for a specific geographic location.</summary>
public static class EclipsesOrchestrator
{
    private const double NoSarosValue = -99999999;

    /// <summary>Search for eclipses between startJD and endJD.
    /// When geoLon/geoLat are provided, each global eclipse is also queried locally
    /// to add type, visibility, and Saros data for that specific location.</summary>
    public static List<EclipseEvent> FindEclipses(double startJd, double endJd, EclipseSearchType type,
        double? geoLon, double? geoLat, double height = 0.0)
    {
        if (startJd >= endJd) return [];

        var useLocal = geoLon.HasValue && geoLat.HasValue;
        var lon = geoLon ?? 0.0;
        var lat = geoLat ?? 0.0;

        var events = new List<EclipseEvent>();

        if (type is EclipseSearchType.Solar or EclipseSearchType.All)
            events.AddRange(SearchSolar(startJd, endJd, useLocal, lon, lat, height));
        if (type is EclipseSearchType.Lunar or EclipseSearchType.All)
            events.AddRange(SearchLunar(startJd, endJd, useLocal, lon, lat, height));

        events.Sort((a, b) => a.DisplayJD.CompareTo(b.DisplayJD));
        return events;
    }

    private static List<EclipseEvent> SearchSolar(double startJd, double endJd,
        bool useLocal, double lon, double lat, double height)
    {
        var results = new List<EclipseEvent>();
        var searchFrom = startJd;

        for (var i = 0; i < 1000; i++)
        {
            if (searchFrom > endJd) break;
            var g = SEWrapper.SolEclipseGlobal(searchFrom);
            if (g is null) break;
            if (g.MaxJD > endJd) break;

            if (useLocal)
            {
                var local = SEWrapper.SolEclipseLocal(g.MaxJD - 1.0, lon, lat, height);
                var sameEclipse = local is not null && Math.Abs(local.MaxEclipseJD - g.MaxJD) < 1.0;

                double saros, sarosMember;
                if (sameEclipse)
                {
                    saros = local!.SarosNumber;
                    sarosMember = local.SarosMemberNumber;
                }
                else if (SEWrapper.SolEclipseSaros(g.MaxJD) is { } fallback)
                {
                    saros = fallback.SarosNumber;
                    sarosMember = fallback.SarosMemberNumber;
                }
                else
                {
                    saros = NoSarosValue;
                    sarosMember = NoSarosValue;
                }

                results.Add(new EclipseEvent(
                    Kind: EclipseKind.Solar,
                    DisplayJD: sameEclipse ? local!.MaxEclipseJD : g.MaxJD,
                    Longitude: SunLongitude(g.MaxJD),
                    IsTotal: sameEclipse ? local!.IsTotal : g.IsTotal,
                    IsAnnular: sameEclipse ? local!.IsAnnular : g.IsAnnular,
                    IsHybrid: sameEclipse ? local!.IsHybrid : g.IsHybrid,
                    IsPartial: sameEclipse ? local!.IsPartial : g.IsPartial,
                    IsPenumbral: false,
                    HasLocalData: sameEclipse,
                    IsVisible: sameEclipse && local!.IsVisible,
                    SarosNumber: saros, SarosMemberNumber: sarosMember
                ));
            }
            else
            {
                results.Add(new EclipseEvent(
                    Kind: EclipseKind.Solar,
                    DisplayJD: g.MaxJD,
                    Longitude: SunLongitude(g.MaxJD),
                    IsTotal: g.IsTotal, IsAnnular: g.IsAnnular, IsHybrid: g.IsHybrid,
                    IsPartial: g.IsPartial,
                    IsPenumbral: false,
                    HasLocalData: false, IsVisible: false,
                    SarosNumber: NoSarosValue, SarosMemberNumber: NoSarosValue
                ));
            }
            searchFrom = g.MaxJD + 1.0;
        }
        return results;
    }

    private static List<EclipseEvent> SearchLunar(double startJd, double endJd,
        bool useLocal, double lon, double lat, double height)
    {
        var results = new List<EclipseEvent>();
        var searchFrom = startJd;

        for (var i = 0; i < 1000; i++)
        {
            if (searchFrom > endJd) break;
            var g = SEWrapper.LunEclipseGlobal(searchFrom);
            if (g is null) break;
            if (g.MaxJD > endJd) break;

            if (useLocal)
            {
                var local = SEWrapper.LunEclipseLocal(g.MaxJD - 1.0, lon, lat, height);
                var sameEclipse = local is not null && Math.Abs(local.MaxEclipseJD - g.MaxJD) < 1.0;

                double saros, sarosMember;
                if (sameEclipse)
                {
                    saros = local!.SarosNumber;
                    sarosMember = local.SarosMemberNumber;
                }
                else if (SEWrapper.LunEclipseSaros(g.MaxJD) is { } fallback)
                {
                    saros = fallback.SarosNumber;
                    sarosMember = fallback.SarosMemberNumber;
                }
                else
                {
                    saros = NoSarosValue;
                    sarosMember = NoSarosValue;
                }

                results.Add(new EclipseEvent(
                    Kind: EclipseKind.Lunar,
                    DisplayJD: sameEclipse ? local!.MaxEclipseJD : g.MaxJD,
                    Longitude: MoonLongitude(g.MaxJD),
                    IsTotal: sameEclipse ? local!.IsTotal : g.IsTotal,
                    IsAnnular: false,
                    IsHybrid: false,
                    IsPartial: sameEclipse ? local!.IsPartial : g.IsPartial,
                    IsPenumbral: sameEclipse ? local!.IsPenumbral : g.IsPenumbral,
                    HasLocalData: sameEclipse,
                    IsVisible: sameEclipse && local!.TrueAltitude > 0,
                    SarosNumber: saros, SarosMemberNumber: sarosMember
                ));
            }
            else
            {
                results.Add(new EclipseEvent(
                    Kind: EclipseKind.Lunar,
                    DisplayJD: g.MaxJD,
                    Longitude: MoonLongitude(g.MaxJD),
                    IsTotal: g.IsTotal, IsAnnular: false, IsHybrid: false,
                    IsPartial: g.IsPartial,
                    IsPenumbral: g.IsPenumbral,
                    HasLocalData: false, IsVisible: false,
                    SarosNumber: NoSarosValue, SarosMemberNumber: NoSarosValue
                ));
            }
            searchFrom = g.MaxJD + 1.0;
        }
        return results;
    }

    private static double SunLongitude(double jd) =>
        SEWrapper.CalculateFactorPosition(jd, Factors.Sun.SeId(), 2)?.MainPos ?? 0.0;

    private static double MoonLongitude(double jd) =>
        SEWrapper.CalculateFactorPosition(jd, Factors.Moon.SeId(), 2)?.MainPos ?? 0.0;
}
