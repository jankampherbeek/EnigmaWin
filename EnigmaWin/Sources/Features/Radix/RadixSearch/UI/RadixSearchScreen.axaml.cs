using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Microsoft.Extensions.DependencyInjection;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixSearch.UI;

public partial class RadixSearchScreen : UserControl
{
    private RadixSearchViewModel? _vm;

    public RadixSearchScreen()
    {
        InitializeComponent();

        if (Design.IsDesignMode || Application.Current is not App app)
            return;

        var repository         = app.Services.GetRequiredService<IHoroscopeRepository>();
        var navigationService  = app.Services.GetRequiredService<INavigationService>();
        var chartContext        = app.Services.GetRequiredService<IChartContext>();
        var rosetta            = app.Services.GetRequiredService<IRosetta>();

        _vm = new RadixSearchViewModel(repository, navigationService, chartContext, rosetta);
        DataContext = _vm;
    }

    private async void OnSearchClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is not null)
            await _vm.SearchAsync();
    }

    private void OnSelectClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is not null && sender is Button { DataContext: HoroscopeSearchRow row })
            _vm.Select(row);
    }

    private async void OnDeleteClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is null || sender is not Button { DataContext: HoroscopeSearchRow row })
            return;

        var confirmed = await ShowDeleteConfirmAsync(row.Name);
        if (confirmed)
            await _vm.DeleteAsync(row);
    }

    private async void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app) return;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var helpWindow = new RadixSearchHelpWindow(rosetta);
        if (TopLevel.GetTopLevel(this) is Window owner)
            await helpWindow.ShowDialog(owner);
    }

    private async Task<bool> ShowDeleteConfirmAsync(string horoscopeName)
    {
        if (_vm is null) return false;

        var tcs = new TaskCompletionSource<bool>();

        var dialog = new Window
        {
            Title  = _vm.LabelDeleteTitle,
            Width  = 400,
            Height = 170,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var message = new TextBlock
        {
            Text = string.Format(_vm.LabelDeleteMessage.Replace("%s", "{0}"), horoscopeName),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };

        var confirmBtn = new Button { Content = _vm.LabelDeleteConfirm };
        var cancelBtn  = new Button { Content = _vm.LabelDeleteCancel };

        confirmBtn.Click += (_, _) => { tcs.TrySetResult(true);  dialog.Close(); };
        cancelBtn.Click  += (_, _) => { tcs.TrySetResult(false); dialog.Close(); };
        dialog.Closed    += (_, _) =>   tcs.TrySetResult(false);

        var buttons = new StackPanel
        {
            Orientation         = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing             = 8
        };
        buttons.Children.Add(confirmBtn);
        buttons.Children.Add(cancelBtn);

        var panel = new StackPanel { Margin = new Thickness(16), Spacing = 16 };
        panel.Children.Add(message);
        panel.Children.Add(buttons);
        dialog.Content = panel;

        if (TopLevel.GetTopLevel(this) is Window owner)
            await dialog.ShowDialog(owner);

        return await tcs.Task;
    }
}
