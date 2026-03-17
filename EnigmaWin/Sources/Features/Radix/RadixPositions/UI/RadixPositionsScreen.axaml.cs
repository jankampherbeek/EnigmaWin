using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Radix.RadixPositions.UI;

public partial class RadixPositionsScreen : UserControl
{
    private const double CompactWidthThreshold = 700;
    private readonly RadixPositionsViewModel _viewModel;
    private IChartContext? _chartContext;

    public RadixPositionsScreen()
    {
        var rosetta = (Application.Current as App)?.Services.GetRequiredService<IRosetta>()
                      ?? throw new InvalidOperationException("IRosetta not available");
        _viewModel = new RadixPositionsViewModel(rosetta);

        InitializeComponent();
        DataContext = _viewModel;

        ResolveChartContext();
        if (_chartContext != null)
        {
            _viewModel.LoadChart(_chartContext.CurrentChart);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ResolveChartContext();

        if (_chartContext is INotifyPropertyChanged notifyChartContext)
        {
            notifyChartContext.PropertyChanged += OnChartContextPropertyChanged;
        }

        if (_chartContext != null)
        {
            _viewModel.LoadChart(_chartContext.CurrentChart);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_chartContext is INotifyPropertyChanged notifyChartContext)
        {
            notifyChartContext.PropertyChanged -= OnChartContextPropertyChanged;
        }

        base.OnDetachedFromVisualTree(e);
    }

    private void OnChartContextPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IChartContext.CurrentChart) || _chartContext == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() => _viewModel.LoadChart(_chartContext.CurrentChart));
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        _viewModel.IsWideLayout = e.NewSize.Width >= CompactWidthThreshold;
    }

    private async void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app) return;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var helpWindow = new RadixPositionsHelpWindow(rosetta);
        if (TopLevel.GetTopLevel(this) is Window owner)
            await helpWindow.ShowDialog(owner);
    }

    private void ResolveChartContext()
    {
        if (_chartContext != null || Avalonia.Controls.Design.IsDesignMode)
        {
            return;
        }

        if (Application.Current is App app)
        {
            _chartContext = app.Services.GetService<IChartContext>();
        }
    }
}
