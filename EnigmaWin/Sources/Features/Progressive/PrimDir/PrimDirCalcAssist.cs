// PrimDirCalcAssist.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;

namespace EnigmaWin.Sources.Features.Progressive.PrimDir;

/// <summary>Pure mathematical helpers for primary-direction calculations.</summary>
public static class PrimDirCalcAssist
{
    public static double Deg2Rad(double deg) => deg * Math.PI / 180.0;
    public static double Rad2Deg(double rad) => rad * 180.0 / Math.PI;

    public static double ValueToRange(double value, double min, double max)
    {
        var range = max - min;
        var result = value;
        while (result < min)  result += range;
        while (result >= max) result -= range;
        return result;
    }

    /// <summary>Returns true when the point falls on the left (eastern, rising) side of the chart.</summary>
    public static bool IsChartLeft(double pos, double mc) =>
        ValueToRange(pos - mc, 0, 360) < 180;

    /// <summary>Returns true when the point falls above the horizon (diurnal hemisphere).</summary>
    public static bool IsChartTop(double pos, double asc) =>
        ValueToRange(pos - asc, 0, 360) > 180;

    /// <summary>Ascensional difference: asin(tan(decl) * tan(geoLat)).</summary>
    public static double AscensionalDifference(double decl, double geoLat)
    {
        var x = Math.Tan(Deg2Rad(decl)) * Math.Tan(Deg2Rad(geoLat));
        if (Math.Abs(x) > 1.0) return 0.0;
        return Rad2Deg(Math.Asin(x));
    }

    public static double ObliqueAscDesc(double ra, double ascDiff, bool chartLeft, bool north)
    {
        double oa;
        if (north)
            oa = chartLeft ? ra - ascDiff : ra + ascDiff;
        else
            oa = chartLeft ? ra + ascDiff : ra - ascDiff;
        return ValueToRange(oa, 0, 360);
    }

    public static double MeridianDistance(double ra, double raMc, double raIc, bool isTop)
    {
        var raMcIc = isTop ? raMc : raIc;
        var shortArc = ValueToRange(Math.Abs(ra - raMcIc), 0, 360);
        if (shortArc >= 180) shortArc = Math.Abs(360 - shortArc);
        return shortArc;
    }

    public static double HorizontalDistance(double oa, double oaAsc, bool chartLeft, bool north)
    {
        double hd;
        if (north)
        {
            if (chartLeft) hd = Math.Abs(oa - oaAsc);
            else           hd = Math.Abs(ValueToRange(oa + 180, 0, 180) - ValueToRange(oaAsc + 180, 0, 180));
        }
        else
        {
            if (!chartLeft) hd = Math.Abs(oa - oaAsc);
            else            hd = Math.Abs(ValueToRange(oa + 180, 0, 180) - ValueToRange(oaAsc + 180, 0, 180));
        }
        if (hd >= 180) hd = 360 - hd;
        if (hd >= 90)  hd = 180 - hd;
        return hd;
    }

    public static double DeclFromLongNoLat(double longitude, double obliquity) =>
        Rad2Deg(Math.Asin(Math.Sin(Deg2Rad(obliquity)) * Math.Sin(Deg2Rad(longitude))));

    public static double RightAscFromLongNoLat(double longitude, double obliquity)
    {
        var lon = Deg2Rad(longitude);
        var obl = Deg2Rad(obliquity);
        return ValueToRange(Rad2Deg(Math.Atan2(Math.Sin(lon) * Math.Cos(obl), Math.Cos(lon))), 0, 360);
    }

    /// <summary>Zenith distance of a point under its Regiomontanus house pole.</summary>
    public static double ZenithDistReg(double decl, double merDist, double geoLat, bool isTop)
    {
        var declRad = Deg2Rad(decl);
        var mdRad   = Deg2Rad(merDist);
        var glRad   = Deg2Rad(geoLat);

        if (Math.Abs(merDist - 90) < 0.000001)
            return 90 - Rad2Deg(Math.Atan(Math.Sin(Math.Abs(glRad)) * Math.Tan(declRad)));

        var a = Rad2Deg(Math.Atan(Math.Cos(glRad) * Math.Tan(mdRad)));
        var b = Rad2Deg(Math.Atan(Math.Tan(Math.Abs(glRad)) * Math.Cos(mdRad)));
        double c;
        if ((decl >= 0 && geoLat >= 0) || (decl < 0 && geoLat < 0))
            c = isTop ? b - Math.Abs(decl) : b + Math.Abs(decl);
        else
            c = isTop ? b + Math.Abs(decl) : b - Math.Abs(decl);

        var f = 0.0;
        if (Math.Abs(b - decl) >= 0.0000001)
            f = Rad2Deg(Math.Atan(Math.Sin(Math.Abs(glRad)) * Math.Sin(mdRad) * Math.Tan(Deg2Rad(c))));

        var zd = a + f;
        if (zd < 0) zd += 180;
        return zd;
    }
}
