// ResearchProjectConfigViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Research.Pipeline;
using EnigmaWin.Sources.Features.Research.ResearchProjects.Persistency;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using InquiriesEnum = EnigmaWin.Sources.Features.Research.Inquiries.Inquiries;
using AppOrbConfig = EnigmaWin.Sources.Features.Config.OrbConfig;

namespace EnigmaWin.Sources.Features.Research.UI;

public sealed partial class ResearchProjectConfigViewModel : ObservableObject
{
    private readonly IRosetta                   _rosetta;
    private readonly INavigationService         _navigationService;
    private readonly IResearchProjectRepository _repository;
    private readonly IConfigContext             _configContext;
    private readonly ResearchProjectDraft       _draft;

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelTitle          => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.title");
    public string LabelSectionFactors => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.section.factors");
    public string LabelSectionAspects => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.section.aspects");
    public string LabelSectionHouse   => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.section.housesystem");
    public string LabelSectionOrb     => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.section.orb");
    public string LabelSectionDial    => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.section.dial");
    public string LabelSectionHarmon  => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.section.harmonics");
    public string LabelOrbFromConfig  => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.orb.fromconfig");
    public string LabelOrbOverride    => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.orb.override");
    public string LabelDial360        => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.dial.360");
    public string LabelDial90         => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.dial.90");
    public string LabelDial45         => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.dial.45");
    public string LabelHarmonicNumber => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.harmonic.number");
    public string LabelSave           => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.button.save");
    public string LabelCancel         => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.button.cancel");
    public string TooltipHelp         => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.help.tooltip");

    private string MsgHarmonicNumberError => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.harmonic.numbererror");
    private string MsgNoFactors           => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.error.nofactors");
    private string MsgNoAspects           => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.error.noaspects");
    private string MsgNoDialSelected      => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectconfigscreen.error.nodial");

    // ── Inquiry type (drives which sections are visible) ─────────────────────

    public InquiriesEnum Inquiry => _draft.Inquiry;

    public bool ShowHouseSystem  => Inquiry == InquiriesEnum.FactorsInHouses;
    public bool ShowAspects      => Inquiry == InquiriesEnum.Aspects;
    public bool ShowAspectOrb    => Inquiry == InquiriesEnum.Aspects;
    public bool ShowUnaspectOrb  => Inquiry == InquiriesEnum.Unaspect;
    public bool ShowDials        => Inquiry == InquiriesEnum.Midpoints;
    public bool ShowMidpointOrbs => Inquiry == InquiriesEnum.Midpoints;
    public bool ShowHarmonicNum  => Inquiry == InquiriesEnum.Harmonics;
    public bool ShowHarmonicOrb  => Inquiry == InquiriesEnum.Harmonics;
    public bool ShowParallelOrb  => Inquiry == InquiriesEnum.Parallels;
    public bool ShowDeclMidOrb   => Inquiry == InquiriesEnum.DeclMidpoints;

    // ── Factor checkboxes ─────────────────────────────────────────────────────

    public IReadOnlyList<FactorItem> FactorItems { get; }

    // ── House system picker ───────────────────────────────────────────────────

    public IReadOnlyList<HouseSystemItem> HouseSystemItems { get; }

    [ObservableProperty]
    private HouseSystemItem? selectedHouseSystemItem;

    // ── Aspect checkboxes ─────────────────────────────────────────────────────

    public IReadOnlyList<AspectItem> AspectItems { get; }

    // ── Aspect orb override ───────────────────────────────────────────────────

    public string OrbFromConfigAspect { get; private set; } = "";

    [ObservableProperty]
    private bool overrideAspectOrb;

    [ObservableProperty]
    private int aspectOrbDeg;

    [ObservableProperty]
    private int aspectOrbMin;

    // ── Unaspect orb override ─────────────────────────────────────────────────

    public string OrbFromConfigUnaspect { get; private set; } = "";

