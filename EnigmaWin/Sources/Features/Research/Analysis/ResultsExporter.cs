// ResultsExporter.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Research.Analysis;

// ── Errors ────────────────────────────────────────────────────────────────────

public enum ResultsExporterError { WriteFailed, UnsupportedResult }

public sealed class ResultsExporterException(ResultsExporterError error, string detail = "")
    : Exception($"ResultsExporter error: {error} {detail}") { public ResultsExporterError Error => error; }

// ── Exporter ──────────────────────────────────────────────────────────────────

/// <summary>Exports analysis results to a semicolon-delimited CSV file.
/// Each AnalysisResult case produces its own column layout.
/// When divisor > 1, control-group counts are divided by divisor and formatted to one decimal place.</summary>
public sealed class ResultsExporter
{
    /// <summary>Exports result to a CSV file at destinationPath.</summary>
    /// <param name="result">The analysis result to export.</param>
    /// <param name="destinationPath">Full path of the output file.</param>
    /// <param name="divisor">1 for raw control counts; cgMultiplication for proportional counts.</param>
    public void Export(AnalysisResult result, string destinationPath, int divisor = 1)
    {
        var csv = result switch
        {
            AnalysisResult.FactorsInSigns  r => BuildFactorsInSignsCsv(r.Result,  divisor),
            AnalysisResult.FactorsInHouses r => BuildFactorsInHousesCsv(r.Result, divisor),
            AnalysisResult.Aspects         r => BuildAspectsCsv(r.Result,         divisor),
            AnalysisResult.Unaspect        r => BuildUnaspectCsv(r.Result,        divisor),
            AnalysisResult.Midpoints       r => BuildMidpointsCsv(r.Result,       divisor),
            AnalysisResult.Harmonics       r => BuildHarmonicsCsv(r.Result,       divisor),
            AnalysisResult.Parallels       r => BuildParallelsCsv(r.Result,       divisor),
            AnalysisResult.DeclMidpoints   r => BuildDeclMidpointsCsv(r.Result,   divisor),
            AnalysisResult.Oob             r => BuildOobCsv(r.Result,             divisor),
            _ => throw new ResultsExporterException(ResultsExporterError.UnsupportedResult)
        };
        try { File.WriteAllText(destinationPath, csv, Encoding.UTF8); }
        catch { throw new ResultsExporterException(ResultsExporterError.WriteFailed, destinationPath); }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string Ctrl(int count, int divisor) =>
        divisor > 1 ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F1}", (double)count / divisor)
                    : count.ToString();

    // ── Factors in Signs ──────────────────────────────────────────────────────

    private static string BuildFactorsInSignsCsv(FactorsInSignsResult result, int divisor)
    {
        var lines = new List<string>();
        var signHeaders = string.Join(";", Enum.GetValues<Signs>());
        var colHeader   = $"Factor;{signHeaders};Total";

        var dataSignTotals = Enumerable.Range(0, 12)
            .Select(si => result.Distributions.Sum(d => d.SignCounts[si].DataCount)).ToList();
        var ctrlSignTotals = Enumerable.Range(0, 12)
            .Select(si => result.Distributions.Sum(d => d.SignCounts[si].ControlCount)).ToList();
        var dataGrandTotal = dataSignTotals.Sum();
        var ctrlGrandTotal = ctrlSignTotals.Sum();

        lines.Add("Data");
        lines.Add(colHeader);
        foreach (var dist in result.Distributions)
        {
            var dataCells = string.Join(";", dist.SignCounts.Select(c => c.DataCount));
            lines.Add($"{dist.Factor};{dataCells};{dist.TotalData}");
        }
        lines.Add($"Total;{string.Join(";", dataSignTotals)};{dataGrandTotal}");

        lines.Add("");
        lines.Add("Control group");
        lines.Add(colHeader);
        foreach (var dist in result.Distributions)
        {
            var ctrlCells = string.Join(";", dist.SignCounts.Select(c => Ctrl(c.ControlCount, divisor)));
            lines.Add($"{dist.Factor};{ctrlCells};{Ctrl(dist.TotalControl, divisor)}");
        }
        lines.Add($"Total;{string.Join(";", ctrlSignTotals.Select(t => Ctrl(t, divisor)))};{Ctrl(ctrlGrandTotal, divisor)}");

        if (result.SkippedRecords > 0) { lines.Add(""); lines.Add($"# Skipped records: {result.SkippedRecords}"); }
        return string.Join("\n", lines);
    }

