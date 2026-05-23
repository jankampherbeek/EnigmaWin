// FactorsInSignsWorker.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Research.Pipeline;

namespace EnigmaWin.Sources.Features.Research.Analysis;

// ── Result types ──────────────────────────────────────────────────────────────

/// <summary>Count of occurrences for one sign, split by data group.</summary>
public sealed record SignCount(Signs Sign, int DataCount, int ControlCount);

/// <summary>Analysis result for one factor: how many times it falls in each sign,
/// separately for real data and control group.</summary>
public sealed class FactorSignDistribution
{
    public Factors Factor { get; init; }
    /// <summary>One entry per sign (Aries … Pisces), always 12 elements.</summary>
    public IReadOnlyList<SignCount> SignCounts { get; init; } = [];
    public int TotalData    => Sum(c => c.DataCount);
    public int TotalControl => Sum(c => c.ControlCount);
    private int Sum(Func<SignCount, int> sel) { var t = 0; foreach (var c in SignCounts) t += sel(c); return t; }
}

/// <summary>Full result of a FactorsInSignsWorker run.</summary>
public sealed record FactorsInSignsResult(
    IReadOnlyList<FactorSignDistribution> Distributions,
    int SkippedRecords);

// ── Errors ────────────────────────────────────────────────────────────────────

public enum FactorsInSignsError { EclipticalNotEnabled, FileNotMapped }

public sealed class FactorsInSignsException(FactorsInSignsError error)
    : Exception($"FactorsInSigns error: {error}") { public FactorsInSignsError Error => error; }

// ── Worker ────────────────────────────────────────────────────────────────────

/// <summary>Reads results.bin (memory-mapped) and counts how many times each enabled factor
/// falls in each zodiac sign, separately for real-data records and control-group records.
/// Requires ecliptical coordinates to be present in the binary file.</summary>
public sealed class FactorsInSignsWorker
{
    private readonly ResultsBinaryFile _binaryFile;
    private readonly ResearchConfig    _config;

    public FactorsInSignsWorker(ResultsBinaryFile binaryFile, ResearchConfig config)
    {
        _binaryFile = binaryFile;
        _config     = config;
    }

    public FactorsInSignsResult Run()
    {
        if (!_config.UseEcliptical) throw new FactorsInSignsException(FactorsInSignsError.EclipticalNotEnabled);

        var layouts     = _config.FactorLayouts;
        var recordCount = (int)_binaryFile.RecordCount;
        var factorCount = layouts.Count;

        var dataCounts    = new int[factorCount][];
        var controlCounts = new int[factorCount][];
        for (var f = 0; f < factorCount; f++)
        {
            dataCounts[f]    = new int[12];
            controlCounts[f] = new int[12];
        }
        var skipped = 0;

        for (var position = 0; position < recordCount; position++)
        {
            bool isData; byte[] regionData;
            try { var rec = _binaryFile.ReadRecord(position); isData = rec.IsData; regionData = rec.Data; }
            catch { skipped++; continue; }
            for (var fi = 0; fi < factorCount; fi++)
            {
                var layout = layouts[fi];
                if (!layout.HasEcliptical) continue;

                double lon;
                try { lon = ResultsBinaryFile.ReadDouble(regionData, layout.ByteOffset); }
                catch { skipped++; continue; }

                var normalised = ((lon % 360) + 360) % 360;
                var signIndex  = Math.Min((int)(normalised / 30.0), 11);

                if (isData) dataCounts[fi][signIndex]++;
                else        controlCounts[fi][signIndex]++;
            }
        }

        var distributions = new List<FactorSignDistribution>(factorCount);
        for (var fi = 0; fi < factorCount; fi++)
        {
            var signCounts = new List<SignCount>(12);
            for (var si = 0; si < 12; si++)
                signCounts.Add(new SignCount((Signs)(si + 1), dataCounts[fi][si], controlCounts[fi][si]));
            distributions.Add(new FactorSignDistribution { Factor = layouts[fi].Factor, SignCounts = signCounts });
        }
        return new FactorsInSignsResult(distributions, skipped);
    }
}
