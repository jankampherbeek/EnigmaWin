using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.Features.Localization;

namespace EnigmaWin.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";
    public string Welcome { get; } = Rosetta.GetText("welcome");
    public IRelayCommand SelectRadixCommand { get; }
    public IRelayCommand SelectConfigurationCommand { get; }
    public IRelayCommand SelectResearchCommand { get; }
    public IRelayCommand NewChartCommand { get; }

    public MainWindowViewModel()
    {
        SelectRadixCommand = new RelayCommand(SelectRadix);
        SelectConfigurationCommand = new RelayCommand(SelectConfiguration);
        SelectResearchCommand = new RelayCommand(SelectResearch);
        NewChartCommand = new RelayCommand(OpenNewChart);
    }

    [ObservableProperty]
    private string activeSection = "Radix";

    [ObservableProperty]
    private bool showRadixInputScreen;

    public bool ShowRadixButtons => ActiveSection == "Radix";
    public bool ShowConfigurationButtons => ActiveSection == "Configuration";
    public bool ShowResearchButtons => ActiveSection == "Research";
    public bool ShowView2Placeholder => !ShowRadixInputScreen;

    private void SelectRadix()
    {
        SetActiveSection("Radix");
    }

    private void SelectConfiguration()
    {
        SetActiveSection("Configuration");
    }

    private void SelectResearch()
    {
        SetActiveSection("Research");
    }

    private void OpenNewChart()
    {
        if (ActiveSection != "Radix")
        {
            return;
        }

        ShowRadixInputScreen = true;
    }

    partial void OnActiveSectionChanged(string value)
    {
        OnPropertyChanged(nameof(ShowRadixButtons));
        OnPropertyChanged(nameof(ShowConfigurationButtons));
        OnPropertyChanged(nameof(ShowResearchButtons));
        ShowRadixInputScreen = false;
    }

    partial void OnShowRadixInputScreenChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowView2Placeholder));
    }

    private void SetActiveSection(string section)
    {
        if (ActiveSection == section)
        {
            return;
        }

        ActiveSection = section;
    }
}