    [ObservableProperty]
    private bool overrideUnaspectOrb;

    [ObservableProperty]
    private int unaspectOrbDeg;

    [ObservableProperty]
    private int unaspectOrbMin;

    // ── Harmonic number ───────────────────────────────────────────────────────

    [ObservableProperty]
    private string harmonicNumberText = "5";

    [ObservableProperty]
    private string harmonicNumberError = "";

    public bool HasHarmonicNumberError => !string.IsNullOrEmpty(HarmonicNumberError);

    // ── Harmonic orb override ─────────────────────────────────────────────────

    public string OrbFromConfigHarmonic { get; private set; } = "";

    [ObservableProperty]
    private bool overrideHarmonicOrb;

    [ObservableProperty]
    private int harmonicOrbDeg;

    [ObservableProperty]
    private int harmonicOrbMin;

    // ── Parallel orb override ─────────────────────────────────────────────────

    public string OrbFromConfigParallel { get; private set; } = "";

    [ObservableProperty]
    private bool overrideParallelOrb;

    [ObservableProperty]
    private int parallelOrbDeg;

    [ObservableProperty]
    private int parallelOrbMin;

    // ── DeclMidpoint orb override ─────────────────────────────────────────────

    public string OrbFromConfigDeclMid { get; private set; } = "";

    [ObservableProperty]
    private bool overrideDeclMidpointOrb;

    [ObservableProperty]
    private int declMidpointOrbDeg;

    [ObservableProperty]
    private int declMidpointOrbMin;

    // ── Dial toggles ──────────────────────────────────────────────────────────

    [ObservableProperty]
    private bool useDial360 = true;

    [ObservableProperty]
    private bool useDial90 = true;

    [ObservableProperty]
    private bool useDial45;

    // ── Midpoint orbs per dial ────────────────────────────────────────────────

    [ObservableProperty]
    private int orbMidpoint360Deg = 1;

    [ObservableProperty]
    private int orbMidpoint360Min = 30;

    [ObservableProperty]
    private int orbMidpoint90Deg = 1;

    [ObservableProperty]
    private int orbMidpoint90Min;

    [ObservableProperty]
    private int orbMidpoint45Deg;

    [ObservableProperty]
    private int orbMidpoint45Min = 30;

    // ── Error / status ────────────────────────────────────────────────────────

    [ObservableProperty]
    private string errorMessage = "";

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool CanSave => FactorItems.Any(f => f.IsSelected);

    // ── Constructor ──────────────────────────────────────────────────────────

    public ResearchProjectConfigViewModel(
        IRosetta rosetta,
        INavigationService navigationService,
        IResearchProjectRepository repository,
        IConfigContext configContext,
        ResearchProjectDraft draft)
    {
        _rosetta           = rosetta;
        _navigationService = navigationService;
        _repository        = repository;
        _configContext     = configContext;
        _draft             = draft;

        FactorItems = FactorsExtensions.SelectableFactors
            .Select(f => new FactorItem(f, FactorDisplayName(f, rosetta)))
            .ToList();

        HouseSystemItems = Enum.GetValues<HouseSystems>()
            .Where(h => h != HouseSystems.NoHouses)
            .Select(h => new HouseSystemItem(h, HouseSystemDisplayName(h, rosetta)))
            .ToList();

        AspectItems = Enum.GetValues<Aspects>()
            .Select(a => new AspectItem(a, AspectDisplayName(a, rosetta)))
            .ToList();

        SelectedHouseSystemItem = HouseSystemItems.FirstOrDefault(h => h.Value == HouseSystems.Placidus)
                                  ?? HouseSystemItems.FirstOrDefault();

        InitializeFromActiveConfig();
    }

    // ── Actions ──────────────────────────────────────────────────────────────

    public void Cancel() => _navigationService.NavigateMain(AppRoutes.ResearchProjectInput);

