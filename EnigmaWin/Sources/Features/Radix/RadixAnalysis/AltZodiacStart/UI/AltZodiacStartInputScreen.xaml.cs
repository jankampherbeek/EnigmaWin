// AltZodiacStartInputScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.AltZodiacStart.UI;

public partial class AltZodiacStartInputScreen : UserControl
{
    public AltZodiacStartInputScreen()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is AltZodiacStartViewModel vm)
                vm.Refresh();
        };
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new AltZodiacStartHelpWindow(rosetta, "altzodiacstart.input.help") { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
