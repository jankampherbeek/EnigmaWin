// ParansInputScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Parans.UI;

public partial class ParansInputScreen : UserControl
{
    public ParansInputScreen()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is ParansInputViewModel vm)
                vm.SharedVm.Calculate();
        };
    }

    private void OnCalculateClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is ParansInputViewModel vm)
            vm.SharedVm.Calculate();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new ParansHelpWindow(rosetta, "view.parans.help.input") { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