    // ── Factors in Houses ─────────────────────────────────────────────────────

    private static string BuildFactorsInHousesCsv(FactorsInHousesResult result, int divisor)
    {
        var lines = new List<string>();
        var houseHeaders = string.Join(";", Enumerable.Range(1, result.NrOfHouses).Select(h => $"House {h}"));
        var colHeader    = $"Factor;{houseHeaders};Total";

        var dataHouseTotals = Enumerable.Range(0, result.NrOfHouses)
            .Select(hi => result.Distributions.Sum(d => d.HouseCounts[hi].DataCount)).ToList();
        var ctrlHouseTotals = Enumerable.Range(0, result.NrOfHouses)
            .Select(hi => result.Distributions.Sum(d => d.HouseCounts[hi].ControlCount)).ToList();
        var dataGrandTotal = dataHouseTotals.Sum();
        var ctrlGrandTotal = ctrlHouseTotals.Sum();

        lines.Add("Data");
        lines.Add(colHeader);
        foreach (var dist in result.Distributions)
        {
            var dataCells = string.Join(";", dist.HouseCounts.Select(c => c.DataCount));
            lines.Add($"{dist.Factor};{dataCells};{dist.TotalData}");
        }
        lines.Add($"Total;{string.Join(";", dataHouseTotals)};{dataGrandTotal}");

        lines.Add("");
        lines.Add("Control group");
        lines.Add(colHeader);
        foreach (var dist in result.Distributions)
        {
            var ctrlCells = string.Join(";", dist.HouseCounts.Select(c => Ctrl(c.ControlCount, divisor)));
            lines.Add($"{dist.Factor};{ctrlCells};{Ctrl(dist.TotalControl, divisor)}");
        }
        lines.Add($"Total;{string.Join(";", ctrlHouseTotals.Select(t => Ctrl(t, divisor)))};{Ctrl(ctrlGrandTotal, divisor)}");

        if (result.SkippedRecords > 0) { lines.Add(""); lines.Add($"# Skipped records: {result.SkippedRecords}"); }
        return string.Join("\n", lines);
    }

    // ── Aspects ───────────────────────────────────────────────────────────────

    private static string BuildAspectsCsv(AspectsResult result, int divisor)
    {
        var lines  = new List<string>();
        var angles = result.Counts.Select(c => c.AspectAngle).Distinct().OrderBy(a => a).ToList();

        var seenPairs  = new HashSet<string>();
        var factorPairs = new List<(Factors, Factors)>();
        foreach (var c in result.Counts)
        {
            var key = $"{(int)c.Factor1}-{(int)c.Factor2}";
            if (seenPairs.Add(key)) factorPairs.Add((c.Factor1, c.Factor2));
        }

        int Lookup(Factors f1, Factors f2, double angle, bool isData)
        {
            var match = result.Counts.FirstOrDefault(c => c.Factor1 == f1 && c.Factor2 == f2 && c.AspectAngle == angle);
            return match is null ? 0 : (isData ? match.DataCount : match.ControlCount);
        }

        var angleHeaders = string.Join(";", angles.Select(a => FormatAngle(a) + "°"));
        var colHeader    = $"Factor 1;Factor 2;{angleHeaders};Total";

        foreach (var (sectionLabel, isData) in new[] { ("Data", true), ("Control group", false) })
        {
            var angleTotals = angles.Select(angle =>
                result.Counts.Where(c => c.AspectAngle == angle)
                             .Sum(c => isData ? c.DataCount : c.ControlCount)).ToList();
            var grandTotal = angleTotals.Sum();

            lines.Add(sectionLabel);
            lines.Add(colHeader);
            foreach (var (f1, f2) in factorPairs)
            {
                var row      = angles.Select(a => Lookup(f1, f2, a, isData)).ToList();
                var rowTotal = row.Sum();
                var cells    = string.Join(";", row.Select(v => isData ? v.ToString() : Ctrl(v, divisor)));
                var rowTotalStr = isData ? rowTotal.ToString() : Ctrl(rowTotal, divisor);
                lines.Add($"{f1};{f2};{cells};{rowTotalStr}");
            }
            var totCells    = string.Join(";", angleTotals.Select(t => isData ? t.ToString() : Ctrl(t, divisor)));
            var grandTotalStr = isData ? grandTotal.ToString() : Ctrl(grandTotal, divisor);
            lines.Add($"Total;;{totCells};{grandTotalStr}");
            lines.Add("");
        }

        if (result.SkippedRecords > 0) lines.Add($"# Skipped records: {result.SkippedRecords}");
        return string.Join("\n", lines);
    }

