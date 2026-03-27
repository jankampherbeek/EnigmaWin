// ConfigSectionViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Config.UI;

/// <summary>
/// Placeholder ViewModel for config section editors. Will be replaced by dedicated
/// ViewModels per section in steps 5–7.
/// </summary>
public sealed partial class ConfigSectionViewModel : ObservableObject
{
    private readonly INavigationService _nav;
    private readonly IRosetta _rosetta;

    public string SectionTitle { get; }
    public UserConfiguration? Config { get; }

    public string LabelBackToOverview => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.backtoverview");

    public ConfigSectionViewModel(
        string sectionTitle,
        IConfigContext configContext,
        INavigationService nav,
        IRosetta rosetta)
    {
        SectionTitle = sectionTitle;
        Config = configContext.EditingConfig;
        _nav = nav;
        _rosetta = rosetta;
    }

    internal void GoBack()
    {
        if (_nav.CanGoBackDetail)
            _nav.GoBackDetail();
        else
            _nav.NavigateDetail(AppRoutes.ConfigEdit);
    }
}
