using System;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Radix.RadixInput.UI;

public sealed class RadixInputModel
{
    public sealed record InputData(
        string ChartName,
        int Year,
        int Month,
        int Day,
        string Calendar,
        string YearCount,
        int Hour,
        int Minute,
        int Second,
        int OffsetHour,
        int OffsetMinute,
        int OffsetSecond,
        string OffsetDirection,
        string Dst,
        int LongitudeDegree,
        int LongitudeMinute,
        int LongitudeSecond,
        string LongitudeDirection,
        int LatitudeDegree,
        int LatitudeMinute,
        int LatitudeSecond,
        string LatitudeDirection);

    public CalcRequest CreateCalcRequest(InputData inputData)
    {
        var astronomicalYear = ToAstronomicalYear(inputData.Year, inputData.YearCount);
        var gregorian = inputData.Calendar == "G";

        var localDate = new AstronomicalDate(
            Year: astronomicalYear,
            Month: inputData.Month,
            Day: inputData.Day,
            Gregorian: gregorian
        );
        var localTime = new AstronomicalTime(
            hour: inputData.Hour,
            minute: inputData.Minute,
            second: inputData.Second
        );

        var localJulianDay = SEWrapper.JulianDay(localDate, localTime);
        var utJulianDay = ToUtJulianDay(localJulianDay, inputData);

        return new CalcRequest(
            julianDay: utJulianDay,
            factorsToUse:
            [
                Factors.Sun,
                Factors.Moon,
                Factors.Mercury,
                Factors.Venus,
                Factors.Mars,
                Factors.Jupiter,
                Factors.Saturn,
                Factors.Pluto
            ],
            houseSystem: (int)HouseSystems.Placidus,
            seFlags: 258,
            latitude: ToDecimalDegrees(
                inputData.LatitudeDegree,
                inputData.LatitudeMinute,
                inputData.LatitudeSecond,
                inputData.LatitudeDirection == "S"
            ),
            longitude: ToDecimalDegrees(
                inputData.LongitudeDegree,
                inputData.LongitudeMinute,
                inputData.LongitudeSecond,
                inputData.LongitudeDirection == "W"
            ),
            height: 0.0,
            configData: new ConfigData(
                HouseSystem: HouseSystems.Placidus,
                Ayanamsha: Ayanamshas.Tropical,
                ObserverPosition: ObserverPositions.Geocentric,
                ProjectionType: ProjectionTypes.TwoDimensional,
                BlackMoonCorrectionType: BlackMoonCorrectionTypes.Swisseph,
                LunarNodeType: LunarNodeTypes.MeanNode,
                LotsType: LotsTypes.Sect
            )
        );
    }

    public FullChart Calculate(InputData inputData)
    {
        var request = CreateCalcRequest(inputData);
        return AstronCalcOrchestrator.PerformCalculation(request);
    }

    private static int ToAstronomicalYear(int year, string yearCount)
    {
        return yearCount switch
        {
            "Astronomical" => year,
            "CE" => year > 0 ? year : throw new ArgumentOutOfRangeException(nameof(year), "CE requires year > 0."),
            "BCE" => year > 0 ? 1 - year : throw new ArgumentOutOfRangeException(nameof(year), "BCE requires year > 0."),
            _ => year
        };
    }

    private static double ToUtJulianDay(double localJulianDay, InputData inputData)
    {
        var offsetSeconds = (inputData.OffsetHour * 3600.0) + (inputData.OffsetMinute * 60.0) + inputData.OffsetSecond;
        var utOffsetSign = inputData.OffsetDirection == "Earlier" ? 1.0 : -1.0;
        var dstSeconds = inputData.Dst == "DST" ? -3600.0 : 0.0;
        return localJulianDay + ((utOffsetSign * offsetSeconds + dstSeconds) / 86400.0);
    }

    private static double ToDecimalDegrees(int degrees, int minutes, int seconds, bool negative)
    {
        var absolute = degrees + (minutes / 60.0) + (seconds / 3600.0);
        return negative ? -absolute : absolute;
    }
}
