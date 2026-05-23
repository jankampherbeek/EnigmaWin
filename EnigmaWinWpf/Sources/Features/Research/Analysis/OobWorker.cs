// OobWorker.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Research.Pipeline;

namespace EnigmaWin.Sources.Features.Research.Analysis;

// ── Result types ──────────────────────────────────────────────────────────────

/// <summary>Count of OOB occurrences for one factor.</summary>
public sealed record OobCount(Factors Factor, int DataCount, int ControlCount);

/// <summary>Full result of an OobWorker run.</summary>
public sealed record OobResult(IReadOnlyList<OobCount> Counts, double Obliquity, int SkippedRecords);

// ── Errors ────────────────────────────────────────────────────────────────────

public enum OobWorkerError { EquatorialNotEnabled, FileNotMapped }

public sealed class OobWorkerException(OobWorkerError error)
    : Exception($"OobWorker error: {error}") { public OobWorkerError Error => error; }

// ── Worker ────────────────────────────────────────────────────────────────────

/// <summary>Reads results.bin and counts how many times each factor's declination exceeds
/// the obliquity of the ecliptic (i.e. is Out Of Bounds).
/// The obliquity is supplied per record (computed from Swiss Ephemeris factor id -1).
/// A factor is OOB when |declination| > obliquity + roundingMargin.
/// The rounding margin prevents the Sun from being incorrectly flagged at the solstices.</summary>
public sealed class OobWorker
{
    /// <summary>1 arc-second in degrees — prevents false OOB due to floating-point rounding.</summary>
    public const double RoundingMargin = 1.0 / 3600.0;

    private readonly ResultsBinaryFile    _binaryFile;
    private readonly ResearchConfig       _config;
    private readonly IReadOnlyList<double> _obliquityPerRecord;

    public OobWorker(ResultsBinaryFile binaryFile, ResearchConfig config,
                     IReadOnlyList<double> obliquityPerRecord)
    {
        _binaryFile         = binaryFile;
        _config             = config;
        _obliquityPerRecord = obliquityPerRecord;
    }

    public OobResult Run()
    {
        if (!_config.UseEquatorial) throw new OobWorkerException(OobWorkerError.EquatorialNotEnabled);

        var layouts     = _config.FactorLayouts;
        var recordCount = (int)_binaryFile.RecordCount;
        var n = layouts.Count;

        var dataCounts    = new int[n];
        var controlCounts = new int[n];
        var skipped = 0;
        var obliquitySum   = 0.0;
        var obliquityCount = 0;

        for (var position = 0; position < recordCount; position++)
        {
            bool isData; byte[] regionData;
            try { var rec = _binaryFile.ReadRecord(position); isData = rec.IsData; regionData = rec.Data; }
            catch { skipped++; continue; }
            var obliquity = position < _obliquityPerRecord.Count
                ? _obliquityPerRecord[position]
                : (_obliquityPerRecord.Count > 0 ? _obliquityPerRecord[_obliquityPerRecord.Count - 1] : 23.4393);
            var threshold = obliquity + RoundingMargin;

            if (isData)
            {
                obliquitySum += obliquity;
                obliquityCount++;
            }

            for (var fi = 0; fi < n; fi++)
            {
                var layout = layouts[fi];
                if (!layout.HasEquatorial) continue;
                double declination;
                try { declination = ResultsBinaryFile.ReadDouble(regionData, layout.ByteOffset + 8); }
                catch { continue; }
                if (Math.Abs(declination) > threshold)
                {
                    if (isData) dataCounts[fi]++;
                    else        controlCounts[fi]++;
                }
            }
        }

        var counts = new List<OobCount>(n);
        for (var fi = 0; fi < n; fi++)
            counts.Add(new OobCount(layouts[fi].Factor, dataCounts[fi], controlCounts[fi]));

        var meanObliquity = obliquityCount > 0
            ? obliquitySum / obliquityCount
            : (_obliquityPerRecord.Count > 0 ? _obliquityPerRecord[0] : 23.4393);

        return new OobResult(counts, meanObliquity, skipped);
    }
}
