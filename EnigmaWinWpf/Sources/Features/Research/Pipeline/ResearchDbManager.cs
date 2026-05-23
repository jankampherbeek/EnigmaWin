// ResearchDbManager.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;
using Microsoft.Data.Sqlite;

namespace EnigmaWin.Sources.Features.Research.Pipeline;

/// <summary>Opens and manages horoscope_data.db in the project folder.
/// Provides CRUD for the horoscope_data table. All methods are synchronous; call from a background context.</summary>
public sealed class ResearchDbManager : IDisposable
{
    private readonly SqliteConnection _conn;

    /// <summary>Opens (or creates) horoscope_data.db at the given folder path.</summary>
    public ResearchDbManager(string folderPath)
    {
        var dbPath = Path.Combine(folderPath, "horoscope_data.db");
        _conn = new SqliteConnection($"Data Source={dbPath}");
        _conn.Open();
        _conn.Execute("PRAGMA foreign_keys = ON; PRAGMA journal_mode = WAL;");
        CreateTableIfNeeded();
    }

    public void Dispose() => _conn.Dispose();

    // ── Schema ───────────────────────────────────────────────────────────────

    private void CreateTableIfNeeded() => _conn.Execute("""
        CREATE TABLE IF NOT EXISTS horoscope_data (
            id      INTEGER PRIMARY KEY,
            data    INTEGER NOT NULL,
            geoLat  REAL    NOT NULL,
            geoLon  REAL    NOT NULL,
            year    INTEGER NOT NULL,
            month   INTEGER NOT NULL,
            day     INTEGER NOT NULL,
            hour    INTEGER NOT NULL,
            minute  INTEGER NOT NULL,
            second  INTEGER NOT NULL,
            offset  REAL    NOT NULL
        )
        """);

    // ── Read ─────────────────────────────────────────────────────────────────

    /// <summary>Returns all rows ordered by id.</summary>
    public IEnumerable<ResearchInputRecord> FetchAll() =>
        _conn.Query<Row>("SELECT * FROM horoscope_data ORDER BY id")
             .Select(ToRecord);

    /// <summary>Returns only real-data rows (data = 1).</summary>
    public IEnumerable<ResearchInputRecord> FetchRealData() => FetchWhere(isData: true);

    /// <summary>Returns only control-group rows (data = 0).</summary>
    public IEnumerable<ResearchInputRecord> FetchControlGroup() => FetchWhere(isData: false);

    /// <summary>Total number of rows in the table.</summary>
    public int Count() => _conn.ExecuteScalar<int>("SELECT COUNT(*) FROM horoscope_data");

    // ── Write ────────────────────────────────────────────────────────────────

    /// <summary>Inserts a single record. Replaces on id conflict.</summary>
    public void Insert(ResearchInputRecord record) =>
        _conn.Execute("""
            INSERT OR REPLACE INTO horoscope_data
                (id, data, geoLat, geoLon, year, month, day, hour, minute, second, offset)
            VALUES
                (@Id, @Data, @GeoLat, @GeoLon, @Year, @Month, @Day, @Hour, @Minute, @Second, @Offset)
            """,
            new
            {
                record.Id,
                Data = record.IsData ? 1 : 0,
                record.GeoLat,
                record.GeoLon,
                record.Year,
                record.Month,
                record.Day,
                record.Hour,
                record.Minute,
                record.Second,
                record.Offset
            });

    /// <summary>Bulk-inserts records inside a single transaction.</summary>
    public void InsertAll(IEnumerable<ResearchInputRecord> records)
    {
        using var tx = _conn.BeginTransaction();
        foreach (var r in records) Insert(r);
        tx.Commit();
    }

    /// <summary>Deletes the record with the given id.</summary>
    public void Delete(int id) =>
        _conn.Execute("DELETE FROM horoscope_data WHERE id = @Id", new { Id = id });

    /// <summary>Deletes all rows from the table.</summary>
    public void DeleteAll() => _conn.Execute("DELETE FROM horoscope_data");

    /// <summary>Drops and recreates the table — more robust than DeleteAll when the previous connection may not have closed cleanly.</summary>
    public void ResetTable()
    {
        _conn.Execute("DROP TABLE IF EXISTS horoscope_data");
        CreateTableIfNeeded();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private IEnumerable<ResearchInputRecord> FetchWhere(bool isData) =>
        _conn.Query<Row>("SELECT * FROM horoscope_data WHERE data = @Data ORDER BY id",
                         new { Data = isData ? 1 : 0 })
             .Select(ToRecord);

    private static ResearchInputRecord ToRecord(Row r) =>
        new(r.Id, r.Data != 0, r.GeoLat, r.GeoLon, r.Year, r.Month, r.Day, r.Hour, r.Minute, r.Second, r.Offset);

    // Dapper maps column names (case-insensitive) to this flat DTO.
    private sealed class Row
    {
        public int    Id     { get; init; }
        public int    Data   { get; init; }
        public double GeoLat { get; init; }
        public double GeoLon { get; init; }
        public int    Year   { get; init; }
        public int    Month  { get; init; }
        public int    Day    { get; init; }
        public int    Hour   { get; init; }
        public int    Minute { get; init; }
        public int    Second { get; init; }
        public double Offset { get; init; }
    }
}
