// ResearchProjectsListViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Features.Research.ResearchProjects.Persistency;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Research.UI;

public sealed partial class ResearchProjectsListViewModel : ObservableObject
{
    private readonly IRosetta                   _rosetta;
    private readonly INavigationService         _navigationService;
    private readonly IResearchProjectRepository _repository;
    private readonly ResearchProjectListMode    _mode;

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelTitle       => _mode == ResearchProjectListMode.All
        ? _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.title.all")
        : _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.title.search");
    public string LabelColumnName  => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.column.name");
    public string LabelColumnDesc  => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.column.description");
    public string LabelColumnDate  => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.column.date");
    public string LabelSearchLabel => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.search.label");
    public string LabelSearchBtn   => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.search.button");
    public string LabelCancel      => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.button.cancel");
    public string LabelOpen        => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.button.open");
    public string LabelDelete      => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.button.delete");
    public string LabelDeleteTitle => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.delete.title");
    public string LabelDeleteConfirm => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.delete.confirm");
    public string LabelDeleteCancel  => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.delete.cancel");
    public string TooltipHelp      => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.help.tooltip");

    private string MsgDeleteFailed  => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.delete.failed");
    private string MsgNoResults     => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.search.noresults");
    private string MsgDeleteMessage => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.delete.message");

    // ── Mode / visibility ─────────────────────────────────────────────────────

    public ResearchProjectListMode Mode        => _mode;
    public bool                    IsSearchMode => _mode == ResearchProjectListMode.Search;

    // ── Bindable state ────────────────────────────────────────────────────────

    [ObservableProperty]
    private string searchQuery = "";

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    private string noResultsMessage = "";

    public bool HasError     => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasNoResults => !string.IsNullOrEmpty(NoResultsMessage);
    public bool CanSearch    => !string.IsNullOrWhiteSpace(SearchQuery);

    public ObservableCollection<ProjectListItem> Projects { get; } = [];

    // ── Constructor ──────────────────────────────────────────────────────────

    public ResearchProjectsListViewModel(
        IRosetta rosetta,
        INavigationService navigationService,
        IResearchProjectRepository repository,
        ResearchProjectListMode mode)
    {
        _rosetta           = rosetta;
        _navigationService = navigationService;
        _repository        = repository;
        _mode              = mode;
    }

    // ── Initialise (called after construction) ────────────────────────────────

    public async Task LoadAsync()
    {
        if (_mode == ResearchProjectListMode.All)
            await LoadAllAsync();
    }

    // ── Actions ──────────────────────────────────────────────────────────────

    public async Task SearchAsync()
    {
        var term = SearchQuery.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(term)) return;

        NoResultsMessage = "";
        Projects.Clear();

        var all = await _repository.FetchAllAsync();
        var matches = all.Where(p => p.Name.ToLowerInvariant().Contains(term)).ToList();

        foreach (var p in matches)
            Projects.Add(ToListItem(p));

        if (Projects.Count == 0)
            NoResultsMessage = MsgNoResults;
    }

    public async Task ConfirmDeleteAsync(ProjectListItem item)
    {
        ErrorMessage = "";

        try
        {
            await _repository.DeleteAsync(item.Id);
            Projects.Remove(item);
        }
        catch
        {
            ErrorMessage = MsgDeleteFailed;
        }
    }

    public void OpenProject(ProjectListItem item) =>
        _navigationService.NavigateMain(AppRoutes.ResearchProjectOpen,
            new ResearchProjectOpenNavigationParameter(item.Id, item.Name));

    public void Cancel() => _navigationService.NavigateMain(AppRoutes.ResearchProjects);

    // ── Notification helpers ──────────────────────────────────────────────────

    partial void OnErrorMessageChanged(string value)     => OnPropertyChanged(nameof(HasError));
    partial void OnNoResultsMessageChanged(string value) => OnPropertyChanged(nameof(HasNoResults));
    partial void OnSearchQueryChanged(string value)      => OnPropertyChanged(nameof(CanSearch));

    public string DeleteConfirmMessage(string name) =>
        string.Format(MsgDeleteMessage, name);

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task LoadAllAsync()
    {
        Projects.Clear();
        var all = await _repository.FetchAllAsync();
        foreach (var p in all)
            Projects.Add(ToListItem(p));
    }

    private ProjectListItem ToListItem(Domain.ResearchProject p) =>
        new(p.Id, p.Name, p.Description, p.CreationDate, LabelOpen, LabelDelete);
}

/// <summary>Row item for the projects list.</summary>
public sealed record ProjectListItem(int Id, string Name, string Description, DateTime CreationDate, string LabelOpen, string LabelDelete)
{
    public string FormattedDate => CreationDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
}
