// ResearchProjectInputViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;
using InquiriesEnum = EnigmaWin.Sources.Features.Research.Inquiries.Inquiries;
using EnigmaWin.Sources.Features.Research.ResearchProjects.Persistency;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Research.UI;

public sealed partial class ResearchProjectInputViewModel : ObservableObject
{
    private readonly IRosetta                    _rosetta;
    private readonly INavigationService          _navigationService;
    private readonly IResearchProjectRepository  _repository;

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelTitle       => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectinputscreen.title");
    public string LabelName        => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectinputscreen.label.name");
    public string LabelDescription => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectinputscreen.label.description");
    public string LabelInquiry     => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectinputscreen.label.inquiry");
    public string LabelCgMult      => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectinputscreen.label.cgmult");
    public string LabelPath        => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectinputscreen.label.path");
    public string LabelSelectPath  => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectinputscreen.button.selectpath");
    public string LabelNext        => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectinputscreen.button.next");
    public string LabelCancel      => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectinputscreen.button.cancel");
    public string TooltipHelp      => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectinputscreen.help.tooltip");

    private string MsgNameEmpty     => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectinputscreen.error.nameempty");
    private string MsgPathEmpty     => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectinputscreen.error.pathempty");
    private string MsgDuplicateName => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectinputscreen.error.duplicatename");

    // ── Inquiry picker items ──────────────────────────────────────────────────

    public IReadOnlyList<InquiryItem> InquiryItems { get; }

    // ── Bindable state ────────────────────────────────────────────────────────

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string description = "";

    [ObservableProperty]
    private InquiryItem? selectedInquiryItem;

    [ObservableProperty]
    private string cgMultiplicationText = "1";

    [ObservableProperty]
    private string selectedPath = "";

    [ObservableProperty]
    private string errorMessage = "";

    public bool CanProceed          => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrEmpty(SelectedPath);
    public bool HasError            => !string.IsNullOrEmpty(ErrorMessage);
    public string SelectedPathDisplay => string.IsNullOrEmpty(SelectedPath) ? "-" : SelectedPath;

    // ── Constructor ──────────────────────────────────────────────────────────

    public ResearchProjectInputViewModel(
        IRosetta rosetta,
        INavigationService navigationService,
        IResearchProjectRepository repository)
    {
        _rosetta           = rosetta;
        _navigationService = navigationService;
        _repository        = repository;

        InquiryItems = System.Enum.GetValues<InquiriesEnum>()
            .Select(i => new InquiryItem(i, InquiryDisplayName(i, rosetta)))
            .ToList();

        SelectedInquiryItem = InquiryItems.FirstOrDefault();
    }

    // ── Actions ──────────────────────────────────────────────────────────────

    public void Cancel() => _navigationService.NavigateMain(AppRoutes.ResearchProjects);

    public async Task ProceedAsync()
    {
        var trimmedName = Name.Trim();
        if (string.IsNullOrEmpty(trimmedName)) { ErrorMessage = MsgNameEmpty; return; }
        if (string.IsNullOrEmpty(SelectedPath)) { ErrorMessage = MsgPathEmpty; return; }

        var existing = await _repository.FetchByNameAsync(trimmedName);
        if (existing is not null) { ErrorMessage = MsgDuplicateName; return; }

        ErrorMessage = "";

        var cgMult = int.TryParse(CgMultiplicationText, out var parsed) ? System.Math.Clamp(parsed, 1, 1000) : 1;
        var inquiry = SelectedInquiryItem?.Value ?? InquiriesEnum.FactorsInSigns;

        var draft = new ResearchProjectDraft(trimmedName, Description, inquiry, cgMult, SelectedPath);
        _navigationService.NavigateMain(AppRoutes.ResearchProjectConfig,
            new ResearchProjectConfigNavigationParameter(draft));
    }

    partial void OnNameChanged(string value)         => OnPropertyChanged(nameof(CanProceed));
    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));
    partial void OnSelectedPathChanged(string value)
    {
        OnPropertyChanged(nameof(CanProceed));
        OnPropertyChanged(nameof(SelectedPathDisplay));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string InquiryDisplayName(InquiriesEnum inquiry, IRosetta rosetta) =>
        inquiry switch
        {
            InquiriesEnum.FactorsInSigns   => rosetta.GetText(RbFile.Localizable, "enum.inquiry.factorsinsigns"),
            InquiriesEnum.FactorsInHouses  => rosetta.GetText(RbFile.Localizable, "enum.inquiry.factorsinhouses"),
            InquiriesEnum.Aspects          => rosetta.GetText(RbFile.Localizable, "enum.inquiry.aspects"),
            InquiriesEnum.Unaspect         => rosetta.GetText(RbFile.Localizable, "enum.inquiry.unaspect"),
            InquiriesEnum.Midpoints        => rosetta.GetText(RbFile.Localizable, "enum.inquiry.midpoints"),
            InquiriesEnum.Harmonics        => rosetta.GetText(RbFile.Localizable, "enum.inquiry.harmonics"),
            InquiriesEnum.Parallels        => rosetta.GetText(RbFile.Localizable, "enum.inquiry.parallels"),
            InquiriesEnum.DeclMidpoints    => rosetta.GetText(RbFile.Localizable, "enum.inquiry.declmidpoints"),
            InquiriesEnum.Oob              => rosetta.GetText(RbFile.Localizable, "enum.inquiry.oob"),
            _                          => inquiry.ToString()
        };
}

/// <summary>Display item for the inquiry ComboBox.</summary>
public sealed record InquiryItem(InquiriesEnum Value, string DisplayName)
{
    public override string ToString() => DisplayName;
}

/// <summary>Data passed from the input screen to the config screen.</summary>
public sealed record ResearchProjectDraft(
    string Name,
    string Description,
    InquiriesEnum Inquiry,
    int CgMultiplication,
    string Path);
