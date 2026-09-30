// GlyphOverviewWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;

namespace EnigmaWin.Sources.Features.GlyphOverView.UI;

public partial class GlyphOverviewWindow : Window
{
    public GlyphOverviewWindow(GlyphOverviewViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
