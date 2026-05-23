// ConfigPrimaryDirectionsSectionScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;

namespace EnigmaWin.Sources.Features.Config.UI;

public partial class ConfigPrimaryDirectionsSectionScreen : UserControl
{
    private ConfigPrimaryDirectionsSectionViewModel? _vm;

    public ConfigPrimaryDirectionsSectionScreen()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        _vm = DataContext as ConfigPrimaryDirectionsSectionViewModel;
    }

    private void OnBackClicked(object sender, RoutedEventArgs e)   => _vm?.GoBack();
    private void OnCancelClicked(object sender, RoutedEventArgs e) => _vm?.Revert();

    private async void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is not null) await _vm.SaveAsync();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        var dialog = new Window
        {
            Title = _vm.LabelHelpTitle, Width = 420,
            SizeToContent = SizeToContent.Height,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Window.GetWindow(this)
        };
        var panel = new StackPanel { Margin = new Thickness(16) };
        foreach (var line in new[] { _vm.LabelHelpLine1, _vm.LabelHelpLine2, _vm.LabelHelpLine3 })
            panel.Children.Add(new TextBlock { Text = line, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 6) });
        var closeBtn = new Button { Content = _vm.LabelHelpClose, HorizontalAlignment = HorizontalAlignment.Center };
        closeBtn.Click += (_, _) => dialog.Close();
        panel.Children.Add(closeBtn);
        dialog.Content = panel;
        dialog.ShowDialog();
    }
}
