// LocationDb.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;

namespace EnigmaWin.Sources.Features.Location;

/// <summary>Low-level read-only access to the bundled enigma.db.</summary>
internal sealed class LocationDb : IDisposable
{
    private readonly SqliteConnection _conn;

    public LocationDb()
    {
        var dbPath = ResolveDbPath();
        _conn = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly");
        _conn.Open();
    }

    public void Dispose() => _conn.Dispose();

    public IReadOnlyList<LocationCountry> AllCountries(string lang)
    {
        const string sql = """
            SELECT cn.country_code, cn.name, c.continent
            FROM CountryName cn
            JOIN Country c ON cn.country_code = c.country_code
            WHERE cn.lang = @lang
            ORDER BY cn.name COLLATE NOCASE
            """;
        var results = _conn.Query<(string Code, string Name, string Continent)>(sql, new { lang })
            .AsList();
        if (results.Count > 0 || lang == "en")
            return results.ConvertAll(r => new LocationCountry(r.Code, r.Name, r.Continent));
        return AllCountries("en");
    }

    public IReadOnlyList<LocationCountry> Countries(string lang, string pattern)
    {
        var likePattern = ToSqlLike(pattern.ToLowerInvariant());
        const string sql = """
            SELECT cn.country_code, cn.name, c.continent
            FROM CountryName cn
            JOIN Country c ON cn.country_code = c.country_code
            WHERE cn.lang = @lang
              AND lower(cn.name) LIKE @likePattern ESCAPE '\'
            ORDER BY cn.name COLLATE NOCASE
            LIMIT 50
            """;
        return _conn.Query<(string Code, string Name, string Continent)>(sql, new { lang, likePattern })
            .AsList()
            .ConvertAll(r => new LocationCountry(r.Code, r.Name, r.Continent));
    }

    public IReadOnlyList<LocationCity> Cities(string countryCode, string pattern = "*")
    {
        var likePattern = ToSqlLike(pattern.ToLowerInvariant());
        var isMatchAll  = likePattern == "%";

        var sql = isMatchAll
            ? """
              SELECT c.id, c.country_code, c.name, c.latitude, c.longitude,
                     c.region_code, c.elevation, c.timezone_id, t.name AS timezone_name
              FROM City c
              JOIN Timezone t ON c.timezone_id = t.id
              WHERE c.country_code = @countryCode
              ORDER BY c.name COLLATE NOCASE
              """
            : """
              SELECT c.id, c.country_code, c.name, c.latitude, c.longitude,
                     c.region_code, c.elevation, c.timezone_id, t.name AS timezone_name
              FROM City c
              JOIN Timezone t ON c.timezone_id = t.id
              WHERE c.country_code = @countryCode
                AND lower(c.name) LIKE @likePattern ESCAPE '\'
              ORDER BY c.name COLLATE NOCASE
              LIMIT 50
              """;

        return _conn.Query(sql, new { countryCode, likePattern })
            .AsList()
            .ConvertAll(row => new LocationCity(
                Id:           (int)row.id,
                CountryCode:  (string)row.country_code,
                Name:         (string)row.name,
                Latitude:     (double)row.latitude,
                Longitude:    (double)row.longitude,
                RegionCode:   (string?)row.region_code,
                Elevation:    row.elevation is DBNull or null ? (int?)null : (int)row.elevation,
                TimezoneId:   (int)row.timezone_id,
                TimezoneName: (string)row.timezone_name));
    }

    public IReadOnlyList<TzDataRow> TzDataRows(string zoneName)
    {
        const string sql = """
            SELECT t.name, td.offset_h, td.offset_m, td.offset_s,
                   td.rule, td.format, td.until_year, td.until_month, td.until_day,
                   td.at_h, td.at_m, td.at_s
            FROM TzData td
            JOIN Timezone t ON td.id_zone = t.id
            WHERE t.name = @zoneName
            ORDER BY td.id
            """;
        return _conn.Query(sql, new { zoneName })
            .AsList()
            .ConvertAll(row => new TzDataRow(
                ZoneName:   (string)row.name,
                OffsetH:    (int)row.offset_h,
                OffsetM:    (int)row.offset_m,
                OffsetS:    (int)row.offset_s,
                Rule:       (string)row.rule,
                Format:     (string)row.format,
                UntilYear:  (int)row.until_year,
                UntilMonth: (int)row.until_month,
                UntilDay:   (string)row.until_day,
                AtH:        (int)row.at_h,
                AtM:        (int)row.at_m,
                AtS:        (int)row.at_s));
    }

    public IReadOnlyList<DstDataRow> DstDataRows(string ruleName)
    {
        const string sql = """
            SELECT rule_name, from_year, to_year, month, day,
                   at_h, at_m, at_s, use_ut, save_h, save_m, save_s, letter
            FROM DstData
            WHERE rule_name = @ruleName
            ORDER BY id
            """;
        return _conn.Query(sql, new { ruleName })
            .AsList()
            .ConvertAll(row => new DstDataRow(
                RuleName: (string)row.rule_name,
                FromYear: (int)row.from_year,
                ToYear:   (string)row.to_year,
                Month:    (int)row.month,
                Day:      (string)row.day,
                AtH:      (int)row.at_h,
                AtM:      (int)row.at_m,
                AtS:      (int)row.at_s,
                UseUt:    (string)row.use_ut,
                SaveH:    (int)row.save_h,
                SaveM:    (int)row.save_m,
                SaveS:    (int)row.save_s,
                Letter:   (string)row.letter));
    }

    private static string ToSqlLike(string pattern)
    {
        var result = pattern
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%",  "\\%",  StringComparison.Ordinal)
            .Replace("_",  "\\_",  StringComparison.Ordinal)
            .Replace("*",  "%",    StringComparison.Ordinal);
        if (string.IsNullOrEmpty(result) || result == "%") return "%";
        if (!result.StartsWith('%')) result += "%";
        return result;
    }

    private static string ResolveDbPath()
    {
        var nextToExe = Path.Combine(AppContext.BaseDirectory, "enigma.db");
        if (File.Exists(nextToExe)) return nextToExe;

        var inResources = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Resources", "enigma.db"));
        if (File.Exists(inResources)) return inResources;

        throw new FileNotFoundException(
            $"enigma.db not found. Tried: '{nextToExe}', '{inResources}'", "enigma.db");
    }
}
