// DataImporter.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.Research.Pipeline;

// ── Errors ────────────────────────────────────────────────────────────────────

public sealed class DataImportException : Exception
{
    public int     Line  { get; }
    public string? Field { get; }

    public DataImportException(string message) : base(message) { }

    public DataImportException(string kind, int line, string field, string value)
        : base($"{kind} at line {line}, field '{field}': '{value}'")
    {
        Line  = line;
        Field = field;
    }

    public static DataImportException ParseError(int line, string text) =>
        new($"Parse error at line {line}: '{text}'", line, "", text);

    public static DataImportException OutOfRange(int line, string field, string value) =>
        new("OutOfRange", line, field, value);

    public static DataImportException Unreadable(string source) =>
        new($"Cannot read source: '{source}'");

    public static DataImportException NoData() =>
        new("The source contains no data rows.");
}

// ── Interface ─────────────────────────────────────────────────────────────────

/// <summary>Converts an external data source into a list of ResearchInputRecord.
/// isData is set by the caller — the same interface is used for real data and control-group imports.</summary>
public interface IDataImporter
{
    /// <summary>Parses the source file at the given path and returns validated records.
    /// startId is assigned to the first record; subsequent records increment by 1.</summary>
    List<ResearchInputRecord> Parse(string sourcePath, bool isData, int startId);

    /// <summary>Parses an in-memory string — useful for tests.</summary>
    List<ResearchInputRecord> ParseString(string content, bool isData, int startId);
}
