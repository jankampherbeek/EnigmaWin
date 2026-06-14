// EnneagramScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Enneagram.UI;

public partial class EnneagramScreen : UserControl
{
    public EnneagramScreen()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TheCanvas.CircleClicked = ShowTypePopup;
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new EnneagramFactsheetWindow(rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new EnneagramHelpWindow(rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnTypeRowClicked(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement fe && fe.DataContext is EnneagramTypeRow row)
            ShowTypePopup(row.Type);
    }

    private void ShowTypePopup(int type)
    {
        if (DataContext is not EnneagramViewModel vm) return;
        var popup = new EnneagramTypePopupWindow(vm.GetTypeName(type), vm.GetTypeDescription(type))
        {
            Owner = Window.GetWindow(this)
        };
        popup.ShowDialog();
    }
}
