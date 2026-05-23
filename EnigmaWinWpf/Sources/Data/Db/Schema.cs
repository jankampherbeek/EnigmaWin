// Schema.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Data.Db;

internal static class Schema
{
    internal const string V2 = """
        CREATE TABLE IF NOT EXISTS UserConfiguration (
            Id                    TEXT PRIMARY KEY,
            Name                  TEXT NOT NULL,
            IsActive              INTEGER NOT NULL DEFAULT 0,
            Language              TEXT NOT NULL DEFAULT '',
            CalculationConfigJson TEXT NOT NULL,
            DisplayConfigJson     TEXT NOT NULL,
            GlyphsConfigJson      TEXT NOT NULL,
            FactorConfigJson      TEXT NOT NULL,
            AspectConfigJson      TEXT NOT NULL,
            OrbConfigJson         TEXT NOT NULL,
            ProgressionsConfigJson TEXT NOT NULL
        );
        """;

    internal const string V3 = """
        CREATE TABLE IF NOT EXISTS ResearchProject (
            Id               INTEGER PRIMARY KEY AUTOINCREMENT,
            Name             TEXT    NOT NULL,
            Description      TEXT    NOT NULL DEFAULT '',
            Inquiry          INTEGER NOT NULL DEFAULT 0,
            Config           TEXT    NOT NULL DEFAULT '',
            CgMultiplication INTEGER NOT NULL DEFAULT 1,
            Path             TEXT    NOT NULL DEFAULT '',
            CreationDate     TEXT    NOT NULL
        );
        """;

    internal const string V4 = """
        CREATE UNIQUE INDEX IF NOT EXISTS UX_ResearchProject_Name ON ResearchProject(Name);
        """;

    internal const string V1 = """
        CREATE TABLE IF NOT EXISTS Horoscope (
            Id           TEXT PRIMARY KEY,
            Name         TEXT NOT NULL,
            Category     TEXT NOT NULL,
            Notes        TEXT,
            Source       TEXT,
            RoddenRating TEXT NOT NULL,
            PlaceName    TEXT,
            Latitude     REAL,
            Longitude    REAL
        );

        CREATE TABLE IF NOT EXISTS HoroscopeDateTime (
            Id                 TEXT PRIMARY KEY,
            HoroscopeId        TEXT NOT NULL REFERENCES Horoscope(Id) ON DELETE CASCADE,
            JulianDate         REAL NOT NULL,
            TimeZoneIdentifier TEXT NOT NULL,
            TimeIsUnknown      INTEGER NOT NULL DEFAULT 0,
            IsPreferred        INTEGER NOT NULL DEFAULT 0,
            Label              TEXT,
            OriginalInput      TEXT
        );

        CREATE TABLE IF NOT EXISTS ChartEvent (
            Id                 TEXT PRIMARY KEY,
            Title              TEXT NOT NULL,
            EventDescription   TEXT,
            JulianDate         REAL NOT NULL,
            TimeZoneIdentifier TEXT NOT NULL DEFAULT 'UTC',
            OriginalInput      TEXT,
            PlaceName          TEXT,
            Latitude           REAL,
            Longitude          REAL
        );

        CREATE TABLE IF NOT EXISTS HoroscopeEvent (
            HoroscopeId TEXT NOT NULL REFERENCES Horoscope(Id) ON DELETE CASCADE,
            EventId     TEXT NOT NULL REFERENCES ChartEvent(Id) ON DELETE CASCADE,
            PRIMARY KEY (HoroscopeId, EventId)
        );
        """;
}
