// ImportExportWorkspaceRouteViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Features.ImportExport;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.ViewModels.Routes;

public sealed class ImportExportWorkspaceRouteViewModel
{
    public string Title       { get; }
    public string Description { get; }

    public ImportExportWorkspaceRouteViewModel(IRosetta rosetta)
    {
        Title       = rosetta.GetText(RbFile.ImportExport, ImportExportKeys.Title);
        Description = rosetta.GetText(RbFile.ImportExport, ImportExportKeys.Select);
    }
}
