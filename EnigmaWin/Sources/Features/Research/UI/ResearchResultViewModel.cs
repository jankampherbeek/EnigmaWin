// ResearchResultViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Research.Analysis;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using InquiriesEnum = EnigmaWin.Sources.Features.Research.Inquiries.Inquiries;

namespace EnigmaWin.Sources.Features.Research.UI;

// ── Simple list row (used for non-matrix inquiry types) ──────────────────────

public sealed record SimpleRow(string Col1, string Col2 = "", string Col3 = "", string Col4 = "", string Col5 = "", bool IsEvenRow = false);

// ── Matrix cell ───────────────────────────────────────────────────────────────

public sealed record MatrixRow(string RowLabel, IReadOnlyList<string> Cells, string Total, bool IsEvenRow = false);

// ── ViewModel ─────────────────────────────────────────────────────────────────

public sealed partial class ResearchResultViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IRosetta           _rosetta;
    private readonly AnalysisResult     _result;
    private readonly ResearchProject    _project;
    private readonly int                _cgMultiplier;

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelTitle        => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.title");
    public string LabelBack         => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.button.back");
    public string LabelExport       => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.button.export");
    public string LabelToggleMean   => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.button.mean");
    public string LabelToggleTotal  => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.button.total");
    public string LabelData         => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.section.data");
    public string LabelControl      => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.section.control");
    public string LabelFactor       => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.col.factor");
    public string LabelFactor1      => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.col.factor1");
    public string LabelFactor2      => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.col.factor2");
    public string LabelFactorA      => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.col.factora");
    public string LabelFactorB      => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.col.factorb");
    public string LabelOccupant     => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.col.occupant");
    public string LabelDial         => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.col.dial");
    public string LabelTotal        => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.col.total");
    public string LabelCount        => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.col.count");
    public string LabelParallel     => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.col.parallel");
    public string LabelContra       => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.col.contra");
    public string LabelHarmonic     => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.col.harmonic");
    public string LabelRadix        => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.col.radix");
    public string TooltipHelp       => _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.help.tooltip");

    // ── Header info ──────────────────────────────────────────────────────────

    public string ProjectName    { get; }
    public string InquiryDisplay { get; }
    public string SkippedMessage { get; }
    public bool   HasSkipped     { get; }

    // ── Proportional toggle ───────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ToggleLabel))]
    [NotifyPropertyChangedFor(nameof(DataMatrixRows))]
    [NotifyPropertyChangedFor(nameof(ControlMatrixRows))]
    [NotifyPropertyChangedFor(nameof(DataListRows))]
    [NotifyPropertyChangedFor(nameof(ControlListRows))]
    [NotifyPropertyChangedFor(nameof(DataColTotals))]
    [NotifyPropertyChangedFor(nameof(ControlColTotals))]
    [NotifyPropertyChangedFor(nameof(DataGrandTotal))]
    [NotifyPropertyChangedFor(nameof(ControlGrandTotal))]
    [NotifyPropertyChangedFor(nameof(MatrixColumnHeaders))]
    private bool proportional;

    partial void OnProportionalChanged(bool value)
    {
        ControlListTotal = BuildControlListTotal();
        OnPropertyChanged(nameof(ControlListTotal));
    }

    public bool ShowToggleButton => _cgMultiplier > 1;
    public string ToggleLabel => Proportional ? LabelToggleTotal : LabelToggleMean;
    private int Divisor => Proportional ? _cgMultiplier : 1;

    // ── Inquiry kind (drives which sub-template is visible) ──────────────────

    public InquiriesEnum Inquiry { get; }
    public bool IsMatrixInquiry     => Inquiry is InquiriesEnum.FactorsInSigns
                                                or InquiriesEnum.FactorsInHouses
                                                or InquiriesEnum.Aspects;
    public bool IsListInquiry       => !IsMatrixInquiry;
    public bool IsParallels         => Inquiry == InquiriesEnum.Parallels;
    public bool IsHarmonics         => Inquiry == InquiriesEnum.Harmonics;

    // ── Matrix headers & data (Signs / Houses / Aspects) ─────────────────────

    public IReadOnlyList<string> MatrixColumnHeaders { get; private set; } = [];

    public IReadOnlyList<MatrixRow> DataMatrixRows    => BuildMatrixRows(isData: true);
    public IReadOnlyList<MatrixRow> ControlMatrixRows => BuildMatrixRows(isData: false);
    public IReadOnlyList<string>    DataColTotals     => BuildColumnTotals(isData: true);
    public IReadOnlyList<string>    ControlColTotals  => BuildColumnTotals(isData: false);
    public string                   DataGrandTotal    => BuildGrandTotal(isData: true);
    public string                   ControlGrandTotal => BuildGrandTotal(isData: false);

    // ── List rows (for non-matrix inquiry types) ──────────────────────────────

    // Column headers for the list view (varies by inquiry)
    public IReadOnlyList<string> ListHeaders { get; }

    public IReadOnlyList<SimpleRow> DataListRows    => BuildListRows(isData: true);
    public IReadOnlyList<SimpleRow> ControlListRows => BuildListRows(isData: false);

    // For Harmonics: harmonic-number info line
    public string HarmonicInfoLine { get; }

    // For OOB: obliquity info line
    public string OobInfoLine  { get; }
    public bool   HasOobInfo   => !string.IsNullOrEmpty(OobInfoLine);

    // Totals row for list inquiries that have a meaningful grand total (Unaspect, Oob)
    public string DataListTotal    { get; private set; } = "";
    public string ControlListTotal { get; private set; } = "";

    // ── Export state ─────────────────────────────────────────────────────────

    [ObservableProperty]
    private string exportMessage = "";

    [ObservableProperty]
    private bool exportIsError;

    public bool HasExportMessage => !string.IsNullOrEmpty(ExportMessage);

    // ── Constructor ──────────────────────────────────────────────────────────

    public ResearchResultViewModel(
        IRosetta rosetta,
        INavigationService navigationService,
        AnalysisResult result,
        ResearchProject project,
        int cgMultiplier)
    {
        _rosetta           = rosetta;
        _navigationService = navigationService;
        _result            = result;
        _project           = project;
        _cgMultiplier      = cgMultiplier;

        Inquiry        = project.Inquiry;
        ProjectName    = project.Name;
        InquiryDisplay = rosetta.GetText(RbFile.Localizable, InquiryKey(project.Inquiry));

        var skipped = result switch
        {
            AnalysisResult.FactorsInSigns  r => r.Result.SkippedRecords,
            AnalysisResult.FactorsInHouses r => r.Result.SkippedRecords,
            AnalysisResult.Aspects         r => r.Result.SkippedRecords,
            AnalysisResult.Unaspect        r => r.Result.SkippedRecords,
            AnalysisResult.Midpoints       r => r.Result.SkippedRecords,
            AnalysisResult.Harmonics       r => r.Result.SkippedRecords,
            AnalysisResult.Parallels       r => r.Result.SkippedRecords,
            AnalysisResult.DeclMidpoints   r => r.Result.SkippedRecords,
            AnalysisResult.Oob             r => r.Result.SkippedRecords,
            _ => 0
        };
        HasSkipped     = skipped > 0;
        SkippedMessage = HasSkipped
            ? string.Format(rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.skipped"), skipped)
            : "";

        MatrixColumnHeaders = BuildColumnHeaders();
        ListHeaders         = BuildListHeaders();
        HarmonicInfoLine    = BuildHarmonicInfoLine();
        OobInfoLine         = BuildOobInfoLine();
        DataListTotal       = BuildDataListTotal();
        ControlListTotal    = BuildControlListTotal();
    }

    // ── Actions ──────────────────────────────────────────────────────────────

    public void Back()
    {
        _navigationService.NavigateMain(AppRoutes.ResearchProjectOpen,
            new ResearchProjectOpenNavigationParameter(_project.Id, _project.Name));
    }

    public void SetExportResult(string path, bool isError)
    {
        if (isError)
        {
            ExportMessage = path;
            ExportIsError = true;
        }
        else
        {
            ExportMessage  = string.Format(
                _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.export.ok"), path);
            ExportIsError  = false;
        }
        OnPropertyChanged(nameof(HasExportMessage));
    }

    public string GetDefaultExportFileName() => $"{_project.Name}.csv";

    public void ExportTo(string destinationPath)
    {
        var exporter = new ResultsExporter();
        exporter.Export(_result, destinationPath, Divisor);
    }

    // ── Column headers ────────────────────────────────────────────────────────

    private static readonly IReadOnlyList<string> SignAbbreviations =
        ["AR", "TA", "GE", "CN", "LE", "VI", "LI", "SC", "SA", "CP", "AQ", "PI"];

    private IReadOnlyList<string> BuildColumnHeaders() => _result switch
    {
        AnalysisResult.FactorsInSigns  _ => SignAbbreviations,
        AnalysisResult.FactorsInHouses r => Enumerable.Range(1, r.Result.NrOfHouses)
            .Select(h => h.ToString()).ToList(),
        AnalysisResult.Aspects         r => r.Result.Counts
            .Select(c => c.AspectAngle).Distinct().OrderBy(a => a)
            .Select(a => $"{a:G}°").ToList(),
        _ => []
    };

    // ── Matrix row builders ───────────────────────────────────────────────────

    private IReadOnlyList<MatrixRow> BuildMatrixRows(bool isData) => _result switch
    {
        AnalysisResult.FactorsInSigns r  => BuildSignsMatrixRows(r.Result, isData),
        AnalysisResult.FactorsInHouses r => BuildHousesMatrixRows(r.Result, isData),
        AnalysisResult.Aspects r         => BuildAspectsMatrixRows(r.Result, isData),
        _ => []
    };

    private IReadOnlyList<MatrixRow> BuildSignsMatrixRows(FactorsInSignsResult r, bool isData)
    {
        return r.Distributions.Select((dist, i) =>
        {
            var label = _rosetta.GetText(RbFile.Localizable, dist.Factor.LocalizedName());
            var cells = dist.SignCounts.Select(sc =>
                isData ? sc.DataCount.ToString() : CtrlText(sc.ControlCount)).ToList();
            var total = isData ? dist.TotalData.ToString() : CtrlText(dist.TotalControl);
            return new MatrixRow(label, cells, total, i % 2 == 0);
        }).ToList();
    }

    private IReadOnlyList<MatrixRow> BuildHousesMatrixRows(FactorsInHousesResult r, bool isData)
    {
        return r.Distributions.Select((dist, i) =>
        {
            var label = _rosetta.GetText(RbFile.Localizable, dist.Factor.LocalizedName());
            var cells = dist.HouseCounts.Select(hc =>
                isData ? hc.DataCount.ToString() : CtrlText(hc.ControlCount)).ToList();
            var total = isData ? dist.TotalData.ToString() : CtrlText(dist.TotalControl);
            return new MatrixRow(label, cells, total, i % 2 == 0);
        }).ToList();
    }

    private IReadOnlyList<MatrixRow> BuildAspectsMatrixRows(AspectsResult r, bool isData)
    {
        var angles = r.Counts.Select(c => c.AspectAngle).Distinct().OrderBy(a => a).ToList();
        var pairs = GetUniqueAspectPairs(r);
        return pairs.Select((pair, i) =>
        {
            var f1 = _rosetta.GetText(RbFile.Localizable, pair.Item1.LocalizedName());
            var f2 = _rosetta.GetText(RbFile.Localizable, pair.Item2.LocalizedName());
            var cells = angles.Select(angle =>
            {
                var c = r.Counts.FirstOrDefault(x => x.Factor1 == pair.Item1 && x.Factor2 == pair.Item2 && x.AspectAngle == angle);
                var raw = c is null ? 0 : (isData ? c.DataCount : c.ControlCount);
                return isData ? raw.ToString() : CtrlText(raw);
            }).ToList();
            var rowTotal = angles.Sum(angle =>
            {
                var c = r.Counts.FirstOrDefault(x => x.Factor1 == pair.Item1 && x.Factor2 == pair.Item2 && x.AspectAngle == angle);
                return c is null ? 0 : (isData ? c.DataCount : c.ControlCount);
            });
            var total = isData ? rowTotal.ToString() : CtrlText(rowTotal);
            return new MatrixRow($"{f1} / {f2}", cells, total, i % 2 == 0);
        }).ToList();
    }

    private IReadOnlyList<string> BuildColumnTotals(bool isData)
    {
        return _result switch
        {
            AnalysisResult.FactorsInSigns r =>
                Enumerable.Range(0, 12).Select(si =>
                {
                    var sum = r.Result.Distributions.Sum(d => isData ? d.SignCounts[si].DataCount : d.SignCounts[si].ControlCount);
                    return isData ? sum.ToString() : CtrlText(sum);
                }).ToList<string>(),
            AnalysisResult.FactorsInHouses r =>
                Enumerable.Range(0, r.Result.NrOfHouses).Select(hi =>
                {
                    var sum = r.Result.Distributions.Sum(d => isData ? d.HouseCounts[hi].DataCount : d.HouseCounts[hi].ControlCount);
                    return isData ? sum.ToString() : CtrlText(sum);
                }).ToList<string>(),
            AnalysisResult.Aspects r =>
                r.Result.Counts.Select(c => c.AspectAngle).Distinct().OrderBy(a => a)
                    .Select(angle =>
                    {
                        var sum = r.Result.Counts.Where(c => c.AspectAngle == angle).Sum(c => isData ? c.DataCount : c.ControlCount);
                        return isData ? sum.ToString() : CtrlText(sum);
                    }).ToList<string>(),
            _ => []
        };
    }

    private string BuildGrandTotal(bool isData)
    {
        return _result switch
        {
            AnalysisResult.FactorsInSigns r =>
                CtrlOrDirect(r.Result.Distributions.Sum(d => isData ? d.TotalData : d.TotalControl), isData),
            AnalysisResult.FactorsInHouses r =>
                CtrlOrDirect(r.Result.Distributions.Sum(d => isData ? d.TotalData : d.TotalControl), isData),
            AnalysisResult.Aspects r =>
                CtrlOrDirect(r.Result.Counts.Sum(c => isData ? c.DataCount : c.ControlCount), isData),
            _ => "0"
        };
    }

    // ── List row builders ─────────────────────────────────────────────────────

    private IReadOnlyList<string> BuildListHeaders() => _result switch
    {
        AnalysisResult.Unaspect      _ => [LabelFactor, LabelCount],
        AnalysisResult.Midpoints     _ => [LabelFactorA, LabelFactorB, LabelOccupant, LabelDial, LabelCount],
        AnalysisResult.Harmonics     _ => [LabelHarmonic, LabelRadix, LabelCount],
        AnalysisResult.Parallels     _ => [LabelFactor1, LabelFactor2, LabelParallel, LabelContra, LabelTotal],
        AnalysisResult.DeclMidpoints _ => [LabelFactorA, LabelFactorB, LabelOccupant, LabelCount],
        AnalysisResult.Oob           _ => [LabelFactor, LabelCount],
        _ => []
    };

    private IReadOnlyList<SimpleRow> BuildListRows(bool isData)
    {
        return _result switch
        {
            AnalysisResult.Unaspect r =>
                r.Result.Counts.Select((c, i) => new SimpleRow(
                    _rosetta.GetText(RbFile.Localizable, c.Factor.LocalizedName()),
                    isData ? c.DataCount.ToString() : CtrlText(c.ControlCount),
                    IsEvenRow: i % 2 == 0
                )).ToList<SimpleRow>(),

            AnalysisResult.Midpoints r =>
                r.Result.Counts.Select((c, i) => new SimpleRow(
                    _rosetta.GetText(RbFile.Localizable, c.FactorA.LocalizedName()),
                    _rosetta.GetText(RbFile.Localizable, c.FactorB.LocalizedName()),
                    _rosetta.GetText(RbFile.Localizable, c.Occupant.LocalizedName()),
                    c.DialSize.ToString(),
                    isData ? c.DataCount.ToString() : CtrlText(c.ControlCount),
                    i % 2 == 0
                )).ToList<SimpleRow>(),

            AnalysisResult.Harmonics r =>
                r.Result.Counts.Select((c, i) => new SimpleRow(
                    _rosetta.GetText(RbFile.Localizable, c.HarmonicFactor.LocalizedName()),
                    _rosetta.GetText(RbFile.Localizable, c.RadixFactor.LocalizedName()),
                    isData ? c.DataCount.ToString() : CtrlText(c.ControlCount),
                    IsEvenRow: i % 2 == 0
                )).ToList<SimpleRow>(),

            AnalysisResult.Parallels r =>
                BuildParallelRows(r.Result, isData),

            AnalysisResult.DeclMidpoints r =>
                r.Result.Counts.Select((c, i) => new SimpleRow(
                    _rosetta.GetText(RbFile.Localizable, c.FactorA.LocalizedName()),
                    _rosetta.GetText(RbFile.Localizable, c.FactorB.LocalizedName()),
                    _rosetta.GetText(RbFile.Localizable, c.Occupant.LocalizedName()),
                    isData ? c.DataCount.ToString() : CtrlText(c.ControlCount),
                    IsEvenRow: i % 2 == 0
                )).ToList<SimpleRow>(),

            AnalysisResult.Oob r =>
                r.Result.Counts.Select((c, i) => new SimpleRow(
                    _rosetta.GetText(RbFile.Localizable, c.Factor.LocalizedName()),
                    isData ? c.DataCount.ToString() : CtrlText(c.ControlCount),
                    IsEvenRow: i % 2 == 0
                )).ToList<SimpleRow>(),

            _ => []
        };
    }

    private List<SimpleRow> BuildParallelRows(ParallelsResult result, bool isData)
    {
        var pairs = GetUniqueParallelPairs(result);
        return pairs.Select((pair, i) =>
        {
            var f1    = _rosetta.GetText(RbFile.Localizable, pair.Item1.LocalizedName());
            var f2    = _rosetta.GetText(RbFile.Localizable, pair.Item2.LocalizedName());
            var p     = result.Counts.FirstOrDefault(c => c.Factor1 == pair.Item1 && c.Factor2 == pair.Item2 && !c.IsContraParallel);
            var cp    = result.Counts.FirstOrDefault(c => c.Factor1 == pair.Item1 && c.Factor2 == pair.Item2 && c.IsContraParallel);
            var pRaw  = p  is null ? 0 : (isData ? p.DataCount  : p.ControlCount);
            var cpRaw = cp is null ? 0 : (isData ? cp.DataCount : cp.ControlCount);
            return new SimpleRow(f1, f2,
                isData ? pRaw.ToString()  : CtrlText(pRaw),
                isData ? cpRaw.ToString() : CtrlText(cpRaw),
                isData ? (pRaw + cpRaw).ToString() : CtrlText(pRaw + cpRaw),
                i % 2 == 0);
        }).ToList();
    }

    // ── Info lines ────────────────────────────────────────────────────────────

    private string BuildHarmonicInfoLine()
    {
        if (_result is AnalysisResult.Harmonics h)
            return string.Format(
                _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.harmonic.nr"),
                h.Result.HarmonicNumber);
        return "";
    }

    private string BuildOobInfoLine()
    {
        if (_result is AnalysisResult.Oob o)
            return string.Format(
                _rosetta.GetText(RbFile.ResearchProjects, "view.researchresultscreen.oob.obliquity"),
                o.Result.Obliquity.ToString("F4", System.Globalization.CultureInfo.InvariantCulture));
        return "";
    }

    // ── List totals ───────────────────────────────────────────────────────────

    private string BuildDataListTotal() => _result switch
    {
        AnalysisResult.Unaspect r => r.Result.Counts.Sum(c => c.DataCount).ToString(),
        AnalysisResult.Oob      r => r.Result.Counts.Sum(c => c.DataCount).ToString(),
        _ => ""
    };

    private string BuildControlListTotal() => _result switch
    {
        AnalysisResult.Unaspect r => CtrlText(r.Result.Counts.Sum(c => c.ControlCount)),
        AnalysisResult.Oob      r => CtrlText(r.Result.Counts.Sum(c => c.ControlCount)),
        _ => ""
    };

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string CtrlText(int count)
    {
        var d = Divisor;
        return d > 1
            ? ((double)count / d).ToString("F1", System.Globalization.CultureInfo.CurrentCulture)
            : count.ToString();
    }

    private string CtrlOrDirect(int count, bool isData) =>
        isData ? count.ToString() : CtrlText(count);

    private static List<(Factors, Factors)> GetUniqueAspectPairs(AspectsResult r)
    {
        var seen = new HashSet<string>();
        var list = new List<(Factors, Factors)>();
        foreach (var c in r.Counts)
        {
            var key = $"{(int)c.Factor1}-{(int)c.Factor2}";
            if (seen.Add(key)) list.Add((c.Factor1, c.Factor2));
        }
        return list;
    }

    private static List<(Factors, Factors)> GetUniqueParallelPairs(ParallelsResult r)
    {
        var seen = new HashSet<string>();
        var list = new List<(Factors, Factors)>();
        foreach (var c in r.Counts)
        {
            var key = $"{(int)c.Factor1}-{(int)c.Factor2}";
            if (seen.Add(key)) list.Add((c.Factor1, c.Factor2));
        }
        list.Sort((x, y) =>
        {
            var cmp = ((int)x.Item1).CompareTo((int)y.Item1);
            return cmp != 0 ? cmp : ((int)x.Item2).CompareTo((int)y.Item2);
        });
        return list;
    }

    private static string InquiryKey(InquiriesEnum inquiry) => inquiry switch
    {
        InquiriesEnum.FactorsInSigns  => "enum.inquiry.factorsinsigns",
        InquiriesEnum.FactorsInHouses => "enum.inquiry.factorsinhouses",
        InquiriesEnum.Aspects         => "enum.inquiry.aspects",
        InquiriesEnum.Unaspect        => "enum.inquiry.unaspect",
        InquiriesEnum.Midpoints       => "enum.inquiry.midpoints",
        InquiriesEnum.Harmonics       => "enum.inquiry.harmonics",
        InquiriesEnum.Parallels       => "enum.inquiry.parallels",
        InquiriesEnum.DeclMidpoints   => "enum.inquiry.declmidpoints",
        InquiriesEnum.Oob             => "enum.inquiry.oob",
        _                             => inquiry.ToString()
    };
}
