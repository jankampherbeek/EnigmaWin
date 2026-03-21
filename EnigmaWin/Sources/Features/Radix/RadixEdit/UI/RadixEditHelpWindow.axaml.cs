using Avalonia.Controls;
using Avalonia.Interactivity;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixEdit.UI;

public partial class RadixEditHelpWindow : Window
{
    public string HelpText   { get; }
    public string LabelClose { get; }

    public RadixEditHelpWindow(IRosetta rosetta)
    {
        HelpText   = rosetta.GetText(RbFile.RadixEdit, "view.radixeditscreen.help.text");
        LabelClose = rosetta.GetText(RbFile.RadixEdit, "view.radixeditscreen.help.close");

        Title = rosetta.GetText(RbFile.RadixEdit, "view.radixeditscreen.help.title");

        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e) => Close();
}
