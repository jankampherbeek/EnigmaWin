// RadixAnalysisScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.UI;

public partial class RadixAnalysisScreen : UserControl
{
    public RadixAnalysisScreen()
    {
        var navigationService = (Application.Current as App)?.Services.GetRequiredService<INavigationService>()
            ?? throw new InvalidOperationException("INavigationService not available");
        var rosetta = (Application.Current as App)?.Services.GetRequiredService<IRosetta>()
            ?? throw new InvalidOperationException("IRosetta not available");

        DataContext = new RadixAnalysisViewModel(navigationService, rosetta);
        InitializeComponent();
    }
}
