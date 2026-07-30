// EclipsesInputScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Periods.Eclipses.UI;

public partial class EclipsesInputScreen : UserControl
{
    public EclipsesInputScreen() => InitializeComponent();

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new EclipsesHelpWindow(rosetta, "view.eclipses.help") { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
