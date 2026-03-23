// ConfigProgressionsSectionViewModel.cs
// EnigmaWin
// Created by Jan Kampherbeek on 23-03-2026

using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Config.UI;

public sealed partial class ConfigProgressionsSectionViewModel : ObservableObject
{
    private readonly INavigationService _nav;
    private readonly IConfigContext     _configContext;
    private readonly IRosetta           _rosetta;

    // ── Labels ───────────────────────────────────────────────────────────────

    public string SectionTitle        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.section.progressions");
    public string LabelBackToOverview => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.backtoverview");
    public string LabelPrimary        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.nav.primary");
    public string LabelTransits       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.nav.transits");
    public string LabelSecondary      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.nav.secondary");
    public string LabelSymbolic       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.nav.symbolic");
    public string LabelSolar          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.prog.nav.solar");

    // ── Constructor ──────────────────────────────────────────────────────────

    public ConfigProgressionsSectionViewModel(
        INavigationService nav,
        IConfigContext     configContext,
        IRosetta           rosetta)
    {
        _nav           = nav;
        _configContext = configContext;
        _rosetta       = rosetta;
    }

    // ── Navigation ───────────────────────────────────────────────────────────

    internal void GoBack()
    {
        if (_nav.CanGoBackDetail)
            _nav.GoBackDetail();
        else
            _nav.NavigateDetail(AppRoutes.ConfigEdit);
    }

    internal void GoToPrimary()    => _nav.NavigateDetail(AppRoutes.ConfigSectionProgPrimary);
    internal void GoToTransits()   => _nav.NavigateDetail(AppRoutes.ConfigSectionProgTransits);
    internal void GoToSecondary()  => _nav.NavigateDetail(AppRoutes.ConfigSectionProgSecondary);
    internal void GoToSymbolic()   => _nav.NavigateDetail(AppRoutes.ConfigSectionProgSymbolic);
    internal void GoToSolar()      => _nav.NavigateDetail(AppRoutes.ConfigSectionProgSolar);
}
