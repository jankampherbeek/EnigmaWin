// ControlGroupGenerator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace EnigmaWin.Sources.Features.Research.Pipeline;

/// <summary>Generates control-group records from real-data records using independent column shuffling.
/// Each call to Generate produces count × multiplicity synthetic records where all time/location
/// fields are independently shuffled, using cryptographically strong randomness.</summary>
public sealed class ControlGroupGenerator
{
    /// <summary>Generates inputRecords.Count × multiplicity control-group records.</summary>
    /// <param name="inputRecords">The real-data records to derive the control group from.</param>
    /// <param name="multiplicity">How many independent shuffled sets to produce.</param>
    /// <param name="startId">Id assigned to the first generated record; increments by 1 for each subsequent.</param>
    public List<ResearchInputRecord> Generate(
        IReadOnlyList<ResearchInputRecord> inputRecords,
        int multiplicity,
        int startId)
    {
        if (inputRecords.Count == 0 || multiplicity <= 0) return [];

        var all    = new List<ResearchInputRecord>(inputRecords.Count * multiplicity);
        var nextId = startId;

        for (var set = 0; set < multiplicity; set++)
            all.AddRange(GenerateOneSet(inputRecords, ref nextId));

        return all;
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private static List<ResearchInputRecord> GenerateOneSet(
        IReadOnlyList<ResearchInputRecord> input,
        ref int nextId)
    {
        // Extract each column
        var years   = ExtractColumn(input, r => r.Year);
        var months  = ExtractColumn(input, r => r.Month);
        var days    = ExtractColumn(input, r => r.Day);
        var hours   = ExtractColumn(input, r => r.Hour);
        var minutes = ExtractColumn(input, r => r.Minute);
        var seconds = ExtractColumn(input, r => r.Second);
        var offsets = ExtractColumnDouble(input, r => r.Offset);
        var geoLats = ExtractColumnDouble(input, r => r.GeoLat);
        var geoLons = ExtractColumnDouble(input, r => r.GeoLon);

        // Shuffle all columns independently using cryptographically strong randomness
        SecureShuffle(years);
        SecureShuffle(months);
        SecureShuffle(days);
        SecureShuffle(hours);
        SecureShuffle(minutes);
        SecureShuffle(seconds);
        SecureShuffle(offsets);
        SecureShuffle(geoLats);
        SecureShuffle(geoLons);

        // Sort days descending to minimise invalid day/month combinations before the calendar fix-up
        days.Sort((a, b) => b.CompareTo(a));

        var result          = new List<ResearchInputRecord>(input.Count);
        var remainingMonths = new List<int>(months); // mutable copy for calendar fix-up

        for (var i = 0; i < input.Count; i++)
        {
            var year  = years[i];
            var day   = days[i];
            var month = FindCompatibleMonth(day, year, remainingMonths)
                     ?? remainingMonths[0]; // fallback: accept whatever is left
            remainingMonths.Remove(month);

            result.Add(new ResearchInputRecord(
                Id: nextId++,
                IsData: false,
                GeoLat: geoLats[i],
                GeoLon: geoLons[i],
                Year:   year,
                Month:  month,
                Day:    day,
                Hour:   hours[i],
                Minute: minutes[i],
                Second: seconds[i],
                Offset: offsets[i]));
        }
        return result;
    }

    /// <summary>Finds and removes the first month from remaining that is valid for the given day/year.</summary>
    private static int? FindCompatibleMonth(int day, int year, List<int> remaining)
    {
        for (var i = 0; i < remaining.Count; i++)
        {
            if (DayFitsInMonth(day, remaining[i], year))
            {
                var m = remaining[i];
                remaining.RemoveAt(i);
                return m;
            }
        }
        return null;
    }

    private static bool DayFitsInMonth(int day, int month, int year)
    {
        if (day < 1) return false;
        return month switch
        {
            2 => day <= (DateTime.IsLeapYear(year) ? 29 : 28),
            4 or 6 or 9 or 11 => day <= 30,
            _ => day <= 31
        };
    }

    // ── Cryptographically strong Fisher-Yates shuffle ─────────────────────────

    private static void SecureShuffle<T>(List<T> list)
    {
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = SecureRandomInt(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    /// <summary>Returns a cryptographically random int in [0, bound) using rejection sampling to eliminate modulo bias.</summary>
    private static int SecureRandomInt(int bound)
    {
        if (bound <= 1) return 0;
        var buf       = new byte[4];
        var maxUnbiased = uint.MaxValue - uint.MaxValue % (uint)bound;
        uint raw;
        do
        {
            RandomNumberGenerator.Fill(buf);
            raw = BitConverter.ToUInt32(buf, 0);
        } while (raw > maxUnbiased);
        return (int)(raw % (uint)bound);
    }

    private static List<int> ExtractColumn(IReadOnlyList<ResearchInputRecord> input, Func<ResearchInputRecord, int> selector)
    {
        var list = new List<int>(input.Count);
        foreach (var r in input) list.Add(selector(r));
        return list;
    }

    private static List<double> ExtractColumnDouble(IReadOnlyList<ResearchInputRecord> input, Func<ResearchInputRecord, double> selector)
    {
        var list = new List<double>(input.Count);
        foreach (var r in input) list.Add(selector(r));
        return list;
    }
}
