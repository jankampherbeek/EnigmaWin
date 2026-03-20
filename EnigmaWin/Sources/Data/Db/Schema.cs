namespace EnigmaWin.Sources.Data.Db;

internal static class Schema
{
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
