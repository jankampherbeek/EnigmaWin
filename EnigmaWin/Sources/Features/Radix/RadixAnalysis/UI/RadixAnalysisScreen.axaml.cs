// RadixAnalysisScreen.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia;
using Avalonia.Controls;
using EnigmaWin.Sources.AppShell.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.UI;

public partial class RadixAnalysisScreen : UserControl
{
    public RadixAnalysisScreen()
    {
        var navigationService = (Application.Current as App)?.Services.GetRequiredService<INavigationService>()
            ?? throw new System.InvalidOperationException("INavigationService not available");

        DataContext = new RadixAnalysisViewModel(navigationService);
        InitializeComponent();
    }
}
