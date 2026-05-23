// MainWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;

namespace EnigmaWin.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnAboutClicked(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("EnigmaWin\nAstrology application\n© Jan Kampherbeek 2026",
            "About EnigmaWin", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
