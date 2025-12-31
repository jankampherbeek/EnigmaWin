// SEWrapper.cs
// EnigmaWin
// Created by Jan Kampherbeek on 27-12-2025

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using EnigmaWin.Sources.Domain;
using Serilog;

namespace EnigmaWin.Sources.Features.AstronCalc;

public class SEWrapper
{
    private static bool _isInitialized = false;

    /// <summary>
    /// Initializer for the Swiss Ephemeris. Check the comments for the methods as they are partly required to run before using the CommonSE.
    /// </summary>
    [SuppressMessage("Interoperability",
        "SYSLIB1054:Use \'LibraryImportAttribute\' instead of \'DllImportAttribute\' to generate P/Invoke marshalling code at compile time")]
    [SuppressMessage("Globalization", "CA2101:Specify marshaling for P/Invoke string arguments")]
    public static void SeInitializer(string? path)
    {
        if (path == null) return;
        try
        {
            ext_swe_set_ephe_path(path);
            _isInitialized = true;
        }
        catch (DllNotFoundException)
        {
            // TODO create logging
            // TODO create specific exceptions
            // Log.Error("SeInitializer could not find swedll64.dll. Throwing SwissEphException which should terminate the program");
            // throw new SwissEphException("The swedll64.dll, which is an essential part of the Swiss Ephemeris, could not be found");
            throw new Exception(
                "The swedll64.dll, which is an essential part of the Swiss Ephemeris, could not be found");
        }
    }


    [DllImport("swedll64.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_set_ephe_path")]
    private static extern void ext_swe_set_ephe_path(string? path);

    public static void SetTopocentric(double geoLong, double geoLat, double altitudeMeters)
    {
        ext_swe_set_topo(geoLong, geoLat, altitudeMeters);
    }

    [DllImport("swedll64.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_set_topo")]
    private static extern void ext_swe_set_topo(double geoLong, double geoLatr, double altitudeMeters);

    /// <summary>Close Swiss Ephemeris and release all allocated memory and resources.</summary>
    /// <remarks>Use ony at end of application or test. To reuse, call SetEphePath().</remarks>
    public static void CloseEphemeris()
    {
        ext_swe_close();
        _isInitialized = false;
    }

    [DllImport("swedll64.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_close")]
    private static extern void ext_swe_close();

    /// <summary>Define Ayanamsha for calculation of sidereal positions.</summary>
    /// <param name="idAyanamsha">The id for the Ayanamsha as used by the CommonSE.</param>
    /// <remarks>Run this method if sidereal calculations will be used. If this method has not run during the current session, Fagan/Bradley is used as default ayanamsha.
    /// The method from the CommonSE dll is called using parameters t0 and t1 with the value 0, these will be ignored for all prdefined ayanamsha's.</remarks>
    public static void SetAyanamsha(int idAyanamsha)
    {
        if (idAyanamsha is >= -1 and <= 39)
        {
            ext_swe_set_sid_mode(idAyanamsha, 0, 0);
        }
    }

    [DllImport("swedll64.dll", CharSet = CharSet.Unicode, EntryPoint = "swe_set_sid_mode")]
    private static extern void ext_swe_set_sid_mode(int idAyanamsha, int t0, int t1);


    /// <summary>Retrieve Julian Day number from Swiss Ephemeris.</summary>
    public static double JdFromSe(AstronomicalDate date, AstronomicalTime time)
    {
        var gregFlag = (date.Gregorian) ? 1 : 0;
        return ext_swe_julday(date.Year, date.Month, date.Day, time.HourDecimal, gregFlag);
    }

    /// <summary>Retrieve date and time from Julian Day number.</summary>
    public static AstronomicalDateTime DateTimeFromJulianDay(double julianDay, bool gregorian)
    {
        var gregFlag = gregorian ? 1 : 0;
        var year = 0;
        var month = 0;
        var day = 0;
        var hour = 0.0;
        ext_swe_revjul(julianDay, gregFlag, ref year, ref month, ref day, ref hour);
        var date = new AstronomicalDate(year, month, day, gregorian);
        var time = new AstronomicalTime(hour);
        return new AstronomicalDateTime(date, time);
    }


