// ResearchProjectWorkViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Research.Inquiries;
using EnigmaWin.Sources.Features.Research.Pipeline;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using InquiriesEnum = EnigmaWin.Sources.Features.Research.Inquiries.Inquiries;

namespace EnigmaWin.Sources.Features.Research.UI;

public sealed partial class ResearchProjectWorkViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IRosetta           _rosetta;

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelTitle           => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.title");
    public string LabelSectionInfo     => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.section.info");
    public string LabelSectionData     => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.section.data");
    public string LabelName            => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.label.name");
    public string LabelDescription     => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.label.description");
    public string LabelInquiry         => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.label.inquiry");
    public string LabelFactors         => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.label.factors");
    public string LabelHouseSystem     => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.label.housesystem");
    public string LabelOrb             => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.label.orb");
    public string LabelDials           => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.label.dials");
    public string LabelHarmonicNr      => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.label.harmonicnr");
    public string LabelCgMult          => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.label.cgmult");
    public string LabelPath            => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.label.path");
    public string LabelCreated         => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.label.created");
    public string LabelFileType        => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.label.filetype");
    public string LabelDataFile        => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.label.datafile");
    public string LabelSelectFile      => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.button.selectfile");
    public string LabelStart           => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.button.start");
    public string LabelBack            => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.button.back");
    public string TooltipHelp          => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectworkscreen.help.tooltip");

    // ── Project info (read-only display) ─────────────────────────────────────

    public string ProjectName        { get; }
    public string ProjectDescription { get; }
    public string InquiryDisplay     { get; }
    public string FactorsDisplay     { get; }
    public string CgMultDisplay      { get; }
    public string PathDisplay        { get; }
    public string CreatedDisplay     { get; }

    // inquiry-specific info rows
    public bool   ShowHouseSystem  { get; }
    public string HouseSystemDisplay { get; } = "";
    public bool   ShowOrb          { get; }
    public string OrbDisplay       { get; } = "";
    public bool   ShowDials        { get; }
    public string DialsDisplay     { get; } = "";
    public bool   ShowHarmonicNr   { get; }
    public string HarmonicNrDisplay { get; } = "";

    // ── File selection ────────────────────────────────────────────────────────

    public IReadOnlyList<DataFileTypeItem> FileTypeItems { get; }

    [ObservableProperty]
    private DataFileTypeItem? selectedFileTypeItem;

    [ObservableProperty]
    private string selectedFilePath = "";

    [ObservableProperty]
    private string fileStatusMessage = "";

    [ObservableProperty]
    private bool fileStatusIsError;

    public bool HasFileStatus   => !string.IsNullOrEmpty(FileStatusMessage);
    public bool HasFileStatusOk => HasFileStatus && !FileStatusIsError;
    public bool CanStart      => !string.IsNullOrEmpty(SelectedFilePath);
    public string SelectedFileDisplay => string.IsNullOrEmpty(SelectedFilePath) ? "-" : SelectedFilePath;

    // ── Constructor ──────────────────────────────────────────────────────────

    public ResearchProjectWorkViewModel(
        IRosetta rosetta,
        INavigationService navigationService,
        ResearchProject project)
    {
        _rosetta           = rosetta;
        _navigationService = navigationService;

        ProjectName        = project.Name;
        ProjectDescription = string.IsNullOrEmpty(project.Description) ? "-" : project.Description;
        CgMultDisplay      = project.CgMultiplication.ToString();
        PathDisplay        = project.Path;
        CreatedDisplay     = project.CreationDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

        InquiryDisplay = rosetta.GetText(RbFile.Localizable, InquiryKey(project.Inquiry));

        ResearchConfig? config = null;
        try { config = ResearchConfig.FromJson(project.Config); } catch { }

        if (config is not null)
        {
            FactorsDisplay = string.Join(", ", config.EnabledFactors
                .Select(f => rosetta.GetText(RbFile.Localizable, f.LocalizedName())));

            switch (project.Inquiry)
            {
                case InquiriesEnum.FactorsInHouses:
                    ShowHouseSystem  = true;
                    HouseSystemDisplay = rosetta.GetText(RbFile.Localizable, config.HouseSystem.LocalizedName());
                    break;

                case InquiriesEnum.Aspects:
                    ShowOrb   = true;
                    OrbDisplay = FormatOrb(config.AspectOrbOverride ?? config.OrbConfig?.AspectBaseOrb);
                    break;

                case InquiriesEnum.Unaspect:
                    ShowOrb   = true;
                    OrbDisplay = FormatOrb(config.UnaspectOrbOverride ?? config.OrbConfig?.AspectBaseOrb);
                    break;

                case InquiriesEnum.Midpoints:
                    ShowDials  = true;
                    ShowOrb    = true;
                    DialsDisplay = config.EnabledDialSizes is { } dials
                        ? string.Join(", ", dials.Select(d => $"{d}°"))
                        : "";
                    OrbDisplay = FormatMidpointOrbs(config);
                    break;

                case InquiriesEnum.Harmonics:
                    ShowHarmonicNr  = true;
                    ShowOrb         = true;
                    HarmonicNrDisplay = (config.HarmonicNumber ?? 5).ToString();
                    OrbDisplay = FormatOrb(config.HarmonicOrbOverride ?? config.OrbConfig?.HarmonicOrb);
                    break;

                case InquiriesEnum.Parallels:
                    ShowOrb   = true;
                    OrbDisplay = FormatOrb(config.ParallelOrbOverride ?? config.OrbConfig?.ParallelOrb);
                    break;

                case InquiriesEnum.DeclMidpoints:
                    ShowOrb   = true;
                    OrbDisplay = FormatOrb(config.DeclMidpointOrbOverride ?? config.OrbConfig?.DeclinationMidpointOrb);
                    break;
            }
        }
        else
        {
            FactorsDisplay = "";
        }

        FileTypeItems = new List<DataFileTypeItem>
        {
            new(DataFileType.StandardEnigma, rosetta.GetText(RbFile.ResearchProjects, "enum.datafiletype.standardenigma")),
            new(DataFileType.Gauquelin,      rosetta.GetText(RbFile.ResearchProjects, "enum.datafiletype.gauquelin")),
            new(DataFileType.QuickChart,     rosetta.GetText(RbFile.ResearchProjects, "enum.datafiletype.quickchart"))
        };
        SelectedFileTypeItem = FileTypeItems[0];
    }

    // ── Actions ──────────────────────────────────────────────────────────────

    public void Back() => _navigationService.NavigateMain(AppRoutes.ResearchProjectListAll);

    // ── Notification helpers ──────────────────────────────────────────────────

    partial void OnSelectedFilePathChanged(string value)
    {
        OnPropertyChanged(nameof(CanStart));
        OnPropertyChanged(nameof(SelectedFileDisplay));
    }

    partial void OnFileStatusMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasFileStatus));
        OnPropertyChanged(nameof(HasFileStatusOk));
    }

    // ── Private helpers ───────────────────────────────────────────────────────

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

    private static string FormatOrb(double? value)
    {
        if (value is null) return "-";
        var deg = (int)value.Value;
        var min = (int)Math.Round((value.Value - deg) * 60.0);
        if (min >= 60) { deg++; min = 0; }
        return $"{deg}° {min}'";
    }

    private static string FormatMidpointOrbs(ResearchConfig config)
    {
        if (config.EnabledDialSizes is null || config.OrbConfig is null) return "-";
        var parts = config.EnabledDialSizes.Select(size => size switch
        {
            360 => $"360°: {FormatOrb(config.OrbConfig.Midpoint360DialOrb)}",
            90  => $"90°: {FormatOrb(config.OrbConfig.Midpoint90DialOrb)}",
            45  => $"45°: {FormatOrb(config.OrbConfig.Midpoint45DialOrb)}",
            _   => ""
        }).Where(s => s.Length > 0);
        return string.Join("  ", parts);
    }
}

public sealed record DataFileTypeItem(DataFileType Value, string DisplayName)
{
    public override string ToString() => DisplayName;
}
