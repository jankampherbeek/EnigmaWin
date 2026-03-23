// ConfigDisplaySectionScreen.axaml.cs
// EnigmaWin
// Created by Jan Kampherbeek on 22-03-2026

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace EnigmaWin.Sources.Features.Config.UI;

public partial class ConfigDisplaySectionScreen : UserControl
{
    private ConfigDisplaySectionViewModel? _vm;

    public ConfigDisplaySectionScreen()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        _vm = DataContext as ConfigDisplaySectionViewModel;
    }

    private void OnBackClicked(object? sender, RoutedEventArgs e) => _vm?.GoBack();

    private async void OnSaveClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is not null)
            await _vm.SaveAsync();
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e) => _vm?.Revert();

    // 48-color palette: 6 rows × 8 columns (grays → dark → medium → bright → light → pastel)
    private static readonly string[] ColorPalette =
    [
        "#000000","#222222","#444444","#666666","#888888","#AAAAAA","#CCCCCC","#FFFFFF",
        "#800000","#804000","#808000","#008000","#008080","#000080","#400080","#800040",
        "#C00000","#C05000","#C0C000","#00A000","#00A0A0","#0000C0","#6000C0","#C000A0",
        "#FF0000","#FF8000","#FFFF00","#00CC00","#00CCCC","#0000FF","#8000FF","#FF00CC",
        "#FF8080","#FFB080","#FFFF80","#80FF80","#80FFFF","#8080FF","#C080FF","#FF80CC",
        "#FFCCCC","#FFE0CC","#FFFFCC","#CCFFCC","#CCFFFF","#CCCCFF","#E0CCFF","#FFCCEE",
    ];

    private async void OnColorSwatchClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: SignColorRowViewModel row }) return;
        if (TopLevel.GetTopLevel(this) is not Window owner) return;

        var selectedColor = row.CurrentColor;

        // Preview bar
        var preview = new Border
        {
            Height       = 44,
            CornerRadius = new CornerRadius(6),
            Background   = new Avalonia.Media.SolidColorBrush(selectedColor),
            Margin       = new Thickness(0, 0, 0, 8)
        };

        // Hex input
        var hexBox = new TextBox
        {
            Text        = $"#{selectedColor.R:X2}{selectedColor.G:X2}{selectedColor.B:X2}",
            Watermark   = "#RRGGBB",
            Margin      = new Thickness(0, 8, 0, 0)
        };

        Border? highlightedSwatch = null;

        void SelectColor(Avalonia.Media.Color c, Border? swatchBorder)
        {
            selectedColor      = c;
            preview.Background = new Avalonia.Media.SolidColorBrush(c);
            hexBox.Text        = $"#{c.R:X2}{c.G:X2}{c.B:X2}";

            if (highlightedSwatch is not null)
            {
                highlightedSwatch.BorderThickness = new Thickness(0);
                highlightedSwatch.BorderBrush     = null;
            }
            highlightedSwatch = swatchBorder;
            if (swatchBorder is not null)
            {
                swatchBorder.BorderThickness = new Thickness(2);
                swatchBorder.BorderBrush     = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black);
            }
        }

        // Build palette (6 rows × 8 cols)
        var palettePanel = new StackPanel { Spacing = 2 };
        for (int rowIdx = 0; rowIdx < 6; rowIdx++)
        {
            var rowPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
            for (int colIdx = 0; colIdx < 8; colIdx++)
            {
                var paletteColor = Avalonia.Media.Color.Parse(ColorPalette[rowIdx * 8 + colIdx]);
                var swatchBorder = new Border
                {
                    Width        = 28,
                    Height       = 28,
                    CornerRadius = new CornerRadius(3),
                    Background   = new Avalonia.Media.SolidColorBrush(paletteColor)
                };
                var swatchBtn = new Button { Content = swatchBorder, Padding = new Thickness(1), MinWidth = 0, MinHeight = 0 };
                swatchBtn.Click += (_, _) => SelectColor(paletteColor, swatchBorder);
                rowPanel.Children.Add(swatchBtn);
            }
            palettePanel.Children.Add(rowPanel);
        }

        // Hex input → live update
        hexBox.TextChanged += (_, _) =>
        {
            var text = hexBox.Text?.Trim() ?? "";
            if (!text.StartsWith('#')) text = "#" + text;
            if (Avalonia.Media.Color.TryParse(text, out var parsed))
                SelectColor(parsed, null);
        };

        var okBtn     = new Button { Content = "OK" };
        var cancelBtn = new Button { Content = "Annuleer" };
        var btnRow    = new StackPanel
        {
            Orientation         = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin              = new Thickness(0, 8, 0, 0),
            Spacing             = 8
        };
        btnRow.Children.Add(cancelBtn);
        btnRow.Children.Add(okBtn);

        var content = new StackPanel { Margin = new Thickness(16), Spacing = 4 };
        content.Children.Add(preview);
        content.Children.Add(palettePanel);
        content.Children.Add(hexBox);
        content.Children.Add(btnRow);

        bool confirmed = false;
        var dialog = new Window
        {
            Title                 = row.SignName,
            Width                 = 310,
            SizeToContent         = SizeToContent.Height,
            CanResize             = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content               = content
        };

        okBtn.Click     += (_, _) => { confirmed = true; dialog.Close(); };
        cancelBtn.Click += (_, _) => dialog.Close();

        await dialog.ShowDialog(owner);

        if (confirmed)
            row.CurrentColor = selectedColor;
    }

    private async void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;

        var dialog = new Window
        {
            Title     = _vm.LabelHelpTitle,
            Width     = 480,
            Height    = 300,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var closeBtn = new Button { Content = _vm.LabelHelpClose, HorizontalAlignment = HorizontalAlignment.Right };
        closeBtn.Click += (_, _) => dialog.Close();

        var panel = new StackPanel { Margin = new Thickness(16), Spacing = 8 };
        foreach (var line in new[] { _vm.LabelHelpLine1, _vm.LabelHelpLine2, _vm.LabelHelpLine3, _vm.LabelHelpLine4 })
            panel.Children.Add(new TextBlock { Text = line, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        panel.Children.Add(closeBtn);
        dialog.Content = panel;

        if (TopLevel.GetTopLevel(this) is Window owner)
            await dialog.ShowDialog(owner);
    }
}
