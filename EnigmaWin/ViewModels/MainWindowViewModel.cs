using EnigmaWin.Sources.Features.Localization;

namespace EnigmaWin.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";
    public string Welcome { get; } = Rosetta.GetText("welcome");
}