    /// <summary>Access dll to retrieve Julian Day number.</summary>
    /// <param name="year">The astronomical year.</param>
    /// <param name="month">The month, counting from 1..12.</param>
    /// <param name="day">The number of the day.</param>
    /// <param name="hour">The hour: integer part and fraction.</param>
    /// <param name="gregflag">Type of calendar: Gregorian = 1, Julian = 0.</param>
    /// <returns>The calculated Julian Day Number.</returns>
    [DllImport("swedll64.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_julday")]
    private static extern double ext_swe_julday(int year, int month, int day, double hour, int gregflag);

    [DllImport("swedll64.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_revjul")]
    private static extern void ext_swe_revjul(double julianDayUt, int gregFlag, ref int year, ref int month,
        ref int day, ref double hour);


    /// <summary>Retrieve positions for a celestial point.</summary>
    public static MainAstronomicalPosition? CalculatePlanetPosition(double julianDay, int planet, int flags)
    {
        // TODO create correct exception
        if (!_isInitialized) throw new Exception("Swiss Ephemeris is not initialized. Call SeInitializer() first.");
        StringBuilder result = new(256);
        var positions = new double[6];
        var returnCode = ext_swe_calc_ut(julianDay, planet, flags, positions, result);
        // TODO create correct exception
        if (returnCode == 0)
            return new MainAstronomicalPosition(
                MainPos: result[0],
                Deviation: result[1],
                Distance: result[2],
                MainPosSpeed: result[3],
                DeviationSpeed: result[4],
                DistanceSpeed: result[5]
            );
        Serilog.Log.Error("Error calculating factor position: {Result}", result);
        throw new Exception($"Error calculating planet position: {result}");
    }

    /// <summary>Access dll to retrieve position for celestial point.</summary>
    /// <param name="tjd">Julian day for UT.</param>
    /// <param name="ipl">Identifier for the celestial point.</param>
    /// <param name="iflag">Combined values for flags.</param>
    /// <param name="xx">The resulting positions.</param>
    /// <param name="serr">Error text, if any.</param>
    /// <returns>An indication if the calculation was successful.</returns>
    [DllImport("swedll64.dll", CharSet = CharSet.Unicode, EntryPoint = "swe_calc_ut")]
    private static extern int ext_swe_calc_ut(double tjd, int ipl, long iflag, double[] xx, StringBuilder serr);
    
    
    
    /// <summary>
    /// Retrieve positions for house cusps and other mundane points.
    /// </summary>
    /// <param name="jdUt">Julian Day for UT.</param>
    /// <param name="flags">0 for tropical, 0 or SEFLG_SIDEREAL for sidereal (logical or).</param>
    /// <param name="geoLat">Geographic latitude.</param>
    /// <param name="geoLon">Geographic longitude.</param>
    /// <param name="houseSystem">Indication for the house system within the Swiss Ephemeris.</param>
    /// <returns>A two dimensional array. The first array contains the cusps, starting from position 1 (position 0 is empty) and ordered by number. 
    /// The length is 13 (for systems with 12 cusps) or 37 (for Gauquelin houses (houseSystem 'G'), which have 36 cusps).
    /// The second array contains 10 positions with the following content:
    /// 0: = Ascendant, 1: MC, 2: ARMC, 3: Vertex, 4: equatorial ascendant( East point), 5: co-ascendant (Koch), 6: co-ascendant (Munkasey), 7: polar ascendant (Munkasey). 
    /// Positions 8 and 9 are empty.
    /// </returns>
    ///  TODO make constant for SEFLG_SIDEREAL
    public double[][] CalculateHouses(double jdUt, int flags, double geoLat, double geoLon, char houseSystem)
    {
        var nrOfCusps = houseSystem == 'G' ? 37 : 13;
        var cusps = new double[nrOfCusps];
        var mundanePoints = new double[10];

        var result = ext_swe_houses_ex(jdUt, flags, geoLat, geoLon, houseSystem, cusps, mundanePoints);
        if (result < 0)
        {
            // TODO create correct exception
            var paramsSummary =
                $"jdUt: {jdUt}, flags: {flags}, geoLat: {geoLat}, geoLon: {geoLon}, houseSystem: {houseSystem}.";
            Serilog.Log.Error("CalculateHouses in SEWrapper returned a negative result {Result} with parameters {Params}", result, paramsSummary);
            throw new Exception($"{result}/SePosHousesFacade.PosHousesFromSe/{paramsSummary}");
        }
        double[][] positions = [cusps, mundanePoints];
        return positions;
    }
    
