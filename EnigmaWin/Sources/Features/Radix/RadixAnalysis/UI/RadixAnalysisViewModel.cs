// RadixAnalysisViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.UI;

public sealed class RadixAnalysisViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IRosetta _rosetta;

    public IRelayCommand ShowAspectsCommand          { get; }
    public IRelayCommand ShowBlaSchemaCommand        { get; }
    public IRelayCommand ShowMidpointsCommand        { get; }
    public IRelayCommand ShowHarmonicsCommand        { get; }
    public IRelayCommand ShowHarmonicOrbsCommand     { get; }
    public IRelayCommand ShowDeclinationsCommand     { get; }
    public IRelayCommand ShowZodiacDivisionsCommand  { get; }
    public IRelayCommand ShowEnneagramCommand        { get; }
    public IRelayCommand ShowVspCommand              { get; }
    public IRelayCommand ShowFixStarsCommand         { get; }
    public IRelayCommand ShowParansCommand           { get; }
    public IRelayCommand ShowCountingsCommand        { get; }

    public RadixAnalysisViewModel(INavigationService navigationService, IRosetta rosetta)
    {
        _navigationService = navigationService;
        _rosetta           = rosetta;
        ShowAspectsCommand         = new RelayCommand(OpenAspects);
        ShowBlaSchemaCommand       = new RelayCommand(OpenBlaSchema);
        ShowMidpointsCommand       = new RelayCommand(OpenMidpoints);
        ShowHarmonicsCommand       = new RelayCommand(OpenHarmonics);
        ShowHarmonicOrbsCommand    = new RelayCommand(OpenHarmonicOrbs);
        ShowDeclinationsCommand    = new RelayCommand(OpenDeclinations);
        ShowZodiacDivisionsCommand = new RelayCommand(OpenZodiacDivisions);
        ShowEnneagramCommand       = new RelayCommand(OpenEnneagram);
        ShowVspCommand             = new RelayCommand(OpenVsp);
        ShowFixStarsCommand        = new RelayCommand(OpenFixStars);
        ShowParansCommand          = new RelayCommand(OpenParans);
        ShowCountingsCommand       = new RelayCommand(OpenCountings);
    }

    public string LabelTitle              => _rosetta.GetText(RbFile.RadixAnalysis, "analysis.title");
    public string LabelBtnAspects         => _rosetta.GetText(RbFile.RadixAnalysis, "analysis.btn.aspects");
    public string LabelBtnBlaSchema       => _rosetta.GetText(RbFile.RadixAnalysis, "analysis.btn.blaschema");
    public string LabelBtnMidpoints       => _rosetta.GetText(RbFile.RadixAnalysis, "analysis.btn.midpoints");
    public string LabelBtnHarmonics       => _rosetta.GetText(RbFile.RadixAnalysis, "analysis.btn.harmonics");
    public string LabelBtnHarmonicOrbs    => _rosetta.GetText(RbFile.RadixAnalysis, "analysis.btn.harmonicorbs");
    public string LabelBtnDeclinations    => _rosetta.GetText(RbFile.RadixAnalysis, "analysis.btn.declinations");
    public string LabelBtnZodiacDivisions => _rosetta.GetText(RbFile.RadixAnalysis, "analysis.btn.zodiacdivisions");
    public string LabelBtnEnneagram       => _rosetta.GetText(RbFile.RadixAnalysis, "analysis.btn.enneagram");
    public string LabelBtnVsp             => _rosetta.GetText(RbFile.RadixAnalysis, "analysis.btn.vsp");
    public string LabelBtnFixStars        => _rosetta.GetText(RbFile.RadixAnalysis, "analysis.btn.fixstars");
    public string LabelBtnParans          => _rosetta.GetText(RbFile.RadixAnalysis, "analysis.btn.parans");
    public string LabelBtnCountings       => _rosetta.GetText(RbFile.RadixAnalysis, "analysis.btn.countings");

    private void OpenAspects()          => _navigationService.NavigateDetail(AppRoutes.RadixAspects);
    private void OpenBlaSchema()        => _navigationService.NavigateDetail(AppRoutes.RadixBlaSchema);
    private void OpenMidpoints()        => _navigationService.NavigateDetail(AppRoutes.RadixMidpoints);
    private void OpenHarmonics()        => _navigationService.NavigateDetail(AppRoutes.RadixHarmonics);

    private void OpenHarmonicOrbs()
    {
        _navigationService.NavigateMain(AppRoutes.RadixHarmonicOrbsInput);
        _navigationService.NavigateDetail(AppRoutes.RadixHarmonicOrbs);
    }

    private void OpenDeclinations()     => _navigationService.NavigateDetail(AppRoutes.RadixDeclinations);
    private void OpenZodiacDivisions()
    {
        _navigationService.NavigateMain(AppRoutes.RadixZodiacDivisions);
        _navigationService.NavigateDetail(AppRoutes.RadixZodiacDivisionsInput);
    }

    private void OpenEnneagram()
    {
        _navigationService.NavigateMain(AppRoutes.RadixEnneagram);
        _navigationService.NavigateDetail(AppRoutes.RadixEnneagramOptions);
    }

    private void OpenVsp()
    {
        _navigationService.NavigateMain(AppRoutes.RadixVsp);
        _navigationService.NavigateDetail(AppRoutes.RadixVspDetail);
    }

    private void OpenFixStars()
    {
        _navigationService.NavigateMain(AppRoutes.RadixFixStarsInput);
        _navigationService.NavigateDetail(AppRoutes.RadixFixStars);
    }

    private void OpenParans()
    {
        _navigationService.NavigateMain(AppRoutes.RadixParansInput);
        _navigationService.NavigateDetail(AppRoutes.RadixParans);
    }

    private void OpenCountings() => _navigationService.NavigateDetail(AppRoutes.RadixCountings);
}