    // ── Unaspect ──────────────────────────────────────────────────────────────

    private static string BuildUnaspectCsv(UnaspectResult result, int divisor)
    {
        var lines     = new List<string>();
        var colHeader = "Factor;Count";

        lines.Add("Data");
        lines.Add(colHeader);
        foreach (var c in result.Counts) lines.Add($"{c.Factor};{c.DataCount}");

        lines.Add("");
        lines.Add("Control group");
        lines.Add(colHeader);
        foreach (var c in result.Counts) lines.Add($"{c.Factor};{Ctrl(c.ControlCount, divisor)}");

        if (result.SkippedRecords > 0) { lines.Add(""); lines.Add($"# Skipped records: {result.SkippedRecords}"); }
        return string.Join("\n", lines);
    }

    // ── Midpoints ─────────────────────────────────────────────────────────────

    private static string BuildMidpointsCsv(MidpointsResult result, int divisor)
    {
        var lines     = new List<string>();
        var colHeader = "Factor A;Factor B;Occupant;Dial size;Count";

        lines.Add("Data");
        lines.Add(colHeader);
        foreach (var c in result.Counts)
            lines.Add($"{c.FactorA};{c.FactorB};{c.Occupant};{c.DialSize};{c.DataCount}");

        lines.Add("");
        lines.Add("Control group");
        lines.Add(colHeader);
        foreach (var c in result.Counts)
            lines.Add($"{c.FactorA};{c.FactorB};{c.Occupant};{c.DialSize};{Ctrl(c.ControlCount, divisor)}");

        if (result.SkippedRecords > 0) { lines.Add(""); lines.Add($"# Skipped records: {result.SkippedRecords}"); }
        return string.Join("\n", lines);
    }

    // ── Harmonics ─────────────────────────────────────────────────────────────

    private static string BuildHarmonicsCsv(HarmonicsResult result, int divisor)
    {
        var lines     = new List<string>();
        var colHeader = "Harmonic factor;Radix factor;Count";

        lines.Add($"Harmonic number;{result.HarmonicNumber}");
        lines.Add("");

        lines.Add("Data");
        lines.Add(colHeader);
        foreach (var c in result.Counts) lines.Add($"{c.HarmonicFactor};{c.RadixFactor};{c.DataCount}");

        lines.Add("");
        lines.Add("Control group");
        lines.Add(colHeader);
        foreach (var c in result.Counts) lines.Add($"{c.HarmonicFactor};{c.RadixFactor};{Ctrl(c.ControlCount, divisor)}");

        if (result.SkippedRecords > 0) { lines.Add(""); lines.Add($"# Skipped records: {result.SkippedRecords}"); }
        return string.Join("\n", lines);
    }

    // ── Parallels ─────────────────────────────────────────────────────────────

