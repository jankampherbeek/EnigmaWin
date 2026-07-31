// LongTimeEphemerisInputScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Periods.LongTimeEphemeris.UI;

public partial class LongTimeEphemerisInputScreen : UserControl
{
    public LongTimeEphemerisInputScreen() => InitializeComponent();

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new LongTimeEphemerisHelpWindow(rosetta, "view.ltephemeris.help") { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