    public async Task SaveAsync()
    {
        ErrorMessage = "";

        var selectedFactorIds = FactorItems.Where(f => f.IsSelected).Select(f => (int)f.Value).OrderBy(id => id).ToList();
        if (selectedFactorIds.Count == 0) { ErrorMessage = MsgNoFactors; return; }

        if (Inquiry == InquiriesEnum.Aspects)
        {
            var selectedAspectIds = AspectItems.Where(a => a.IsSelected).Select(a => (int)a.Value).ToList();
            if (selectedAspectIds.Count == 0) { ErrorMessage = MsgNoAspects; return; }
        }

        if (Inquiry == InquiriesEnum.Midpoints && !UseDial360 && !UseDial90 && !UseDial45)
        {
            ErrorMessage = MsgNoDialSelected;
            return;
        }

        int? resolvedHarmonicNumber = null;
        if (Inquiry == InquiriesEnum.Harmonics)
        {
            HarmonicNumberError = "";
            var parsed = int.TryParse(HarmonicNumberText.Trim(), out var n) ? n : 0;
            if (parsed < 2) { HarmonicNumberError = MsgHarmonicNumberError; return; }
            resolvedHarmonicNumber = parsed;
        }

        var activeConfig = _configContext.ActiveConfig;
        var calcConfig   = activeConfig.CalculationConfig;
        var orbConfig    = activeConfig.OrbConfig;

        var calculationConfigJson = JsonSerializer.Serialize(calcConfig);

        var researchOrbConfig = BuildResearchOrbConfig(orbConfig);
        var orbConfigJson = JsonSerializer.Serialize(researchOrbConfig);

        var houseSystemId = SelectedHouseSystemItem?.Value is HouseSystems hs
            ? (int)hs
            : (int)HouseSystems.Placidus;

        List<int>? enabledAspectIds = null;
        if (Inquiry == InquiriesEnum.Aspects)
            enabledAspectIds = AspectItems.Where(a => a.IsSelected).Select(a => (int)a.Value).OrderBy(id => id).ToList();

        double? aspectOrbOverride        = OverrideAspectOrb && Inquiry == InquiriesEnum.Aspects
            ? ToDecimalDegrees(AspectOrbDeg, AspectOrbMin) : null;
        double? unaspectOrbOverride      = OverrideUnaspectOrb && Inquiry == InquiriesEnum.Unaspect
            ? ToDecimalDegrees(UnaspectOrbDeg, UnaspectOrbMin) : null;
        double? declMidpointOrbOverride  = OverrideDeclMidpointOrb && Inquiry == InquiriesEnum.DeclMidpoints
            ? ToDecimalDegrees(DeclMidpointOrbDeg, DeclMidpointOrbMin) : null;
        double? harmonicOrbOverride      = OverrideHarmonicOrb && Inquiry == InquiriesEnum.Harmonics
            ? ToDecimalDegrees(HarmonicOrbDeg, HarmonicOrbMin) : null;
        double? parallelOrbOverride      = OverrideParallelOrb && Inquiry == InquiriesEnum.Parallels
            ? ToDecimalDegrees(ParallelOrbDeg, ParallelOrbMin) : null;

        List<int>? enabledDialSizes = null;
        if (Inquiry == InquiriesEnum.Midpoints)
        {
            var sizes = new List<int>();
            if (UseDial360) sizes.Add(360);
            if (UseDial90)  sizes.Add(90);
            if (UseDial45)  sizes.Add(45);
            enabledDialSizes = sizes;
        }

        var researchConfig = new ResearchConfig
        {
            EnabledFactorIds        = selectedFactorIds,
            InquiryId               = (int)Inquiry,
            HouseSystemId           = houseSystemId,
            CalculationConfigJson   = calculationConfigJson,
            OrbConfigJson           = orbConfigJson,
            EnabledAspectIds        = enabledAspectIds,
            AspectOrbOverride       = aspectOrbOverride,
            UnaspectOrbOverride     = unaspectOrbOverride,
            DeclMidpointOrbOverride = declMidpointOrbOverride,
            HarmonicOrbOverride     = harmonicOrbOverride,
            ParallelOrbOverride     = parallelOrbOverride,
            EnabledDialSizes        = enabledDialSizes,
            HarmonicNumber          = resolvedHarmonicNumber
        };

        var projectFolder = Path.Combine(_draft.Path, _draft.Name);
        Directory.CreateDirectory(projectFolder);

        var project = new ResearchProject
        {
            Name              = _draft.Name,
            Description       = _draft.Description,
            Inquiry           = Inquiry,
            Config            = researchConfig.ToJson(),
            CgMultiplication  = _draft.CgMultiplication,
            Path              = projectFolder,
            CreationDate      = DateTime.UtcNow
        };

        await _repository.AddAsync(project);
        var saved = await _repository.FetchByNameAsync(project.Name);
        _navigationService.NavigateMain(AppRoutes.ResearchProjectOpen,
            new ResearchProjectOpenNavigationParameter(saved?.Id ?? 0, project.Name));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    partial void OnErrorMessageChanged(string value)        => OnPropertyChanged(nameof(HasError));
    partial void OnHarmonicNumberErrorChanged(string value) => OnPropertyChanged(nameof(HasHarmonicNumberError));
    partial void OnUseDial360Changed(bool value)       => OnPropertyChanged(nameof(CanSave));
    partial void OnUseDial90Changed(bool value)        => OnPropertyChanged(nameof(CanSave));
    partial void OnUseDial45Changed(bool value)        => OnPropertyChanged(nameof(CanSave));

    public void NotifyFactorSelectionChanged() => OnPropertyChanged(nameof(CanSave));

    private void InitializeFromActiveConfig()
    {
        var config   = _configContext.ActiveConfig;
        var factors  = config.FactorConfig;
        var aspects  = config.AspectConfig;
        var orbs     = config.OrbConfig;
        var calcConf = config.CalculationConfig;

        foreach (var item in FactorItems)
        {
            var setting = factors.Settings.FirstOrDefault(s => s.Factor == item.Value);
            item.IsSelected = setting.IsUsed;
        }

        SelectedHouseSystemItem = HouseSystemItems.FirstOrDefault(h => h.Value == calcConf.HouseSystem)
                                  ?? SelectedHouseSystemItem;

        if (Inquiry == InquiriesEnum.Aspects)
        {
            foreach (var item in AspectItems)
            {
                var setting = aspects.Settings.FirstOrDefault(s => s.Aspect == item.Value);
                item.IsSelected = setting.IsUsed;
            }
            FromDecimalDegrees(orbs.AspectBaseOrb, out var d, out var m);
            AspectOrbDeg = d; AspectOrbMin = m;
            OrbFromConfigAspect = FormatOrbFromConfig(orbs.AspectBaseOrb);
        }

        if (Inquiry == InquiriesEnum.Unaspect)
        {
            FromDecimalDegrees(orbs.AspectBaseOrb, out var d, out var m);
            UnaspectOrbDeg = d; UnaspectOrbMin = m;
            OrbFromConfigUnaspect = FormatOrbFromConfig(orbs.AspectBaseOrb);
        }

        if (Inquiry == InquiriesEnum.Midpoints)
        {
            FromDecimalDegrees(orbs.Midpoint360DialOrb, out var d360, out var m360);
            OrbMidpoint360Deg = d360; OrbMidpoint360Min = m360;
            FromDecimalDegrees(orbs.Midpoint90DialOrb, out var d90, out var m90);
            OrbMidpoint90Deg  = d90;  OrbMidpoint90Min  = m90;
            FromDecimalDegrees(orbs.Midpoint45DialOrb, out var d45, out var m45);
            OrbMidpoint45Deg  = d45;  OrbMidpoint45Min  = m45;
        }

        if (Inquiry == InquiriesEnum.Harmonics)
        {
            FromDecimalDegrees(orbs.HarmonicOrb, out var d, out var m);
            HarmonicOrbDeg = d; HarmonicOrbMin = m;
            OrbFromConfigHarmonic = FormatOrbFromConfig(orbs.HarmonicOrb);
        }

        if (Inquiry == InquiriesEnum.Parallels)
        {
            FromDecimalDegrees(orbs.ParallelOrb, out var d, out var m);
            ParallelOrbDeg = d; ParallelOrbMin = m;
            OrbFromConfigParallel = FormatOrbFromConfig(orbs.ParallelOrb);
        }

        if (Inquiry == InquiriesEnum.DeclMidpoints)
        {
            FromDecimalDegrees(orbs.DeclinationMidpointOrb, out var d, out var m);
            DeclMidpointOrbDeg = d; DeclMidpointOrbMin = m;
            OrbFromConfigDeclMid = FormatOrbFromConfig(orbs.DeclinationMidpointOrb);
        }
    }

    private Pipeline.OrbConfig BuildResearchOrbConfig(AppOrbConfig source)
    {
        var orb360 = Inquiry == InquiriesEnum.Midpoints
            ? ToDecimalDegrees(OrbMidpoint360Deg, OrbMidpoint360Min)
            : source.Midpoint360DialOrb;
        var orb90 = Inquiry == InquiriesEnum.Midpoints
            ? ToDecimalDegrees(OrbMidpoint90Deg, OrbMidpoint90Min)
            : source.Midpoint90DialOrb;
        var orb45 = Inquiry == InquiriesEnum.Midpoints
            ? ToDecimalDegrees(OrbMidpoint45Deg, OrbMidpoint45Min)
            : source.Midpoint45DialOrb;

        return new Pipeline.OrbConfig
        {
            AspectBaseOrb          = source.AspectBaseOrb,
            Midpoint360DialOrb     = orb360,
            Midpoint90DialOrb      = orb90,
            Midpoint45DialOrb      = orb45,
            HarmonicOrb            = source.HarmonicOrb,
            ParallelOrb            = source.ParallelOrb,
            DeclinationMidpointOrb = source.DeclinationMidpointOrb
        };
    }

    private static double ToDecimalDegrees(int degrees, int minutes) =>
        degrees + minutes / 60.0;

    private static void FromDecimalDegrees(double value, out int degrees, out int minutes)
    {
        degrees = (int)value;
        minutes = (int)Math.Round((value - degrees) * 60.0);
        if (minutes >= 60) { degrees++; minutes = 0; }
    }

    private static string FormatOrbFromConfig(double value)
    {
        FromDecimalDegrees(value, out var d, out var m);
        return $"{d}° {m}'";
    }

    private static string FactorDisplayName(Factors factor, IRosetta rosetta) =>
        rosetta.GetText(RbFile.Localizable, factor.LocalizedName());

    private static string HouseSystemDisplayName(HouseSystems system, IRosetta rosetta) =>
        rosetta.GetText(RbFile.Localizable, system.LocalizedName());

    private static string AspectDisplayName(Aspects aspect, IRosetta rosetta) =>
        rosetta.GetText(RbFile.Localizable, aspect.LocalizedName());
}

/// <summary>Checkbox item representing a celestial factor.</summary>
public sealed class FactorItem(Factors value, string displayName) : ObservableObject
{
    public Factors Value       { get; } = value;
    public string  DisplayName { get; } = displayName;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public override string ToString() => DisplayName;
}

/// <summary>Picker item for a house system.</summary>
public sealed record HouseSystemItem(HouseSystems Value, string DisplayName)
{
    public override string ToString() => DisplayName;
}

/// <summary>Checkbox item representing an aspect.</summary>
public sealed class AspectItem(Aspects value, string displayName) : ObservableObject
{
    public Aspects Value       { get; } = value;
    public string  DisplayName { get; } = displayName;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public override string ToString() => DisplayName;
}