    private static string BuildParallelsCsv(ParallelsResult result, int divisor)
    {
        var lines     = new List<string>();
        var colHeader = "Factor 1;Factor 2;Parallel;Contra-parallel;Total";

        var seen  = new HashSet<string>();
        var pairs = new List<(Factors, Factors)>();
        foreach (var c in result.Counts)
        {
            var key = $"{(int)c.Factor1}-{(int)c.Factor2}";
            if (seen.Add(key)) pairs.Add((c.Factor1, c.Factor2));
        }
        pairs.Sort((x, y) =>
        {
            var cmp = ((int)x.Item1).CompareTo((int)y.Item1);
            return cmp != 0 ? cmp : ((int)x.Item2).CompareTo((int)y.Item2);
        });

        (int data, int control) ParallelCount(Factors f1, Factors f2, bool isContra)
        {
            var match = result.Counts.FirstOrDefault(c => c.Factor1 == f1 && c.Factor2 == f2 && c.IsContraParallel == isContra);
            return (match?.DataCount ?? 0, match?.ControlCount ?? 0);
        }

        foreach (var (sectionLabel, isData) in new[] { ("Data", true), ("Control group", false) })
        {
            lines.Add(sectionLabel);
            lines.Add(colHeader);
            var totalP = 0; var totalC = 0;
            foreach (var (f1, f2) in pairs)
            {
                var p    = ParallelCount(f1, f2, isContra: false);
                var c    = ParallelCount(f1, f2, isContra: true);
                var pRaw = isData ? p.data    : p.control;
                var cRaw = isData ? c.data    : c.control;
                var pOut = isData ? pRaw.ToString() : Ctrl(pRaw, divisor);
                var cOut = isData ? cRaw.ToString() : Ctrl(cRaw, divisor);
                var sOut = isData ? (pRaw + cRaw).ToString() : Ctrl(pRaw + cRaw, divisor);
                lines.Add($"{f1};{f2};{pOut};{cOut};{sOut}");
                totalP += pRaw; totalC += cRaw;
            }
            var tpOut = isData ? totalP.ToString() : Ctrl(totalP, divisor);
            var tcOut = isData ? totalC.ToString() : Ctrl(totalC, divisor);
            var tsOut = isData ? (totalP + totalC).ToString() : Ctrl(totalP + totalC, divisor);
            lines.Add($"Total;{tpOut};{tcOut};{tsOut}");
            lines.Add("");
        }

        if (result.SkippedRecords > 0) lines.Add($"# Skipped records: {result.SkippedRecords}");
        return string.Join("\n", lines);
    }

    // ── Declination Midpoints ─────────────────────────────────────────────────

    private static string BuildDeclMidpointsCsv(DeclMidpointsResult result, int divisor)
    {
        var lines     = new List<string>();
        var colHeader = "Factor A;Factor B;Occupant;Count";

        lines.Add("Data");
        lines.Add(colHeader);
        foreach (var c in result.Counts) lines.Add($"{c.FactorA};{c.FactorB};{c.Occupant};{c.DataCount}");

        lines.Add("");
        lines.Add("Control group");
        lines.Add(colHeader);
        foreach (var c in result.Counts) lines.Add($"{c.FactorA};{c.FactorB};{c.Occupant};{Ctrl(c.ControlCount, divisor)}");

        if (result.SkippedRecords > 0) { lines.Add(""); lines.Add($"# Skipped records: {result.SkippedRecords}"); }
        return string.Join("\n", lines);
    }

    // ── Out of Bounds ─────────────────────────────────────────────────────────

    private static string BuildOobCsv(OobResult result, int divisor)
    {
        var lines     = new List<string>();
        var colHeader = "Factor;Count";

        lines.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                "Obliquity threshold;{0}", result.Obliquity));
        lines.Add("");

        lines.Add("Data");
        lines.Add(colHeader);
        foreach (var c in result.Counts) lines.Add($"{c.Factor};{c.DataCount}");

        lines.Add("");
        lines.Add("Control group");
        lines.Add(colHeader);
        foreach (var c in result.Counts) lines.Add($"{c.Factor};{Ctrl(c.ControlCount, divisor)}");

        if (result.SkippedRecords > 0) { lines.Add(""); lines.Add($"# Skipped records: {result.SkippedRecords}"); }
        return string.Join("\n", lines);
    }

    // ── Formatting helpers ────────────────────────────────────────────────────

    private static string FormatAngle(double angle)
    {
        var formatted = angle.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
        return formatted;
    }
}
