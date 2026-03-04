using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using EnigmaWin.Sources.AppShell.State;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace EnigmaWin.Sources.Features.Radix.RadixPositions.UI;

public partial class RadixPositionsScreen : UserControl
{
    private readonly RadixPositionsViewModel _viewModel = new();
    private IChartContext? _chartContext;

    public RadixPositionsScreen()
    {
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
