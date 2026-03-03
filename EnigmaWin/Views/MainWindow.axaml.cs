using Avalonia.Controls;
using EnigmaWin.ViewModels;

namespace EnigmaWin.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        RadixInputScreenControl.CalculationCompleted += chart =>
        {
            if (DataContext is MainWindowViewModel mainWindowViewModel)
            {
                mainWindowViewModel.OpenRadixPositions(chart);
            }
        };
    }
}
