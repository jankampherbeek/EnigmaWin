// ImportOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.IO;

namespace EnigmaWin.Sources.Features.Research.Pipeline;

/// <summary>Coordinates the full import flow for one data file into a research project:
/// 1. Parse the source file using the supplied IDataImporter
/// 2. Persist real-data records into horoscope_data.db (reset first for a clean import)
/// 3. Generate control-group records using ControlGroupGenerator
/// 4. Persist control-group records into the same database
///
/// All work is synchronous — call from a background Task.</summary>
public sealed class ImportOrchestrator
{
    private readonly ControlGroupGenerator _generator;

    public ImportOrchestrator(ControlGroupGenerator? generator = null)
    {
        _generator = generator ?? new ControlGroupGenerator();
    }

    /// <summary>Runs the import pipeline.</summary>
    /// <param name="sourceFile">Path to the data file to import.</param>
    /// <param name="importer">The IDataImporter matching the file's format.</param>
    /// <param name="projectPath">Directory that contains (or will contain) horoscope_data.db.</param>
    /// <param name="cgMultiplication">Control-group multiplier from the ResearchProject.</param>
    /// <returns>(realCount, controlCount)</returns>
    public (int RealCount, int ControlCount) Run(
        string sourceFile,
        IDataImporter importer,
        string projectPath,
        int cgMultiplication)
    {
        // ── 1. Parse ──────────────────────────────────────────────────────────
        System.Collections.Generic.List<ResearchInputRecord> realRecords;
        try
        {
            realRecords = importer.Parse(sourceFile, isData: true, startId: 1);
        }
        catch (DataImportException) { throw; }
        catch (Exception ex)
        {
            throw new ImportException($"Parse failed: {ex.Message}", ex);
        }

        if (realRecords.Count == 0)
            throw new ImportException("No records were parsed from the source file.");

        // ── 2. Open/reset the database ────────────────────────────────────────
        // Strategy: try to delete the file first (cleanest). If the file is
        // locked or permissions deny deletion, open and DROP+recreate instead.
        var dbPath  = Path.Combine(projectPath, "horoscope_data.db");
        var deleted = TryDeleteDbFiles(dbPath);

        ResearchDbManager db;
        try
        {
            db = new ResearchDbManager(projectPath);
            if (!deleted) db.ResetTable();
        }
        catch (Exception ex)
        {
            throw new ImportException($"Cannot open database at '{projectPath}': {ex.Message}", ex);
        }

        using (db)
        {
            // ── 3. Persist real data ──────────────────────────────────────────
            try { db.InsertAll(realRecords); }
            catch (Exception ex) { throw new ImportException($"Database write failed: {ex.Message}", ex); }

            // ── 4. Generate and persist control group ─────────────────────────
            var controlStartId = (realRecords[^1].Id) + 1;
            var controlRecords = _generator.Generate(realRecords, cgMultiplication, controlStartId);

            if (controlRecords.Count > 0)
            {
                try { db.InsertAll(controlRecords); }
                catch (Exception ex) { throw new ImportException($"Control-group write failed: {ex.Message}", ex); }
            }

            return (realRecords.Count, controlRecords.Count);
        }
    }

    /// <summary>Convenience overload that selects the importer from the DataFileType.</summary>
    public (int RealCount, int ControlCount) Run(
        string sourceFile,
        DataFileType fileType,
        string projectPath,
        int cgMultiplication)
    {
        IDataImporter importer = fileType switch
        {
            DataFileType.StandardEnigma => new EnigmaFormatImporter(),
            DataFileType.Gauquelin      => new GauquelinImporter(),
            DataFileType.QuickChart     => new QuickChartImporter(),
            _ => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null)
        };
        return Run(sourceFile, importer, projectPath, cgMultiplication);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static bool TryDeleteDbFiles(string dbPath)
    {
        try
        {
            if (File.Exists(dbPath))           File.Delete(dbPath);
            if (File.Exists(dbPath + "-wal"))  File.Delete(dbPath + "-wal");
            if (File.Exists(dbPath + "-shm"))  File.Delete(dbPath + "-shm");
            return true;
        }
        catch { return false; }
    }
}

public sealed class ImportException(string message, Exception? inner = null)
    : Exception(message, inner);
