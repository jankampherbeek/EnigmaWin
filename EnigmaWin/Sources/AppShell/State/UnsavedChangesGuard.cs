// UnsavedChangesGuard.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.AppShell.State;

/// <summary>A screen that edits data and can have unsaved changes, such as a configuration section.</summary>
public interface IUnsavedChangesEditor
{
    bool IsDirty { get; }
    Task SaveAsync();
    void Revert();
}

/// <summary>
/// Holds back navigation that would leave a screen with unsaved changes until the user chooses to
/// save, discard or stay. Mirrors UnsavedChangesGuard in the Apple version.
/// </summary>
public interface IUnsavedChangesGuard
{
    /// <summary>Sets the screen that is currently shown; it is guarded when it is an <see cref="IUnsavedChangesEditor"/>.</summary>
    void Track(object? screenViewModel);

    bool HasUnsavedChanges { get; }

    /// <summary>Runs <paramref name="action"/> immediately when there are no unsaved changes; otherwise asks the
    /// user first. Save and discard continue with the action, cancel drops it and runs <paramref name="onCancel"/>.</summary>
    void Perform(Action action, Action? onCancel = null);
}

/// <summary>The user's answer to the unsaved-changes question.</summary>
public enum UnsavedChangesChoice { Save, Discard, Cancel }

public sealed class UnsavedChangesGuard : IUnsavedChangesGuard
{
    private readonly IRosetta? _rosetta;
    private readonly Func<UnsavedChangesChoice> _ask;

    private IUnsavedChangesEditor? _editor;
    private bool _asking;
    // Editor the user just decided about. A single user action often navigates more than once (main and
    // detail); the decision covers all of them, until the current UI operation has finished.
    private IUnsavedChangesEditor? _decided;

    /// <summary>Asks the user with a dialog.</summary>
    public UnsavedChangesGuard(IRosetta rosetta)
    {
        _rosetta = rosetta;
        _ask     = AskWithDialog;
    }

    /// <summary>Asks the user with the given function instead of a dialog (used by tests).</summary>
    public UnsavedChangesGuard(Func<UnsavedChangesChoice> ask) => _ask = ask;

    public void Track(object? screenViewModel) => _editor = screenViewModel as IUnsavedChangesEditor;

    public bool HasUnsavedChanges => _editor?.IsDirty == true;

    public void Perform(Action action, Action? onCancel = null)
    {
        var editor = _editor;
        if (editor is null || !editor.IsDirty || _asking || ReferenceEquals(editor, _decided))
        {
            action();
            return;
        }

        _asking = true;
        UnsavedChangesChoice choice;
        try { choice = _ask(); }
        finally { _asking = false; }

        if (choice == UnsavedChangesChoice.Cancel)
        {
            onCancel?.Invoke();
            return;
        }

        _decided = editor;
        Application.Current?.Dispatcher.BeginInvoke(() => _decided = null,
            System.Windows.Threading.DispatcherPriority.Background);

        if (choice == UnsavedChangesChoice.Save)
        {
            _ = SaveAndContinueAsync(editor, action);
            return;
        }
        editor.Revert();
        action();
    }

    private static async Task SaveAndContinueAsync(IUnsavedChangesEditor editor, Action action)
    {
        await editor.SaveAsync();
        action();
    }

    private UnsavedChangesChoice AskWithDialog()
    {
        string T(string key) => _rosetta!.GetText(RbFile.ConfigEdit, key);

        var choice = UnsavedChangesChoice.Cancel;
        var dialog = new Window
        {
            Title                 = T("view.configedit.unsaved.title"),
            Width                 = 420,
            SizeToContent         = SizeToContent.Height,
            ResizeMode            = ResizeMode.NoResize,
            ShowInTaskbar         = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner                 = Application.Current?.MainWindow
        };

        Button MakeButton(string key, UnsavedChangesChoice result, bool isDefault = false, bool isCancel = false)
        {
            var button = new Button
            {
                Content   = T(key),
                MinWidth  = 90,
                Margin    = new Thickness(8, 0, 0, 0),
                Padding   = new Thickness(10, 3, 10, 3),
                IsDefault = isDefault,
                IsCancel  = isCancel
            };
            button.Click += (_, _) => { choice = result; dialog.Close(); };
            return button;
        }

        var buttons = new StackPanel
        {
            Orientation         = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin              = new Thickness(0, 16, 0, 0)
        };
        buttons.Children.Add(MakeButton("view.configedit.edit.save",       UnsavedChangesChoice.Save, isDefault: true));
        buttons.Children.Add(MakeButton("view.configedit.unsaved.discard", UnsavedChangesChoice.Discard));
        buttons.Children.Add(MakeButton("view.configedit.cancel",          UnsavedChangesChoice.Cancel, isCancel: true));

        var panel = new StackPanel { Margin = new Thickness(16) };
        panel.Children.Add(new TextBlock { Text = T("view.configedit.unsaved.message"), TextWrapping = TextWrapping.Wrap });
        panel.Children.Add(buttons);
        dialog.Content = panel;

        dialog.ShowDialog();
        return choice;
    }
}
