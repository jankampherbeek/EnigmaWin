using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;

namespace EnigmaWin.Sources.Features.Radix.RadixInput.UI;

public partial class RadixInputScreen : UserControl
{
    private enum InputSection { About, Location, DateTime }

    private bool _isUpdatingExpanders;
    private InputSection _activeSection = InputSection.About;
    private RadixInputViewModel? _vm;

    public RadixInputScreen()
    {
        InitializeComponent();

        if (Design.IsDesignMode || Application.Current is not App app)
            return;

        var navigationService = app.Services.GetRequiredService<INavigationService>();
        var chartContext = app.Services.GetRequiredService<IChartContext>();
        _vm = new RadixInputViewModel(new RadixInputModel(), navigationService, chartContext);
        DataContext = _vm;
    }

    private void OnSectionExpanded(object? sender, RoutedEventArgs e)
    {
        if (_isUpdatingExpanders || sender is not Expander expanded || _vm is null)
            return;

        var newSection = ReferenceEquals(AboutSection, expanded) ? InputSection.About
            : ReferenceEquals(LocationSection, expanded) ? InputSection.Location
            : InputSection.DateTime;

        if (newSection != _activeSection)
        {
            // Force-validate the section we are leaving so errors are up to date.
            switch (_activeSection)
            {
                case InputSection.About: _vm.ValidateAbout(); break;
                case InputSection.DateTime: _vm.ValidateDateTime(); break;
            }

            var hasError = _activeSection switch
            {
                InputSection.About => _vm.HasAboutSectionError,
                InputSection.DateTime => _vm.HasDateTimeSectionError,
                _ => false
            };

            if (hasError)
            {
                // Block the navigation and snap back to the current section.
                _isUpdatingExpanders = true;
                try
                {
                    expanded.IsExpanded = false;
                    ExpanderFor(_activeSection).IsExpanded = true;
                }
                finally
                {
                    _isUpdatingExpanders = false;
                }
                return;
            }

            _activeSection = newSection;
        }

        // Collapse all other sections.
        _isUpdatingExpanders = true;
        try
        {
            if (!ReferenceEquals(AboutSection, expanded)) AboutSection.IsExpanded = false;
            if (!ReferenceEquals(LocationSection, expanded)) LocationSection.IsExpanded = false;
            if (!ReferenceEquals(DateTimeSection, expanded)) DateTimeSection.IsExpanded = false;
        }
        finally
        {
            _isUpdatingExpanders = false;
        }
    }

    private void OnSectionGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is not Expander section || _isUpdatingExpanders || section.IsExpanded)
            return;

        section.IsExpanded = true;
    }

    private void OnCalculateClicked(object? sender, RoutedEventArgs e)
    {
        _vm?.Calculate();
    }

    private void OnClearClicked(object? sender, RoutedEventArgs e)
    {
        _vm?.Clear();

        _activeSection = InputSection.About;
        _isUpdatingExpanders = true;
        try
        {
            AboutSection.IsExpanded = true;
            LocationSection.IsExpanded = false;
            DateTimeSection.IsExpanded = false;
        }
        finally
        {
            _isUpdatingExpanders = false;
        }
    }

    private Expander ExpanderFor(InputSection section) => section switch
    {
        InputSection.About => AboutSection,
        InputSection.Location => LocationSection,
        _ => DateTimeSection
    };
}