    [DllImport("swedll64.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_houses_ex")]
    private static extern int ext_swe_houses_ex(double tjdut, int flags, double geolat, double geolon, int hsys, double[] hcusp0, double[] ascmc0);


    
    
    /// <summary>
    /// Calculate azimuth and altitude.
    /// </summary>
    /// <remarks>
    /// Assumes zero for atmospheric pressure and temperature. 
    /// </remarks>
    /// <returns>Array with azimuth and altitude in that sequence.</returns>
    public double[] AzimuthAndAltitude(double julianDayUt, double rightAscension, double declination, double observerLatitude, double observerLongitude, double height )
    {
        const int flag = 1;   // Corresponds to SE_EQU2HOR in SE, handles equatorial coordinates
        double[] geoPos = [observerLongitude, observerLatitude, height];
        double[] equCoordinates = [rightAscension, declination, 0.0];
        var horizontalCoordinates = new double[3];  // at index 2 the apparent altitude is given, which is ignored.
        var result = ext_swe_azalt(julianDayUt, flag, geoPos, 0, 0, equCoordinates, horizontalCoordinates);
        if (result < 0)
        {
            Log.Error("AzimuthAndAltitude in SEWrappeer returns a negative result {Result}", result);
            throw new Exception($"{result}/SEWrapper.AzimuthAndAltitude");   // TODO use explicit exception
        } 
        var azimuth = horizontalCoordinates[0];
        var altitude = horizontalCoordinates[1];
        return [azimuth, altitude];
    }

    /// <summary>
    /// Access dll to retrieve horizontal positions.
    /// </summary>
    /// <param name="tjd">Julian day for UT.</param>
    /// <param name="iflag">Flag: always SE_EQU2HOR = 1.</param>
    /// <param name="geoCoordinates">Geographic longitude, altitude and height above sea (ignored for real altitude).</param>
    /// <param name="atPress">Atmospheric pressure in mbar, ignored for true altitude.</param>
    /// <param name="atTemp">Atmospheric temperature in degrees Celsius, ignored for true altitude.</param>
    /// <param name="equatorialCoordinates">Right ascension, declinationa and distance.</param>
    /// <param name="horizontalCoordinates">Resulting values for azimuth, true altitude and apparent altitude.</param>
    /// <returns>An indication if the calculation was successful. Negative values indicate an error.</returns>
    [DllImport("swedll64.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_azalt")]
    private static extern int ext_swe_azalt(double tjd, long iflag, double[] geoCoordinates, double atPress, double atTemp, double[] equatorialCoordinates, double[] horizontalCoordinates);
    
    
    
    /// <summary>Convert ecliptic to equatorial coordinates.</summary>
    /// <param name="eclipticCoordinates">Array with subsequently longitude and latitude.</param>
    /// <param name="obliquity"/>
    /// <returns>Array with subsequently right ascension and declination.</returns>
    public (double, double) EclipticToEquatorial(double[] eclipticCoordinates, double obliquity)
    {
        var negativeObliquity = -Math.Abs(obliquity);
        const double distance = 1.0;   // ignore distance as it won't change
        double[] allEclipticCoordinates = [eclipticCoordinates[0], eclipticCoordinates[1], distance];
        var equCoordinates = new double[3];
        ext_swe_cotrans(allEclipticCoordinates, equCoordinates, negativeObliquity);
        var ra = equCoordinates[0];
        var dec = equCoordinates[1];
        return (ra, dec);
    }

    [DllImport("swedll64.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_cotrans")]
    private static extern void ext_swe_cotrans(double[] allEclipticCoordinates, double[] equatorialResults, double negativeObliquity);
    
    /// <summary>
    /// Calculate sidereal time.
    /// </summary>
    /// <param name="julianDay">Julian day for the calculation.</param>
    /// <returns>Sidereal time in degrees.</returns>
    public static double SiderealTime(double julianDay)
    {
        return ext_swe_sidtime(julianDay);
    }
    
    [DllImport("swedll64.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_sidtime")]
    private static extern double ext_swe_sidtime(double julianDay);
    
}


