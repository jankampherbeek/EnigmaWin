// AnalysisOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Research.Pipeline;
using InquiriesEnum = EnigmaWin.Sources.Features.Research.Inquiries.Inquiries;

namespace EnigmaWin.Sources.Features.Research.Analysis;

// ── Errors ────────────────────────────────────────────────────────────────────

public enum AnalysisOrchestratorError
{
    MissingResultsFile,
    ConfigDecodingFailed,
    InquiryNotSupported,
    WorkerFailed,
    MissingInputDatabase,
    MissingCalculationConfig
}

public sealed class AnalysisOrchestratorException(AnalysisOrchestratorError error, string detail = "")
    : Exception($"AnalysisOrchestrator error: {error} {detail}")
{ public AnalysisOrchestratorError Error => error; }

// ── Typed union of all possible analysis results ──────────────────────────────

public abstract class AnalysisResult
{
    private AnalysisResult() { }

    public sealed class FactorsInSigns  : AnalysisResult { public required FactorsInSignsResult  Result { get; init; } }
    public sealed class FactorsInHouses : AnalysisResult { public required FactorsInHousesResult Result { get; init; } }
    public sealed class Aspects         : AnalysisResult { public required AspectsResult         Result { get; init; } }
    public sealed class Unaspect        : AnalysisResult { public required UnaspectResult        Result { get; init; } }
    public sealed class Midpoints       : AnalysisResult { public required MidpointsResult       Result { get; init; } }
    public sealed class Harmonics       : AnalysisResult { public required HarmonicsResult       Result { get; init; } }
    public sealed class Parallels       : AnalysisResult { public required ParallelsResult       Result { get; init; } }
    public sealed class DeclMidpoints   : AnalysisResult { public required DeclMidpointsResult   Result { get; init; } }
    public sealed class Oob             : AnalysisResult { public required OobResult             Result { get; init; } }
}

// ── Orchestrator ──────────────────────────────────────────────────────────────

/// <summary>Dispatches analysis for a project to the correct worker based on the project's inquiry.
/// Opens results.bin (memory-mapped), runs the worker synchronously, and returns the result.
/// Call from a background Task — the work is synchronous and potentially large.</summary>
public sealed class AnalysisOrchestrator
{
    /// <summary>Entry point. Accepts plain strings instead of a model object so it can
    /// safely be called across thread/task boundaries.</summary>
    public AnalysisResult RunWith(string path, string configJson, int inquiryId)
    {
        ResearchConfig config;
        try { config = ResearchConfig.FromJson(configJson); }
        catch { throw new AnalysisOrchestratorException(AnalysisOrchestratorError.ConfigDecodingFailed); }

        ResultsBinaryFile binaryFile;
        try { binaryFile = ResultsBinaryFile.OpenForReading(path); }
        catch { throw new AnalysisOrchestratorException(AnalysisOrchestratorError.MissingResultsFile, path); }

        binaryFile.OpenMemoryMap();

        if (!Enum.IsDefined(typeof(InquiriesEnum), inquiryId))
            throw new AnalysisOrchestratorException(AnalysisOrchestratorError.InquiryNotSupported,
                                                    $"id={inquiryId}");
        var inquiry = (InquiriesEnum)inquiryId;

        return Dispatch(inquiry, config, binaryFile, path);
    }

    /// <summary>Convenience overload taking a ResearchProject domain model.</summary>
    public AnalysisResult Run(ResearchProject project) =>
        RunWith(project.Path, project.Config, project.Inquiry.GetHashCode());

    // ── Dispatch ──────────────────────────────────────────────────────────────

