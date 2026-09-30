// ExchangeMessage.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;

namespace EnigmaWin.Sources.Features.ImportExport.Shared;

/// <summary>
/// Severity of a single message produced while importing or exporting a record in an exchange format.
/// A warning never aborts the surrounding record or file; a fatal error aborts only the record it belongs to.
/// </summary>
public enum ExchangeSeverity
{
    Warning,
    Fatal
}

/// <summary>
/// A single diagnostic produced while parsing, mapping, importing or exporting one record of an exchange
/// format (QCK, AAF'97, Enigma JSON). The text is not stored: <see cref="Key"/> is a Rosetta key in
/// ImportExport.strings and <see cref="Args"/> fill its {0}, {1}... placeholders.
/// <see cref="RecordNumber"/> is 1-based and null for messages that apply to the file as a whole.
/// </summary>
public sealed record ExchangeMessage(
    ExchangeSeverity Severity,
    string Key,
    IReadOnlyList<object> Args,
    int? RecordNumber = null,
    string? Field = null)
{
    public static ExchangeMessage Warning(string key, int? recordNumber, string? field, params object[] args) =>
        new(ExchangeSeverity.Warning, key, args, recordNumber, field);

    public static ExchangeMessage Fatal(string key, int? recordNumber, string? field, params object[] args) =>
        new(ExchangeSeverity.Fatal, key, args, recordNumber, field);
}

/// <summary>
/// Outcome of parsing a whole exchange-format file into format-specific intermediate records, before mapping
/// to the Enigma domain model. A record with a fatal error is left out of <see cref="Records"/>; parsing
/// continues with the remaining records rather than aborting the file.
/// </summary>
public sealed class FormatParseResult<TRecord>
{
    public List<TRecord> Records { get; } = [];
    public List<ExchangeMessage> Messages { get; } = [];

    public IEnumerable<ExchangeMessage> FatalMessages   => Messages.Where(m => m.Severity == ExchangeSeverity.Fatal);
    public IEnumerable<ExchangeMessage> WarningMessages => Messages.Where(m => m.Severity == ExchangeSeverity.Warning);
}

/// <summary>
/// Outcome of importing records into the Enigma database, with per-record warnings and fatal errors so a
/// problem in one record does not have to abort the whole import.
/// </summary>
public sealed class FormatImportResult
{
    public int ChartsImported { get; set; }
    public int ChartsSkipped  { get; set; }
    public int EventsImported { get; set; }
    public int EventsSkipped  { get; set; }
    public List<ExchangeMessage> Messages { get; } = [];

    public IEnumerable<ExchangeMessage> FatalMessages => Messages.Where(m => m.Severity == ExchangeSeverity.Fatal);
}

/// <summary>Outcome of an export: the file content and the number of charts and events written.</summary>
public sealed class FormatExportResult
{
    public byte[] Data { get; init; } = [];
    public int ChartsExported { get; init; }
    public int EventsExported { get; init; }
    public List<ExchangeMessage> Messages { get; init; } = [];

    public IEnumerable<ExchangeMessage> FatalMessages => Messages.Where(m => m.Severity == ExchangeSeverity.Fatal);
}
