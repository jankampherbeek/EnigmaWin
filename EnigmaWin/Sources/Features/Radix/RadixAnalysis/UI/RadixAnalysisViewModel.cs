// RadixAnalysisViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.Navigation;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.UI;

public sealed class RadixAnalysisViewModel
{
    private readonly INavigationService _navigationService;

    public IRelayCommand ShowAspectsCommand { get; }
    public IRelayCommand ShowMidpointsCommand { get; }

    public RadixAnalysisViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        ShowAspectsCommand = new RelayCommand(OpenAspects);
        ShowMidpointsCommand = new RelayCommand(OpenMidpoints);
    }

    private void OpenAspects() => _navigationService.NavigateDetail(AppRoutes.RadixAspects);
    private void OpenMidpoints() => _navigationService.NavigateDetail(AppRoutes.RadixMidpoints);
}