    private AnalysisResult Dispatch(InquiriesEnum inquiry, ResearchConfig config,
                                    ResultsBinaryFile binaryFile, string projectPath)
    {
        try
        {
            switch (inquiry)
            {
                case InquiriesEnum.FactorsInSigns:
                {
                    var worker = new FactorsInSignsWorker(binaryFile, config);
                    return new AnalysisResult.FactorsInSigns { Result = worker.Run() };
                }

                case InquiriesEnum.FactorsInHouses:
                {
                    var cusps  = CuspLongitudes(projectPath, config);
                    var worker = new FactorsInHousesWorker(binaryFile, config, cusps);
                    return new AnalysisResult.FactorsInHouses { Result = worker.Run() };
                }

                case InquiriesEnum.Aspects:
                {
                    var (angles, orb) = AspectParameters(config);
                    var worker = new AspectsWorker(binaryFile, config, angles, orb);
                    return new AnalysisResult.Aspects { Result = worker.Run() };
                }

                case InquiriesEnum.Unaspect:
                {
                    var (angles, _) = AspectParameters(config);
                    var unaspectOrb = config.UnaspectOrbOverride
                                   ?? config.OrbConfig?.AspectBaseOrb
                                   ?? 8.0;
                    var worker = new UnaspectWorker(binaryFile, config, angles, unaspectOrb);
                    return new AnalysisResult.Unaspect { Result = worker.Run() };
                }

                case InquiriesEnum.Midpoints:
                {
                    var dialSizes = config.EnabledDialSizes ?? [360];
                    var orbsPerDialSize = new Dictionary<int, double>
                    {
                        [360] = config.OrbConfig?.Midpoint360DialOrb ?? 1.5,
                        [90]  = config.OrbConfig?.Midpoint90DialOrb  ?? 1.0,
                        [45]  = config.OrbConfig?.Midpoint45DialOrb  ?? 0.5
                    };
                    var worker = new MidpointsWorker(binaryFile, config, dialSizes, orbsPerDialSize);
                    return new AnalysisResult.Midpoints { Result = worker.Run() };
                }

                case InquiriesEnum.Harmonics:
                {
                    var orb           = config.HarmonicOrbOverride ?? config.OrbConfig?.HarmonicOrb ?? 1.0;
                    var harmonicNumber = config.HarmonicNumber ?? 5;
                    var worker = new HarmonicsWorker(binaryFile, config, harmonicNumber, orb);
                    return new AnalysisResult.Harmonics { Result = worker.Run() };
                }

                case InquiriesEnum.Parallels:
                {
                    var orb    = config.ParallelOrbOverride ?? config.OrbConfig?.ParallelOrb ?? 1.0;
                    var worker = new ParallelsWorker(binaryFile, config, orb);
                    return new AnalysisResult.Parallels { Result = worker.Run() };
                }

                case InquiriesEnum.DeclMidpoints:
                {
                    var orb    = config.DeclMidpointOrbOverride ?? config.OrbConfig?.DeclinationMidpointOrb ?? 1.0;
                    var worker = new DeclMidpointsWorker(binaryFile, config, orb);
                    return new AnalysisResult.DeclMidpoints { Result = worker.Run() };
                }

                case InquiriesEnum.Oob:
                {
                    var obliquities = ObliquitiesPerRecord(projectPath);
                    var worker = new OobWorker(binaryFile, config, obliquities);
                    return new AnalysisResult.Oob { Result = worker.Run() };
                }

                default:
                    throw new AnalysisOrchestratorException(AnalysisOrchestratorError.InquiryNotSupported,
                                                            inquiry.ToString());
            }
        }
        catch (AnalysisOrchestratorException) { throw; }
        catch (Exception ex)
        {
            throw new AnalysisOrchestratorException(AnalysisOrchestratorError.WorkerFailed, ex.Message);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static (List<double> angles, double orb) AspectParameters(ResearchConfig config)
    {
        var orb = config.AspectOrbOverride ?? config.OrbConfig?.AspectBaseOrb ?? 8.0;
        List<double> angles;
        if (config.EnabledAspectIds is { Count: > 0 } ids)
        {
            angles = ids.Select(id => Enum.IsDefined(typeof(Aspects), id) ? (Aspects?)((Aspects)id) : null)
                        .Where(a => a.HasValue)
                        .Select(a => a!.Value.Angle())
                        .ToList();
        }
        else
        {
            angles = [0, 30, 45, 60, 72, 90, 120, 135, 144, 150, 180];
        }
        return (angles, orb);
    }

    /// <summary>Re-calculates house cusp longitudes for every record in the project database.
    /// Returns one array of cusp longitudes per record (indexed by cusp 0 = house 1).</summary>
    private static IReadOnlyList<IReadOnlyList<double>> CuspLongitudes(string path, ResearchConfig config)
    {
        if (config.CalculationConfig is null)
            throw new AnalysisOrchestratorException(AnalysisOrchestratorError.MissingCalculationConfig);

        ResearchDbManager db;
        try { db = new ResearchDbManager(path); }
        catch { throw new AnalysisOrchestratorException(AnalysisOrchestratorError.MissingInputDatabase, path); }

        using (db)
        {
            var records    = db.FetchAll().ToList();
            var calcConfig = config.CalculationConfig ?? CalculationConfig.Default;
            var result     = new List<IReadOnlyList<double>>(records.Count);

            foreach (var record in records)
            {
                var utHour = UtDecimalHour(record.Hour, record.Minute, record.Second, record.Offset);
                var date   = new AstronomicalDate(record.Year, record.Month, record.Day);
                var time   = new AstronomicalTime(utHour);
                var julDay = SEWrapper.JulianDay(date, time);
                var calcRequest = new CalcRequest(
                    julDay,
                    new List<Factors>(),
                    (int)calcConfig.HouseSystem.SeId(),
                    0,
                    record.GeoLat,
                    record.GeoLon,
                    0,
                    calcConfig);
                var chart = AstronCalcOrchestrator.PerformCalculation(calcRequest);
                result.Add(chart.HousePositions.Cusps.Select(c => c.Longitude).ToList());
            }
            return result;
        }
    }

    /// <summary>Calculates the true obliquity of the ecliptic for every record in the project database.
    /// Uses Swiss Ephemeris factor id -1 with flags 2, which returns the true obliquity in mainPos.</summary>
    private static IReadOnlyList<double> ObliquitiesPerRecord(string path)
    {
        ResearchDbManager db;
        try { db = new ResearchDbManager(path); }
        catch { throw new AnalysisOrchestratorException(AnalysisOrchestratorError.MissingInputDatabase, path); }

        using (db)
        {
            var records = db.FetchAll().ToList();
            var result  = new List<double>(records.Count);

            foreach (var record in records)
            {
                var utHour = UtDecimalHour(record.Hour, record.Minute, record.Second, record.Offset);
                var date   = new AstronomicalDate(record.Year, record.Month, record.Day);
                var time   = new AstronomicalTime(utHour);
                var julDay = SEWrapper.JulianDay(date, time);
                var pos    = SEWrapper.CalculateFactorPosition(julDay, -1, 2);
                result.Add(pos?.MainPos ?? 23.4393);
            }
            return result;
        }
    }

    private static double UtDecimalHour(int hour, int minute, int second, double offset) =>
        hour + minute / 60.0 + second / 3600.0 - offset;
}
