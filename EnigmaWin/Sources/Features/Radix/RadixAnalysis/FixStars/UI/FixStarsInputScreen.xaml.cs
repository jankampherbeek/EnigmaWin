// FixStarsInputScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.FixStars.UI;

public partial class FixStarsInputScreen : UserControl
{
    public FixStarsInputScreen()
    {
        InitializeComponent();
    }

    private void OnCalculateClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is FixStarsViewModel vm)
            vm.Calculate();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new FixStarsHelpWindow(rosetta, "view.fixstar.help.input") { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
