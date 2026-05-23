// FactorsInHousesWorker.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Research.Pipeline;

namespace EnigmaWin.Sources.Features.Research.Analysis;

// ── Result types ──────────────────────────────────────────────────────────────

/// <summary>Count of occurrences for one house, split by data group.</summary>
public sealed record HouseCount(int HouseNr, int DataCount, int ControlCount);

/// <summary>Analysis result for one factor: how many times it falls in each house.</summary>
public sealed class FactorHouseDistribution
{
    public Factors Factor { get; init; }
    public IReadOnlyList<HouseCount> HouseCounts { get; init; } = [];
    public int TotalData    => Sum(c => c.DataCount);
    public int TotalControl => Sum(c => c.ControlCount);
    private int Sum(Func<HouseCount, int> sel) { var t = 0; foreach (var c in HouseCounts) t += sel(c); return t; }
}

/// <summary>Full result of a FactorsInHousesWorker run.</summary>
public sealed record FactorsInHousesResult(
    IReadOnlyList<FactorHouseDistribution> Distributions,
    int NrOfHouses,
    int SkippedRecords);

// ── Errors ────────────────────────────────────────────────────────────────────

public enum FactorsInHousesError { EclipticalNotEnabled, FileNotMapped, NoCuspData }

public sealed class FactorsInHousesException(FactorsInHousesError error)
    : Exception($"FactorsInHouses error: {error}") { public FactorsInHousesError Error => error; }

// ── Worker ────────────────────────────────────────────────────────────────────

/// <summary>Reads results.bin and counts how many times each enabled factor falls in each house.
/// House cusp longitudes must be supplied by the caller (one array per record, indexed by
/// cusp position: 0 = cusp 1, 11 = cusp 12 for a 12-house system).</summary>
public sealed class FactorsInHousesWorker
{
    private readonly ResultsBinaryFile    _binaryFile;
    private readonly ResearchConfig       _config;
    private readonly IReadOnlyList<IReadOnlyList<double>> _cuspLongitudesByRecord;

    public FactorsInHousesWorker(ResultsBinaryFile binaryFile, ResearchConfig config,
                                 IReadOnlyList<IReadOnlyList<double>> cuspLongitudesByRecord)
    {
        _binaryFile             = binaryFile;
        _config                 = config;
        _cuspLongitudesByRecord = cuspLongitudesByRecord;
    }

    public FactorsInHousesResult Run()
    {
        if (!_config.UseEcliptical) throw new FactorsInHousesException(FactorsInHousesError.EclipticalNotEnabled);
        if (_cuspLongitudesByRecord.Count == 0) throw new FactorsInHousesException(FactorsInHousesError.NoCuspData);

        var layouts     = _config.FactorLayouts;
        var recordCount = (int)_binaryFile.RecordCount;
        var nrOfHouses  = _cuspLongitudesByRecord[0].Count;
        var factorCount = layouts.Count;

        var dataCounts    = new int[factorCount][];
        var controlCounts = new int[factorCount][];
        for (var f = 0; f < factorCount; f++)
        {
            dataCounts[f]    = new int[nrOfHouses];
            controlCounts[f] = new int[nrOfHouses];
        }
        var skipped = 0;

        for (var position = 0; position < recordCount; position++)
        {
            bool isData; byte[] regionData;
            try { var rec = _binaryFile.ReadRecord(position); isData = rec.IsData; regionData = rec.Data; }
            catch { skipped++; continue; }

            if (position >= _cuspLongitudesByRecord.Count) { skipped++; continue; }
            var cusps = _cuspLongitudesByRecord[position];

            for (var fi = 0; fi < factorCount; fi++)
            {
                var layout = layouts[fi];
                if (!layout.HasEcliptical) continue;

                double lon;
                try { lon = ResultsBinaryFile.ReadDouble(regionData, layout.ByteOffset); }
                catch { skipped++; continue; }

                var normalised = ((lon % 360) + 360) % 360;

                // Ascendant and MC: add tiny epsilon so they always land on their own cusp boundary.
                if (layout.Factor == Factors.Ascendant || layout.Factor == Factors.Mc)
                {
                    const double epsilon = 0.01 / 3600.0;
                    normalised = ((normalised + epsilon) % 360 + 360) % 360;
                }

                var houseIndex = HouseIndexFor(normalised, cusps);
                if (houseIndex < 0) { skipped++; continue; }

                if (isData) dataCounts[fi][houseIndex]++;
                else        controlCounts[fi][houseIndex]++;
            }
        }

        var distributions = new List<FactorHouseDistribution>(factorCount);
        for (var fi = 0; fi < factorCount; fi++)
        {
            var houseCounts = new List<HouseCount>(nrOfHouses);
            for (var hi = 0; hi < nrOfHouses; hi++)
                houseCounts.Add(new HouseCount(hi + 1, dataCounts[fi][hi], controlCounts[fi][hi]));
            distributions.Add(new FactorHouseDistribution { Factor = layouts[fi].Factor, HouseCounts = houseCounts });
        }
        return new FactorsInHousesResult(distributions, nrOfHouses, skipped);
    }

    /// <summary>Returns the 0-based house index for the given longitude, or -1 if unplaceable.</summary>
    private static int HouseIndexFor(double longitude, IReadOnlyList<double> cusps)
    {
        var n = cusps.Count;
        for (var i = 0; i < n; i++)
        {
            var first  = cusps[i];
            var second = cusps[(i + 1) % n];
            if (first > second)
            {
                if (longitude >= first || longitude < second) return i;
            }
            else
            {
                if (longitude >= first && longitude < second) return i;
            }
        }
        return -1;
    }
}
