// ConfigEditScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.AppShell.Navigation;

namespace EnigmaWin.Sources.Features.Config.UI;

public partial class ConfigEditScreen : UserControl
{
    private ConfigEditViewModel? _vm;

    public ConfigEditScreen()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        _vm = DataContext as ConfigEditViewModel;
    }

    private async void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is not null) await _vm.SaveAsync();
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e) => _vm?.Revert();

    private async void OnSetActiveClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is not null) await _vm.SetActiveAsync();
    }

    private void OnCalcClicked(object sender, RoutedEventArgs e)         => _vm?.NavigateTo(AppRoutes.ConfigSectionCalc);
    private void OnDisplayClicked(object sender, RoutedEventArgs e)      => _vm?.NavigateTo(AppRoutes.ConfigSectionDisplay);
    private void OnGlyphsClicked(object sender, RoutedEventArgs e)       => _vm?.NavigateTo(AppRoutes.ConfigSectionGlyphs);
    private void OnFactorsClicked(object sender, RoutedEventArgs e)      => _vm?.NavigateTo(AppRoutes.ConfigSectionFactors);
    private void OnAspectsClicked(object sender, RoutedEventArgs e)      => _vm?.NavigateTo(AppRoutes.ConfigSectionAspects);
    private void OnOrbsClicked(object sender, RoutedEventArgs e)         => _vm?.NavigateTo(AppRoutes.ConfigSectionOrbs);
    private void OnProgressionsClicked(object sender, RoutedEventArgs e) => _vm?.NavigateTo(AppRoutes.ConfigSectionProgressions);
    private void OnFixStarsClicked(object sender, RoutedEventArgs e)     => _vm?.NavigateTo(AppRoutes.ConfigSectionFixStars);

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        var dialog = new Window
        {
            Title = _vm.LabelHelpTitle,
            Width = 480,
            Height = 280,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Window.GetWindow(this)
        };
        var panel = new StackPanel { Margin = new Thickness(16) };
        panel.Children.Add(new TextBlock { Text = _vm.LabelHelpLine1, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 8) });
        panel.Children.Add(new TextBlock { Text = _vm.LabelHelpLine2, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 12) });
        var closeBtn = new Button { Content = _vm.LabelHelpClose, HorizontalAlignment = HorizontalAlignment.Right };
        closeBtn.Click += (_, _) => dialog.Close();
        panel.Children.Add(closeBtn);
        dialog.Content = panel;
        dialog.ShowDialog();
    }
}
